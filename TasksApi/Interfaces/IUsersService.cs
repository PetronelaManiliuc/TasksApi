using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Interfaces
{
    public interface IUsersService
    {
        Task<SignupResponse> SignupAsync(SignupRequest signupRequest);
        Task<TokenResponse> LoginAsync(LoginRequest loginRequest);
        Task<UserResponse> GetInformation(int userId);
        Task<LogoutResponse> Logout(int userId);
    }
}
