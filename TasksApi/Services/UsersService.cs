using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using TasksApi.Entities;
using TasksApi.Helpers;
using TasksApi.Interfaces;
using TasksApi.Requests;
using TasksApi.Responses;

namespace TasksApi.Services
{
    public class UsersService : IUsersService
    {
        private readonly TasksDbContext _dbContext;

        private readonly ITokenService tokenService;
        public UsersService(TasksDbContext dbContext, ITokenService tokenService)
        {
            this._dbContext = dbContext;
            this.tokenService = tokenService;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest loginRequest)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Email.Equals(loginRequest.Email));
            if (user == null)
            {
                return new TokenResponse { Success = false, Error = "Email not found.", ErrorCode = "L01" };
            }

            var encryptedPassword = PasswordHelper.EncryptUsingPbkdf2(loginRequest.Password, Convert.FromBase64String(user.PasswordSalt));

            if (user.Password != encryptedPassword)
            {
                return new TokenResponse { Success = false, Error = "Invalid password.", ErrorCode = "L02" };
            }

            var tokens = await tokenService.GenerateTokens(user.Id);

            return new TokenResponse
            {
                AccessToken = tokens.Item1,
                RefreshToken = tokens.Item2,
                FirstName = user.FirstName,
                UserId = user.Id,
                Success = true
            };
        }

        public async Task<SignupResponse> SignupAsync(SignupRequest signupRequest)
        {
            var existingUser = _dbContext.Users.FirstOrDefault(x => x.Email == signupRequest.Email);
            if (existingUser != null)
            {
                return new SignupResponse { Success = false, ErrorCode = "S01", Error = "Email already existing!" };
            }
           
            bool isPasswordValid = ValidatePassword(signupRequest.Password, signupRequest.ConfirmPassword);
            if (!isPasswordValid)
            {
                return new SignupResponse { Success = false, Email = signupRequest.Email, Error = "Invalid password!", ErrorCode = "S03" };
            }
            var passwordSalt = PasswordHelper.GetSecureSalt();
            var encryptedPassword = PasswordHelper.EncryptUsingPbkdf2(signupRequest.Password, passwordSalt);
            User newUser = new User
            {
                Email = signupRequest.Email,
                FirstName = signupRequest.FirstName,
                LastName = signupRequest.LastName,
                Active = true,
                Password = encryptedPassword,
                PasswordSalt = Convert.ToBase64String(passwordSalt),
                Ts = signupRequest.Ts
            };

            await _dbContext.Users.AddAsync(newUser);
            var saveUserResponse = await _dbContext.SaveChangesAsync();

            if (saveUserResponse > 0)
            {
                return new SignupResponse { Success = true, Email = signupRequest.Email };
            }

            return new SignupResponse { Success = false, Email = signupRequest.Email, Error = "Signup fail!", ErrorCode = "S02" };
        }

        public async Task<UserResponse> GetInformation(int userId)
        {

            var user = await _dbContext.Users.FindAsync(userId);

            if (user != null)
            {
                return new UserResponse { Success = true, Email = user.Email, FirstName = user.FirstName, LastName = user.LastName, CreationDate = user.Ts };
            }


            return new UserResponse { Success = false, Error = "User not found!", ErrorCode = "I01" };

        }



        private bool ValidatePassword(string password, string confirmPassword)
        {
            bool isValid = true;
            isValid = String.Compare(password, confirmPassword) == 0;

            if (isValid)
            {
                var hasNumber = new Regex(@"[0-9]+");
                var hasUpperChar = new Regex(@"[A-Z]+");
                var hasMinimum8Chars = new Regex(@".{8,}");

                isValid = hasNumber.IsMatch(password) && hasUpperChar.IsMatch(password) && hasMinimum8Chars.IsMatch(password);
            }

            return isValid;
        }

        public async Task<LogoutResponse> Logout(int userID)
        {
            var token = await _dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.UserId == userID);
            if (token != null)
            {
                _dbContext.RefreshTokens.Remove(token);

                var response = _dbContext.SaveChanges();

                if (response > 0)
                {
                    return new LogoutResponse { Success = true };
                }
            }

            return new LogoutResponse { Success = false, Error = "User already logout.", ErrorCode = "L03" };
        }

    }
}
