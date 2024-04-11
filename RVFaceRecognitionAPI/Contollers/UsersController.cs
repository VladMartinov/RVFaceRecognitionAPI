using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVFaceRecognitionAPI.DTOs;
using RVFaceRecognitionAPI.Models;
using RVFaceRecognitionAPI.Services;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILoggingService _loggingService;

        public UsersController(ApplicationContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET api/users
        /// <summary>
        /// Получение всех пользователей системы.
        /// </summary>
        /// <remarks>
        /// Получение списка всех пользователей системы в формате UserDto
        /// </remarks>
        /// <returns>Список пользователей системы</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetUsers()
        {
            var usersDtos = new List<UserDto>();

            foreach (var user in _context.Users)
                usersDtos.Add(new UserDto(user));

            return Ok(usersDtos);
        }

        // GET api/users/current-user
        /// <summary>
        /// Получение информации об текущем пользователе.
        /// </summary>
        /// <remarks>
        /// Получение списка всех пользователей системы в формате UserDto
        /// </remarks>
        /// <returns>Текущий пользователь</returns>
        [HttpGet("current-user")]
        [ProducesResponseType(typeof(UserReviewDto), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetCurrentUser()
        {
            string userLogin = Request.HttpContext.User.Identity?.Name ?? string.Empty;

            var currentUser = _context.Users.SingleOrDefault(u => u.Login == userLogin);

            if (currentUser == null) return Unauthorized();

            var currentUserReview = new UserReviewDto(currentUser);

            return Ok(currentUserReview);
        }

        // GET api/users/{id}
        /// <summary>
        /// Получение пользователя системы по его ID
        /// </summary>
        /// <remarks>
        /// Получение конкретного пользователя системы в формате UserDto.
        /// </remarks>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        /// <returns>Найденный пользователь</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetUser(uint id)
        {
            try
            {
                var user = _context.Users.SingleOrDefault(u => u.UserId == id);

                if (user == null) return NotFound("User by this ID not found");

                var userDto = new UserDto(user);

                return Ok(userDto);
            }
            catch (Exception ex) { return StatusCode(500, $"An error occurred: {ex.Message}"); }
        }

        // POST api/users
        /// <summary>
        /// Создание нового пользователя системы
        /// </summary>
        /// <remarks>
        /// UserRole: 1 - Пользователь, 2 - Наблюдатель, 3 - Админ; UserStatus: 1 - Активный, 2 - Заблокированный, 3 - Удалённый.
        /// </remarks>
        /// <param name="userCUDto">Пользователь в формате UserCUDto</param>
        /// <returns>Новый пользователь системы</returns>
        [HttpPost]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateUser(UserCUDto userCUDto)
        {
            if (_context.Users.Any(x => x.Login == userCUDto.Login))
                return StatusCode(400, $"The user with login: \"${userCUDto.Login}\" is already exist!");

            var user = new User
            {
                UserRole = userCUDto.UserRole,
                UserStatus = userCUDto.UserStatus,
                FullName = userCUDto.FullName,
                Photo = userCUDto.Photo is not null ? Convert.FromBase64String(userCUDto.Photo) : null,
                Login = userCUDto.Login,
                Password = BCrypt.Net.BCrypt.HashPassword(userCUDto.Password)
            };

            _context.Users.Add(user);

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(currentUser, TypeActionEnum.CreateUser);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var createdUserDto = new UserDto(user);
            return CreatedAtAction(nameof(CreateUser), new { id = user.UserId }, createdUserDto);
        }

        // PUT api/users/{id}
        /// <summary>
        /// Обновление пользователя по уникальному идентификатору
        /// </summary>
        /// <remarks>
        /// UserRole: 1 - Пользователь, 2 - Наблюдатель, 3 - Админ; UserStatus: 1 - Активный, 2 - Заблокированный, 3 - Удалённый.
        /// </remarks>
        /// <param name="id">Уникальный идентификатор пользователя</param>
        /// <param name="userCUDto">Пользователь в формате UserCUDto</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UserCUDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUser(uint id, UserCUDto userCUDto)
        {
            var userToUpdate = _context.Users.SingleOrDefault(u => u.UserId == id);

            if (userToUpdate == null) return NotFound("User by this ID not founded");

            userToUpdate.UserRole = userCUDto.UserRole;
            userToUpdate.UserStatus = userCUDto.UserStatus;
            
            userToUpdate.FullName = userCUDto.FullName;
            userToUpdate.Photo = userCUDto.Photo is not null ? Convert.FromBase64String(userCUDto.Photo) : null;

            userToUpdate.Login = userCUDto.Login;

            if (!string.IsNullOrWhiteSpace(userCUDto.Password)) userToUpdate.Password = BCrypt.Net.BCrypt.HashPassword(userCUDto.Password);

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(currentUser, TypeActionEnum.UpdateUser);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var updatedUserDto = new UserCUDto(userToUpdate);
            return Ok(updatedUserDto);
        }

        // DELETE api/users/{id}
        /// <summary>
        /// Удаление пользователя в системе
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого пользователя</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteUser(uint id)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserId == id);

            if (user == null) return NotFound("User by this ID not founded");

            user.UserStatus = (ushort) UserStatusEnum.Removed;

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(currentUser, TypeActionEnum.DeleteUser);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var userDto = new UserDto(user);

            return Ok(userDto);
        }

        // PUT api/users/{id}/status
        /// <summary>
        /// Обновление статуса пользователя
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого пользователя</param>
        /// <param name="status">Код статуса</param>
        /// <returns>Обновленный пользователь системы</returns>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(UserDto), 201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateUserStatu(uint id, ushort status)
        {
            var user = _context.Users.SingleOrDefault(u => u.UserId == id);

            if (user == null) return NotFound("User by this ID not founded");

            user.UserStatus = status;

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var currentUser = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(currentUser, TypeActionEnum.ChangeUserStatus);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var userDto = new UserDto(user);

            return Ok(userDto);
        }
    }
}
