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


        // ------------------------------------------------------------------
        // Profile & account settings
        // ------------------------------------------------------------------

        /// <summary>Returns the full profile and settings of a user.</summary>
        /// <response code="200">The user profile.</response>
        /// <response code="404">No active user with this id.</response>
        [HttpGet("{id:int}/profile")]
        [ProducesResponseType(typeof(UserProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDTO>> GetUserProfileApi(int id)
        {
            var profile = await userService.GetUserProfile(id);
            if (profile == null)
            {
                return NotFound(BuildProblem(StatusCodes.Status404NotFound, $"User with id {id} was not found.", new Dictionary<string, string[]>()));
            }
            return Ok(profile);
        }

        /// <summary>Updates first/last name, username, email and phone number.</summary>
        /// <response code="200">The updated profile.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">No active user with this id.</response>
        /// <response code="409">Username and/or email already used by another account (see <c>errors.user_name</c> / <c>errors.email</c>).</response>
        [HttpPut("{id:int}/profile")]
        [ProducesResponseType(typeof(UserProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status409Conflict)]
        public async Task<ActionResult<UserProfileDTO>> UpdateUserProfileApi(int id, UpdateUserProfileDTO profile)
        {
            var result = await userService.UpdateUserProfile(id, profile);
            return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
        }

        /// <summary>Updates notification and appearance settings.</summary>
        /// <response code="200">The updated profile, including settings.</response>
        /// <response code="400">Validation failed.</response>
        /// <response code="404">No active user with this id.</response>
        [HttpPut("{id:int}/settings")]
        [ProducesResponseType(typeof(UserProfileDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDTO>> UpdateUserSettingsApi(int id, UpdateUserSettingsDTO settings)
        {
            var result = await userService.UpdateUserSettings(id, settings);
            return result.IsSuccess ? Ok(result.Value) : MapFailure(result);
        }

        /// <summary>Changes the password after verifying the current one.</summary>
        /// <response code="204">Password changed.</response>
        /// <response code="400">Validation failed or current password is incorrect (see <c>errors.current_password</c>).</response>
        /// <response code="404">No active user with this id.</response>
        [HttpPut("{id:int}/password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ChangePasswordApi(int id, ChangePasswordDTO password)
        {
            var result = await userService.ChangePassword(id, password);
            return result.IsSuccess ? NoContent() : MapFailure(result);
        }


        /// <summary>
        /// Maps a failed service result to an HTTP response. The body is a ValidationProblemDetails
        /// (same shape as automatic 400 model-validation responses), deserializable as Shared.DTO.ApiErrorDTO.
        /// </summary>
        private ActionResult MapFailure<T>(UserOperationResult<T> result)
        {
            return result.Status switch
            {
                UserOperationStatus.NotFound => NotFound(BuildProblem(StatusCodes.Status404NotFound, result.Message, result.Errors)),
                UserOperationStatus.Conflict => Conflict(BuildProblem(StatusCodes.Status409Conflict, result.Message, result.Errors)),
                UserOperationStatus.ValidationFailed => BadRequest(BuildProblem(StatusCodes.Status400BadRequest, result.Message, result.Errors)),
                _ => throw new InvalidOperationException($"Cannot map successful result status '{result.Status}' to an error response.")
            };
        }

        private static ValidationProblemDetails BuildProblem(int status, string? title, IReadOnlyDictionary<string, string[]> errors)
        {
            return new ValidationProblemDetails(errors.ToDictionary(e => e.Key, e => e.Value))
            {
                Status = status,
                Title = title
            };
        }
    }
}
