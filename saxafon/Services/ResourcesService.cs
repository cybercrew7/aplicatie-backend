using Microsoft.EntityFrameworkCore;
using saxafon.Data;
using Shared.DTO;
using saxafon.Models;

namespace saxafon.Services
{


    public interface IResourcesService
    {
        Task<IEnumerable<ResourcesFullDTO>> GetAllResources();
        Task<IEnumerable<ResourcesMinimalDTO>> GetAllAvaiableResources(int limit = -1);
        Task<ResourcesFullDTO?> GetResourcesById(int id);
        Task<ResourcesMinimalDTO> CreateRoom(ResourcesRoomCreateDTO roomCreateDTO);
        Task<ResourcesMinimalDTO> CreateEquipment(ResourcesEquipmentCreateDTO createEquipment);
    }
    public class ResourcesService : IResourcesService
    {


        private readonly AppDbContext context;

        public ResourcesService(AppDbContext _context)
        {
            context = _context;
        }


        private static ResourcesFullDTO MapToFullDTO(Resources r) {

            var eq = r as Equipments;
            var rm = r as Rooms;
            return new ResourcesFullDTO(
            r.Id,
            r.resource_name,
            r.ParentId,
            r.status,
            eq?.type,
            r.is_approval_needed,
            r.created_at,
            r.updated_at,
            r.deleted_at,
            eq?.equipment_model,
            eq?.description,
            eq?.serial_number,
            eq?.inventory_code,
            rm?.capacity,
            rm?.buildingId,
            r.image_url,
            r.max_concurrent_bookings,
            r.min_booking_minutes,
            r.max_booking_minutes,
            r.max_bookings_per_user_per_week
            );
        
        }

        private static ResourcesMinimalDTO MapToMinimalDTO(Resources r)
        {
            var eq = r as Equipments;
            var rm = r as Rooms;
            return new ResourcesMinimalDTO(
            r.Id,
            r.resource_name,
            r.ParentId,
            r.status,
            r.is_approval_needed,
            eq?.type,
            eq?.equipment_model,
            eq?.description,
            eq?.serial_number,
            eq?.inventory_code,
            rm?.capacity,
            rm?.buildingId,
            r.image_url
            );
        }   
        public async Task<IEnumerable<ResourcesFullDTO>> GetAllResources()
        {
            //ACTIVA and INACTIVE equipment 
    
            var entities =  await context.Resources.Where(e => e.deleted_at == null).ToListAsync();
            return entities.Select(MapToFullDTO);
        }

        public async Task<IEnumerable<ResourcesMinimalDTO>> GetAllAvaiableResources(int limit = -1 )
        {
            //only ACTIVE  equipment

            var query  =  context.Resources.Where(e => e.deleted_at == null &&  e.status ==  Shared.Status.ACTIVE);
            if (limit > 0)
            {
                query = query.Take(limit);
            }

            var entities = await query.ToListAsync();
            return entities.Select(MapToMinimalDTO);

        }

        public async Task<ResourcesFullDTO?> GetResourcesById(int id)
        {
            var resource = await context.Resources.FirstOrDefaultAsync(r => r.Id == id && r.deleted_at == null);
            return resource == null ? null : MapToFullDTO(resource);
        }

        public async Task<ResourcesMinimalDTO> CreateRoom(ResourcesRoomCreateDTO roomCreateDTO)
        { 
            var room = new Rooms
            {
                resource_name = roomCreateDTO.resource_name,
                status = roomCreateDTO.status,
                is_approval_needed = roomCreateDTO.is_approval_needed,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                capacity = roomCreateDTO.capacity,
                buildingId = roomCreateDTO.buildingId
            };
            context.Resources.Add(room);
            await context.SaveChangesAsync();
            return MapToMinimalDTO(room);
        }
        public async Task<ResourcesMinimalDTO> CreateEquipment(ResourcesEquipmentCreateDTO createEquipment ) {

            var equipment = new Equipments
            {
                resource_name = createEquipment.resource_name,
                ParentId = createEquipment.ParentId,
                status = createEquipment.status,
                is_approval_needed = createEquipment.is_approval_needed,
                type = createEquipment.type,
                equipment_model = createEquipment.equipment_model,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                description = createEquipment.description,
                serial_number = createEquipment.serial_number,
                inventory_code = createEquipment.inventory_code

            };

            context.Resources.Add(equipment);
            await context.SaveChangesAsync();
            return MapToMinimalDTO(equipment);
        }

    }
}
