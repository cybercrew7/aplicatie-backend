using saxafon.Data;
using Shared.DTO;
using saxafon.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace saxafon.Services
{


    public interface IUserService {
        Task<IEnumerable<UserMinDTO>> GetAllUsers();
        Task<UserMinDTO> GetUserById(int id);
        Task<UserMinDTO> CreateUser(UserRegisterDTO userRegisterDTO);
        Task<UserResponseAuthDTO> LoginUser(UserLoginDTO userLoginDTO);



    }
    public class UserService : IUserService
    {
        private readonly AppDbContext context;


        public UserService(AppDbContext _context) {
            context = _context;
        }

        public async Task<IEnumerable<UserMinDTO>> GetAllUsers() {
            return await context.Users.Where(u => u.deleted_at == null).Select(usr => new UserMinDTO(
                usr.Id,
                usr.first_name,
                usr.last_name,
                usr.email,
                usr.role,
                usr.profile_image_url
            )).ToListAsync();
        }

        public async Task<UserMinDTO> GetUserById(int id) { 
            return await context.Users.Where(u => u.Id == id && u.deleted_at == null).Select(usr => new UserMinDTO(
                usr.Id,
                usr.first_name,
                usr.last_name,
                usr.email,
                usr.role,
                usr.profile_image_url
            )).FirstOrDefaultAsync();
        }




        public async Task<UserMinDTO> CreateUser(UserRegisterDTO userRegisterDTO)
        {
            var user = new User
            {
                first_name = userRegisterDTO.first_name,
                last_name = userRegisterDTO.last_name,
                email = userRegisterDTO.email,
                user_name = userRegisterDTO.user_name,
                phone_number = userRegisterDTO.phone_number,
                passwordHash = userRegisterDTO.password,
                created_at = DateTime.UtcNow,
                updated_at = DateTime.UtcNow,
                last_time_active_at = DateTime.UtcNow,
                role = Shared.Role.USER
            };
            

            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserMinDTO(
                user.Id,
                user.first_name,
                user.last_name,
                user.email,
                user.role,
                user.profile_image_url
            );
        }

        public async Task<UserResponseAuthDTO> LoginUser(UserLoginDTO userLoginDTO)
        {
            var user = await context.Users.Where(u => ((u.user_name == userLoginDTO.email_or_username) 
            || (u.email == userLoginDTO.email_or_username) ) 
            && u.passwordHash == userLoginDTO.password
            && u.deleted_at == null).FirstOrDefaultAsync();
            if (user == null)
            {
                return null;
            }
            return new UserResponseAuthDTO(
                user.Id
            );
        }



    }



}
