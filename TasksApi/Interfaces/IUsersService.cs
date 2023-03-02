using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Interfaces
{
    public interface IUsersService
    {
        Task<SignupResponse> SignupAsync(SignupRequest signupRequest);
        Task<TokenResponse> LoginAsync(LoginRequest loginRequest);

    }
}
