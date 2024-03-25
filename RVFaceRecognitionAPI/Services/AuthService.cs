using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RVFaceRecognitionAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RVFaceRecognitionAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UsersContext _usersContext;
        private readonly IConfiguration _configuration;

        public AuthService(UsersContext usersContext, IConfiguration configuration)
        {
            _usersContext = usersContext;
            _configuration = configuration;
        }

        public async Task<LoginResponse> Login(LoginUser user)
        {
            var response = new LoginResponse();
            var identityUser = _usersContext.Users.FirstOrDefault(u => u.Login == user.Login);

            if (identityUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, identityUser.Password)) return response;

            response.IsLoggedIn = true;

            response.IsLoggedIn = true;
            response.JwtToken = this.GenerateTokenString(identityUser.Login, identityUser.UserRole.ToString(), identityUser.UserStatus.ToString());
            response.RefreshToken = this.GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await _usersContext.SaveChangesAsync();

            return response;
        }

        public async Task<LoginResponse> RefreshToken(RefreshTokenModel model)
        {
            var principal = this.GetTokenPrincipal(model.JwtToken);

            var response = new LoginResponse();
            if (principal?.Identity?.Name is null)
                return response;

            var identityUser = await _usersContext.Users.FirstOrDefaultAsync(u => u.Login == principal.Identity.Name);

            if (identityUser is null || identityUser.RefreshToken != model.RefreshToken || identityUser.RefreshTokenExpiry < DateTime.UtcNow)
                return response;

            response.IsLoggedIn = true;
            response.JwtToken = this.GenerateTokenString(identityUser.Login, identityUser.UserRole.ToString(), identityUser.UserStatus.ToString());
            response.RefreshToken = this.GenerateRefreshTokenString();

            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiry = DateTime.UtcNow.AddHours(12);
            await _usersContext.SaveChangesAsync();

            return response;
        }

        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value ?? ""));

            var validationParameters = new TokenValidationParameters()
            {
                ValidateActor = false,
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateLifetime = false,
                IssuerSigningKey = securityKey
            };

            return new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
        }

        private string GenerateRefreshTokenString()
        {
            var randomNumber = new byte[64];

            using (var numberGenerator = RandomNumberGenerator.Create())
                numberGenerator.GetBytes(randomNumber);

            var refreshToken = Convert.ToBase64String(randomNumber);
            return refreshToken;
        }

        private string GenerateTokenString(string login, string role, string status)
        {
            var claims = new List<Claim>()
            {
                new Claim(type: ClaimTypes.Name, value: login ?? ""),
                new Claim(type: ClaimTypes.Role, value: role ?? ""),
                new Claim(type: "Status", value: status ?? ""),
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("Jwt:Key").Value ?? ""));
            var signingCred = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature);

            var securityToker = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(15),
                    issuer: _configuration.GetSection("Jwt:Issuer").Value,
                    audience: _configuration.GetSection("Jwt:Audience").Value,
                    signingCredentials: signingCred
                    );
            string tokenString = new JwtSecurityTokenHandler().WriteToken(securityToker);
            return tokenString;
        }
    }
}
