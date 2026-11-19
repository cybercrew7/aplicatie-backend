using System.ComponentModel.DataAnnotations;

namespace Shared.DTO
{
    public record UserFullDTO(
         int Id,
         string first_name,
         string last_name,
         string email,
         string user_name,
         string? phone_number,
         Role role,
         string passwordHash,
         DateTime created_at,
         DateTime updated_at,
         DateTime last_time_active_at,
         DateTime? deleted_at
    );



      
    public record UserRegisterDTO(
       
        string first_name,
        string last_name,
        string email,
        string user_name,
        string? phone_number,
        string password
    );


    public record UserProfileDate(
        string name
    );

    public record UserMinDTO(
        int Id,
        string first_name,
        string last_name,
        string email,
        Role role,
        string? profile_image_url
    );

    public record UserLoginDTO(
        string email_or_username,
        string password
    );

    public record UserResponseAuthDTO(
        int id
    );


    // ---------------------------------------------------------------------
    // Profile & account settings
    //
    // Read models are positional records (same style as the DTOs above).
    // Write models are classes with settable properties so they can be bound
    // directly by Blazor <EditForm> and validated by DataAnnotationsValidator
    // on the client, and by [ApiController] model validation on the server.
    // ---------------------------------------------------------------------

    public record UserProfileDTO(
        int Id,
        string first_name,
        string last_name,
        string email,
        string user_name,
        string? phone_number,
        Role role,
        string? profile_image_url,
        bool email_notifications_enabled,
        bool reservation_reminders_enabled,
        Theme theme,
        DateTime created_at,
        DateTime updated_at,
        DateTime last_time_active_at
    );

    public class UpdateUserProfileDTO
    {
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
        public string first_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
        public string last_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Username may only contain letters, digits, '.', '_' and '-'.")]
        public string user_name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Email address is not valid.")]
        [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters.")]
        public string email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Phone number is not valid.")]
        [StringLength(13, ErrorMessage = "Phone number cannot exceed 13 characters.")]
        public string? phone_number { get; set; }
    }

    public class UpdateUserSettingsDTO
    {
        public bool email_notifications_enabled { get; set; } = true;

        public bool reservation_reminders_enabled { get; set; } = true;

        [EnumDataType(typeof(Theme), ErrorMessage = "Theme value is not valid.")]
        public Theme theme { get; set; } = Theme.SYSTEM;
    }

    public class ChangePasswordDTO
    {
        [Required(ErrorMessage = "Current password is required.")]
        public string current_password { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "New password must be between 8 and 100 characters.")]
        public string new_password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please confirm the new password.")]
        [Compare(nameof(new_password), ErrorMessage = "The new password and confirmation do not match.")]
        public string confirm_new_password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Error payload returned by the API for 400 / 404 / 409 responses.
    /// Its JSON shape matches ASP.NET Core's ValidationProblemDetails, so the client
    /// can deserialize automatic model-validation errors (400) and business-rule
    /// failures (409 username/email taken, wrong current password) with one type.
    /// Keys in <see cref="errors"/> are DTO property names (e.g. "user_name", "email").
    /// </summary>
    public class ApiErrorDTO
    {
        public string? title { get; set; }
        public int? status { get; set; }
        public Dictionary<string, string[]> errors { get; set; } = new();
    }






}
