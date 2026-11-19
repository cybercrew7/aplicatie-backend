using Shared.DTO;

namespace IPFront.Service
{
    public static class ResourceDisplayHelper
    {
        public static bool IsRoom(ResourcesFullDTO resource)
            => string.IsNullOrWhiteSpace(resource.type)
               && (resource.capacity.HasValue || !string.IsNullOrWhiteSpace(resource.buildingId));

        public static string GetCategory(ResourcesFullDTO resource)
            => !string.IsNullOrWhiteSpace(resource.type) ? resource.type : "Room";

        public static string GetCode(ResourcesFullDTO resource)
            => !string.IsNullOrWhiteSpace(resource.inventory_code)
                ? resource.inventory_code
                : $"RES-{resource.Id}";

        public static string GetLocation(
            ResourcesFullDTO resource,
            IReadOnlyDictionary<int, ResourcesFullDTO>? resourcesById = null)
        {
            if (!string.IsNullOrWhiteSpace(resource.buildingId))
            {
                return $"Building {resource.buildingId}";
            }

            if (resource.ParentId.HasValue
                && resourcesById is not null
                && resourcesById.TryGetValue(resource.ParentId.Value, out var parent))
            {
                var building = !string.IsNullOrWhiteSpace(parent.buildingId)
                    ? $"Building {parent.buildingId} · "
                    : string.Empty;
                return $"{building}{parent.resource_name}";
            }

            return "Campus Lab";
        }
    }
}
