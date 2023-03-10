using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Interfaces
{
    public interface ITokenService
    {
        Task<Tuple<string, string>> GenerateTokens(int userId);

        Task<ValidateRefreshTokenResponse> ValidateRefreshTokens(RefreshTokenRequest refreshTokenRequest);
    }
}
