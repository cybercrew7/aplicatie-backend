using Microsoft.AspNetCore.Mvc;
using Shared.DTO;
using saxafon.Services;

namespace saxafon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReservationController : ControllerBase
    {
        private readonly IReservationService reservationService;
        public ReservationController(IReservationService _reservationService)
        {
            reservationService = _reservationService;
        }


        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<ReservationFullDTO>>> getAllReservationApi() {
            var result = await  reservationService.GetAllReservations();
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ReservationFullDTO>> getReservationByIdApi(int id) {
            var result = await reservationService.GetReservationById(id);
            if (result == null) {
                return NotFound();
            }
            return Ok(result);
        }

        [HttpGet("resource/{resourceId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationFullDTO>>> getReservationByResourceIdApi(int resourceId)
        {
            var result = await reservationService.GetReservationByResourceId(resourceId);
            return Ok(result);
        }



        [HttpGet("user/{userId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationFullDTO>>> geAlltReservationByUserIdApi(int userId)
        {
            var result = await reservationService.GetAllReservationByUserId(userId);
            return Ok(result);
        }



        [HttpGet("current/{userId:int}")]
        public async Task<ActionResult<IEnumerable<ReservationFullDTO>>> GetCurrentReservationByUserIdApi(int userId)
        {
            var result = await reservationService.GetCurrentReservationByUserId(userId);
            return Ok(result);
        }

        [HttpPost("cancel")]
        public async Task<ActionResult<ReservationCancelResponseDTO>> CancelReservationApi(ReservationCancelDTO reservation) { 
            try
            {
                var result = await reservationService.CancelReservation(reservation);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("thisWeek")]
        public async Task<ActionResult<IEnumerable<ReservationViewDTO>>> ShowCurrentWeekReservationsApi([FromQuery] int resourceId)
        {
            var result = await reservationService.ShowCurrentWeekReservations(resourceId);
            return Ok(result);
        }
        

        [HttpPost("create")]
        public async Task<ActionResult<ReservationFullDTO>> CreateReservationApi(ReservationCreateDTO reservation) {
            try
            {
                var result = await reservationService.CreateReservation(reservation);
                return CreatedAtAction(nameof(getReservationByIdApi), new { id = result.id }, result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
        }

    }
}
