using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

namespace TasksApi.Helpers
{
    public class TokenHelper
    {
        public const string Issuer = "http://tasksapp.com";
        public const string Audience = "http://tasksapp.com";
        public const string Secret = "U2VjcmV0S2V5MTIzNDU2Nzg5";
        public static async Task<string> GenerateAccessToken(int userId) // JWT token
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
            var signingCredentials = new SigningCredentials(new SymmetricSecurityKey(Convert.FromBase64String(Secret)), SecurityAlgorithms.HmacSha256Signature);

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = claimsIdentity,
                Issuer = Issuer,
                Expires = DateTime.Now.AddMinutes(15),
                SigningCredentials = signingCredentials, 
                Audience = Audience
            };

            var securityToken = handler.CreateToken(securityTokenDescriptor);
            return handler.WriteToken(securityToken);
        }

        public static async Task<string> GenerateRefreshToken()
        {
            var salt = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                return Convert.ToBase64String(salt);
            }
        }

    }
}
