using Microsoft.AspNetCore.Authorization;
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
        private readonly ITokenService tokensService;

        public UsersController(IUsersService usersService, ITokenService tokenService)
        {
            this.usersService = usersService;
            this.tokensService = tokenService;
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
                return BadRequest(new TokenResponse { Error = "Missing crediantials.", ErrorCode = "L02", Success = false });
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

        [HttpGet, Route("info")]
        [Authorize]
        public async Task<IActionResult> GetInformations()
        {

            UserResponse information = await usersService.GetInformation(UserId);
            if (information.Success)
            {
                return Ok(information);
            }

            return UnprocessableEntity(information);
        }

        [HttpPost, Route("logout")]
        public async Task<IActionResult> Logout()
        {

            LogoutResponse logout = await usersService.Logout(UserId);

            if (logout.Success)
            {
                return Ok();
            }

            return UnprocessableEntity(logout);
        }

        [HttpPost, Route("refreshToken")]
        [Authorize]
        public async Task<IActionResult> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {

            if (refreshTokenRequest.UserId == 0 && string.IsNullOrEmpty(refreshTokenRequest.RefreshToken))
            {
                return BadRequest(refreshTokenRequest);
            }

            var validateRefreshTokenResponse = await tokensService.ValidateRefreshTokens(refreshTokenRequest);

            if (!validateRefreshTokenResponse.Success)
            {
                return BadRequest(validateRefreshTokenResponse);
            }

            var tokens = await tokensService.GenerateTokens(refreshTokenRequest.UserId);

            return Ok(new TokenResponse { AccessToken = tokens.Item1, RefreshToken = tokens.Item2 });
        }
    }
}

