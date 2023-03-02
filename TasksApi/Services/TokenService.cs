using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using TasksApi.Entities;
using TasksApi.Helpers;
using TasksApi.Interfaces;
using TasksApi.Requests;

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
            AddTokenAsync(userId, refreshToken);

            return Tuple.Create(jwtToken, refreshToken);
        }

        private async System.Threading.Tasks.Task AddTokenAsync(int userId, string refreshToken)
        {
            var existingToken = _dbContext.RefreshTokens.FirstOrDefault(x => x.UserId.Equals(userId));
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
