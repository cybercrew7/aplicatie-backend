using Shared.DTO;

namespace IPFront.Service
{
    /// <summary>Display helpers for resources (rooms and equipment).</summary>
    public static class ResourceDisplayHelper
    {
        /// <summary>True when the resource is a room rather than equipment.</summary>
        public static bool IsRoom(ResourcesFullDTO resource)
            => string.IsNullOrWhiteSpace(resource.type)
               && (resource.capacity.HasValue || !string.IsNullOrWhiteSpace(resource.buildingId));

        /// <summary>Category label: the equipment type, or "Room".</summary>
        public static string GetCategory(ResourcesFullDTO resource)
            => !string.IsNullOrWhiteSpace(resource.type) ? resource.type : "Room";

        /// <summary>Inventory code, or a fallback built from the id.</summary>
        public static string GetCode(ResourcesFullDTO resource)
            => !string.IsNullOrWhiteSpace(resource.inventory_code)
                ? resource.inventory_code
                : $"RES-{resource.Id}";

        /// <summary>Location label, resolved through the parent resource when <paramref name="resourcesById"/> is given.</summary>
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
