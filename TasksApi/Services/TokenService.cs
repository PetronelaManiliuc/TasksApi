using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TasksApi.Entities;
using TasksApi.Helpers;
using TasksApi.Interfaces;
using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Services
{
    public class TokenService : ITokenService
    {
        private readonly TasksDbContext _dbContext;

        public TokenService(TasksDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<Tuple<string, string>> GenerateTokens(int userId)
        {
            var jwtToken = await TokenHelper.GenerateAccessToken(userId);
            var refreshToken = await TokenHelper.GenerateRefreshToken();

            //add new refresh token in DB, delete old one, 
            // refreshToken crypted, cript with salt like pass
            await AddTokenAsync(userId, refreshToken);

            return Tuple.Create(jwtToken, refreshToken);
        }

        public async Task<ValidateRefreshTokenResponse> ValidateRefreshTokens(RefreshTokenRequest refreshTokenRequest)
        {
            var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == refreshTokenRequest.UserId);
            if (existingToken == null)
            {

                return new ValidateRefreshTokenResponse { Success = false, Error = "Token not found.", ErrorCode = "R01" };
            }
            var encryptedToken = PasswordHelper.EncryptUsingPbkdf2(refreshTokenRequest.RefreshToken, Convert.FromBase64String(existingToken.TokenSalt));
            if (encryptedToken != existingToken.TokenHash)
            {
                return new ValidateRefreshTokenResponse { Success = false, Error = "Invalid token.", ErrorCode = "R01" };
            }

            if (existingToken.ExpiryDate < DateTime.Now)
            {
                return new ValidateRefreshTokenResponse { Success = false, Error = "Expired token.", ErrorCode = "R01" };
            }

            return new ValidateRefreshTokenResponse { Success = true, UserId = refreshTokenRequest.UserId };
        }

        private async System.Threading.Tasks.Task AddTokenAsync(int userId, string refreshToken)
        {
            var existingToken = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId.Equals(userId));
            if (existingToken != null)
            {
                _dbContext.RefreshTokens.Remove(existingToken);
            }

            var tokenSalt = PasswordHelper.GetSecureSalt();
            var encryptedToken = PasswordHelper.EncryptUsingPbkdf2(refreshToken, tokenSalt);

            RefreshToken newToken = new RefreshToken
            {
                UserId = userId,
                TokenHash = encryptedToken,
                TokenSalt = Convert.ToBase64String(tokenSalt),
                Ts = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(1)
            };

            await _dbContext.RefreshTokens.AddAsync(newToken);
            var taskResponse = await _dbContext.SaveChangesAsync();
        }
    }
}
