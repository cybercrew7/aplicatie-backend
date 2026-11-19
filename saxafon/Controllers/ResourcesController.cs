using Microsoft.AspNetCore.Mvc;
using Shared.DTO;
using saxafon.Models;
using saxafon.Services;

namespace saxafon.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ResourcesController : ControllerBase
    {

        private readonly IResourcesService resourcesService;
        public ResourcesController(IResourcesService _resourceService) {
            resourcesService = _resourceService;
        }

        [HttpGet("getAll")]
        public async Task<ActionResult<IEnumerable<ResourcesFullDTO>>> GetAllResourcesApi()
        {
            var resources = await resourcesService.GetAllResources();
            return Ok(resources);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ResourcesFullDTO>> GetResourceByIdApi(int id)
        {
            var resource = await resourcesService.GetResourcesById(id);
            if (resource == null)
            {
                return NotFound();
            }
            return Ok(resource);
        }

        [HttpGet("getAvailable")]
        public async Task<ActionResult<IEnumerable<ResourcesMinimalDTO>>> getAvaiableResorucesApi() { 
           var resources = await resourcesService.GetAllAvaiableResources(30);
            return Ok(resources);
        }

        [HttpPost("createroom")]
        public async Task<ActionResult<ResourcesMinimalDTO>> CreateResourceApi(ResourcesRoomCreateDTO resourceCreateDTO)
        {
            var resource = await resourcesService.CreateRoom(resourceCreateDTO);
            return CreatedAtAction(nameof(GetResourceByIdApi), new { id = resource.Id }, resource);
        }

        [HttpPost("createEquipment")]
        public async Task<ActionResult<ResourcesMinimalDTO>> CreateEquipmentApi(ResourcesEquipmentCreateDTO resourceCreateDTO)
        {
            var resource = await resourcesService.CreateEquipment(resourceCreateDTO);
            return CreatedAtAction(nameof(GetResourceByIdApi), new { id = resource.Id }, resource);
        }


        //add post to active inactive for a resource.


    }
}
