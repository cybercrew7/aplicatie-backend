
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






}
