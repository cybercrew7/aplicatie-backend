using saxafon.Data;
using Shared.DTO;
using Microsoft.EntityFrameworkCore;
using saxafon.Models;
using System.Linq.Expressions;


namespace saxafon.Services
{

    public interface IReservationService {
         Task<IEnumerable<ReservationFullDTO>> GetAllReservations();
         Task<ReservationFullDTO?> GetReservationById(int id);
         Task<IEnumerable<ReservationFullDTO>> GetReservationByResourceId(int resourceId);
         Task<IEnumerable<ReservationFullDTO>> GetAllReservationByUserId(int UserID);
        Task<IEnumerable<ReservationFullDTO>> GetCurrentReservationByUserId(int UserId);
        Task<ReservationCancelResponseDTO> CancelReservation(ReservationCancelDTO reservation);
        Task<IEnumerable<ReservationViewDTO>> ShowCurrentWeekReservations(int resoruceId);

        Task<ReservationFullDTO> CreateReservation(ReservationCreateDTO reservation);

    }


    public class ReservationService : IReservationService
    {

        private readonly AppDbContext context;
        public ReservationService(AppDbContext _context)
        {
            context = _context;
        }

        // Single projection used by every query, so new fields are mapped in one place.
        private static readonly Expression<Func<Reservations, ReservationFullDTO>> ToFullDTO = r => new ReservationFullDTO(
            r.Id,
            r.ResourceId,
            r.UserId,
            r.purpose,
            r.status_rezervare,
            r.start_time,
            r.end_time,
            r.created_at,
            r.canceled_at,
            r.expected_attendees,
            r.reviewed_at,
            r.rejection_or_cancel_reason
        );
        private static readonly Func<Reservations, ReservationFullDTO> ToFullDTOCompiled = ToFullDTO.Compile();

        public async Task<IEnumerable<ReservationFullDTO>> GetAllReservations()
        {
            return await context.Reservations.Select(ToFullDTO).ToListAsync();
        }

        public async Task<ReservationFullDTO?> GetReservationById(int id ) { 
            return await context.Reservations.Where(r => r.Id == id).Select(ToFullDTO).FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<ReservationFullDTO>> GetReservationByResourceId(int resourceId) { 
            return await context.Reservations.Where(r => r.ResourceId == resourceId).Select(ToFullDTO).ToListAsync();
        }

        public async Task<IEnumerable<ReservationFullDTO>> GetAllReservationByUserId(int UserID)
        {
            return await context.Reservations.Where(r => r.UserId == UserID).Select(ToFullDTO).ToListAsync();
        }

        public async Task<IEnumerable<ReservationFullDTO>> GetCurrentReservationByUserId(int UserId)
        {
            return await context.Reservations.Where(r => r.end_time > DateTime.UtcNow && r.UserId == UserId).Select(ToFullDTO).ToListAsync();
        }

        public async Task<ReservationCancelResponseDTO> CancelReservation(ReservationCancelDTO reservation) {

           
            var query = await context.Reservations.Where(r => r.Id == reservation.id && r.UserId == reservation.userId).FirstOrDefaultAsync();

            if (query == null)
            {
                throw new KeyNotFoundException("Reservation not found.");
            }

            query.status_rezervare = Shared.ReservationStatus.CANCELED;
            query.canceled_at = DateTime.UtcNow;
            await context.SaveChangesAsync();

            return new ReservationCancelResponseDTO(
                query.Id,
                query.canceled_at ?? DateTime.UtcNow
            );
        }


        public async Task<IEnumerable<ReservationViewDTO>> ShowCurrentWeekReservations(int resoruceId) {
            int days = (int)(DateTime.Today.DayOfWeek + 6) % 7;
            DateTime StartofWeek = DateTime.Today.AddDays(-days);
            DateTime EndnofWeek = StartofWeek.AddDays(7);

            var query = context.Reservations.Where(r => r.ResourceId == resoruceId && r.start_time >= StartofWeek && r.end_time <= EndnofWeek).Select(e => new ReservationViewDTO(
                e.Id,
                e.ResourceId,
                e.start_time,
                e.end_time
            ));

            return await query.ToListAsync();
        }

        public async Task<ReservationFullDTO> CreateReservation(ReservationCreateDTO reservation)
        {
            if (reservation.end_time <= reservation.start_time) { 
                throw new ArgumentException("End time must be greater than start time.");
            }

            var resource = await context.Resources.FirstOrDefaultAsync(r => r.Id == reservation.resourceId && r.deleted_at == null);
            if (resource == null)
            {
                throw new KeyNotFoundException("Resource not found.");
            }
            if (resource.status != Shared.Status.ACTIVE)
            {
                throw new InvalidOperationException("This resource is not available for booking.");
            }

            // Booking duration rules
            var durationMinutes = (reservation.end_time - reservation.start_time).TotalMinutes;
            if (resource.min_booking_minutes.HasValue && durationMinutes < resource.min_booking_minutes.Value)
            {
                throw new ArgumentException($"The minimum booking duration is {resource.min_booking_minutes} minutes.");
            }
            if (resource.max_booking_minutes.HasValue && durationMinutes > resource.max_booking_minutes.Value)
            {
                throw new ArgumentException($"The maximum booking duration is {resource.max_booking_minutes} minutes.");
            }

            // Expected attendees are validated against the room capacity
            if (reservation.expected_attendees.HasValue)
            {
                if (reservation.expected_attendees.Value < 1)
                {
                    throw new ArgumentException("Expected attendees must be at least 1.");
                }
                if (resource is Rooms room && room.capacity.HasValue && reservation.expected_attendees.Value > room.capacity.Value)
                {
                    throw new ArgumentException($"Expected attendees exceed the room capacity ({room.capacity}).");
                }
            }

            // Concurrent bookings on the same time frame
            var overlapping = await context.Reservations.CountAsync(r => r.ResourceId == reservation.resourceId
                && r.start_time < reservation.end_time
                && r.end_time > reservation.start_time
                && r.status_rezervare != Shared.ReservationStatus.CANCELED
                && r.status_rezervare != Shared.ReservationStatus.REJECTED);

            if (overlapping >= resource.max_concurrent_bookings) { 
                throw new InvalidOperationException("The reservation time overlaps with an existing reservation.");
            }

            // Weekly limit per user (week = Monday..Sunday of the reservation start)
            if (resource.max_bookings_per_user_per_week.HasValue)
            {
                int offset = ((int)reservation.start_time.DayOfWeek + 6) % 7;
                var weekStart = reservation.start_time.Date.AddDays(-offset);
                var weekEnd = weekStart.AddDays(7);

                var userBookingsThisWeek = await context.Reservations.CountAsync(r => r.ResourceId == reservation.resourceId
                    && r.UserId == reservation.userId
                    && r.start_time >= weekStart
                    && r.start_time < weekEnd
                    && r.status_rezervare != Shared.ReservationStatus.CANCELED
                    && r.status_rezervare != Shared.ReservationStatus.REJECTED);

                if (userBookingsThisWeek >= resource.max_bookings_per_user_per_week.Value)
                {
                    throw new InvalidOperationException($"You can book this resource at most {resource.max_bookings_per_user_per_week} times per week.");
                }
            }

            var query = context.Reservations.Add(new Reservations
            {

                Id = 0,
                ResourceId = reservation.resourceId,
                UserId = reservation.userId,
                purpose = reservation.prupose,
                status_rezervare = resource.is_approval_needed ? Shared.ReservationStatus.PENDING : Shared.ReservationStatus.CONFIRMED,
                start_time = reservation.start_time,
                end_time = reservation.end_time,
                expected_attendees = reservation.expected_attendees,
                created_at = DateTime.UtcNow,
                canceled_at = null
            });
            await context.SaveChangesAsync();

            return ToFullDTOCompiled(query.Entity);
        }
    }
}
