using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TasksApi.Interfaces;
using TasksApi.Requests;
using TasksApi.Responses;
using TasksApi.Services;

namespace TasksApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : BaseApiController
    {
        private readonly IUsersService usersService;

        public UsersController(IUsersService usersService)
        {
            this.usersService = usersService;
        }

        [HttpPost, Route("signup")]
        public async Task<IActionResult> Signup(SignupRequest signup)
        {

            var user = new Entities.User { Email = signup.Email, FirstName = signup.Email, LastName = signup.LastName, Password = signup.Password };

            SignupResponse signupResponse = await usersService.SignupAsync(signup);

            if (!signupResponse.Success)
            {
                return UnprocessableEntity(signupResponse);
            }

            return Ok(new SignupResponse { Success = true, Email = signup.Email });
        }

        [HttpPost, Route("login")]
        public async Task<IActionResult> Login(LoginRequest login)
        {
            if (string.IsNullOrEmpty(login.Email) || string.IsNullOrEmpty(login.Password))
            {
                return BadRequest(new TokenResponse { Error = "Missing crediantials.", ErrorCode = "L01", Success = false });
            }

            TokenResponse tokenResponse = await usersService.LoginAsync(login);
            if (!tokenResponse.Success)
            {
                return Unauthorized((new
                {
                    tokenResponse.ErrorCode,
                    tokenResponse.Error
                }));
            }

            return Ok(tokenResponse);
        }
    }
}
