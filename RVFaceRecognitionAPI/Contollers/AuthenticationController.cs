using Azure;
using Microsoft.AspNetCore.Mvc;
using RVFaceRecognitionAPI.DTOs;
using RVFaceRecognitionAPI.Models;
using RVFaceRecognitionAPI.Services;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/authentication")]
    public class AuthenticationController : Controller
    {
        private readonly ApplicationContext _applicationContext;
        private readonly IAuthService _authService;

        public AuthenticationController(ApplicationContext userContext, IAuthService authService)
        {
            _applicationContext = userContext;
            _authService = authService;
        }

        private void AddCookie(string key, string value)
        {
            HttpContext.Response.Cookies.Append(key, value,
                new CookieOptions
                {
                    Expires = DateTime.UtcNow.AddDays(7),
                    HttpOnly = true,
                    Secure = true,
                    IsEssential = true,
                    SameSite = SameSiteMode.None
                });
        }

        // POST api/authentication/login
        /// <summary>
        /// Метод для авторизации пользователя
        /// </summary>
        /// <param name="userCreds">Объект содержащий Login и Password</param>
        [HttpPost("login")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> AuthenticateUser(LoginUser userCreds)
        {
            var result = await _authService.Login(userCreds);

            if (!result.IsLoggedIn) return Unauthorized();

            AddCookie("AccessToken", result.JwtToken);
            AddCookie("RefreshToken", result.RefreshToken);

            return Ok();
        }

        // POST api/authentication/logout
        /// <summary>
        /// Метод для де авторизации пользователя
        /// </summary>
        [HttpPost("logout")]
        [ProducesResponseType(302)]
        [ProducesResponseType(401)]
        public IActionResult AuthenticateUser()
        {
            var model = new RefreshTokenModel
            {
                JwtToken = Request.Cookies["AccessToken"] ?? "",
                RefreshToken = Request.Cookies["RefreshToken"] ?? ""
            };

            if (String.IsNullOrEmpty(model.JwtToken) || String.IsNullOrEmpty(model.RefreshToken)) return Unauthorized();

            HttpContext.Response.Cookies.Delete("AccessToken");
            HttpContext.Response.Cookies.Delete("RefreshToken");

            return Ok();
        }

        // POST api/authentication/refresh-token
        /// <summary>
        /// Метод для получения нового Access токена
        /// </summary>
        [HttpPost("refresh-token")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> RefreshToken()
        {
            var model = new RefreshTokenModel
            {
                JwtToken = Request.Cookies["AccessToken"] ?? "",
                RefreshToken = Request.Cookies["RefreshToken"] ?? ""
            };

            if (String.IsNullOrEmpty(model.JwtToken) || String.IsNullOrEmpty(model.RefreshToken)) return BadRequest("Failed to refresh token");

            var result = await _authService.RefreshToken(model);

            if (!result.IsLoggedIn) return BadRequest("Failed to refresh token");

            AddCookie("AccessToken", result.JwtToken);
            AddCookie("RefreshToken", result.RefreshToken);

            return Ok();
        }

        // POST api/authentication/check-user
        /// <summary>
        /// Метод проверки существования пользователя
        /// </summary>
        /// <param name="checkUser">Login запрашиваемого пользователя</param>
        [HttpPost("check-user")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public IActionResult UserExistenceCheck(CheckUserDto checkUser)
        {
            var user = _applicationContext.Users.FirstOrDefault(u => u.Login == checkUser.Login);

            if (user == null) return NotFound("User with this login not found");
            else return Ok();
        }

        // PUT api/authentication/changepassword
        /// <summary>
        /// Метод по обновлению пароля пользователя
        /// </summary>
        /// <param name="userCreds">Объект содержащий Login и Password</param>
        [HttpPut("changepassword")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> ChangeUserPassword(LoginUser userCreds)
        {
            var user = _applicationContext.Users.FirstOrDefault(u => u.Login == userCreds.Login);
            if (user == null) return NotFound("User with this login not found");

            if (!string.IsNullOrWhiteSpace(userCreds.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(userCreds.Password);
                await _applicationContext.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
