using Microsoft.IdentityModel.Tokens;
using RVFaceRecognitionAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RVFaceRecognitionAPI.Services
{
    public class LoggingService : ILoggingService
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IConfiguration _configuration;

        public LoggingService(ApplicationContext applicationContext, IConfiguration configuration)
        {
            _applicationContext = applicationContext;
            _configuration = configuration;
        }

        /* Метод по добавлению записи активности в базу данных */
        public async Task AddHistoryRecordAsync(User user, TypeActionEnum typeActionEnum)
        {
            var action = _applicationContext.TypeActions.FirstOrDefault(x => x.ActionId == (uint) typeActionEnum);

            var record = new HistoryRecord
            {
                DateAction = DateTime.UtcNow,
                TypeActionId = (uint) typeActionEnum,
                TypeAction = action,
                UserId = user.UserId,
                User = user,
            };

            _applicationContext.HistoryRecords.Add(record);
            await _applicationContext.SaveChangesAsync();
        }

        /* Метод по получению логина пользователя из токена */
        public string? GetUserLoginFromToken(string token)
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

            ClaimsPrincipal principal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return principal?.Identity?.Name ?? null;
        }
    }
}
