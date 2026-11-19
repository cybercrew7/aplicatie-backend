using Microsoft.AspNetCore.Mvc;
using saxafon.Models;
using saxafon.Services;
using Shared.DTO;


namespace saxafon.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {

        private readonly IUserService userService; 
        public UserController(IUserService _userService)
        {
            userService = _userService;
        }


        [HttpGet("getUser")]
        public  async Task<ActionResult<IEnumerable<UserMinDTO>>> getUsers()
        {
            return  Ok( await userService.GetAllUsers());

        }

        [HttpGet("getUserId/{id}")]
        public async Task<ActionResult<UserMinDTO>> getUserById( int id)
        {
           
            var user = await userService.GetUserById(id);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserMinDTO>> createUser(UserRegisterDTO userRegisterDTO)
        {
            var user = await userService.CreateUser(userRegisterDTO);
            return CreatedAtAction(nameof(getUserById), new { id = user.Id }, user);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserResponseAuthDTO>> loginUser( UserLoginDTO userLogin)
        {
            var user = await userService.LoginUser(userLogin);
            if (user == null)
            {
                return Unauthorized(new { meesage =  "Invalid credentials" });
            }
            return Ok(user);
        }

       
    }
}
