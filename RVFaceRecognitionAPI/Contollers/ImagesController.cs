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
    [Route("api/images")]
    public class ImagesController : Controller
    {
        private readonly ApplicationContext _context;
        private readonly ILoggingService _loggingService;

        public ImagesController(ApplicationContext context, ILoggingService loggingService)
        {
            _context = context;
            _loggingService = loggingService;
        }

        // GET api/images
        /// <summary>
        /// Получение всех изображений.
        /// </summary>
        /// <returns>Список изображений</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<ImageDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetImages()
        {
            var imagesDtos = new List<ImageDto>();

            foreach (var image in _context.Images)
                imagesDtos.Add(new ImageDto(image));

            return Ok(imagesDtos);
        }

        // GET api/images/{id}
        /// <summary>
        /// Получение изображения по его ID
        /// </summary>
        /// <param name="id">Уникальный идентификатор изображения</param>
        /// <returns>Найденное изображение</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ImageDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public IActionResult GetImage(uint id)
        {
            try
            {
                var image = _context.Images.SingleOrDefault(u => u.ImageId == id);

                if (image == null) return NotFound("Image by this ID not found");

                var imageDto = new ImageDto(image);

                return Ok(imageDto);
            }
            catch (Exception ex) { return StatusCode(500, $"An error occurred: {ex.Message}"); }
        }

        // POST api/images
        /// <summary>
        /// Создание нового изображения
        /// </summary>
        /// <param name="image">Объект изображение с самим файлом и полным именем</param>
        /// <returns>Новое изображение</returns>
        [HttpPost]
        [ProducesResponseType(typeof(ImageDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(401)]
        public async Task<IActionResult> CreateImage(ImageCreateDto image)
        {
            var newImage = new Image
            {
                FullName = image.FullName,
                Photo = Convert.FromBase64String(image.Photo),
                DateCreate = DateTime.UtcNow,
            };

            _context.Images.Add(newImage);

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(user, TypeActionEnum.CreateImage);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var createdImageDto = new ImageDto(newImage);
            return CreatedAtAction(nameof(CreateImage), new { id = newImage.ImageId }, createdImageDto);
        }

        // PUT api/images/{id}
        /// <summary>
        /// Обновление изображения по уникальному идентификатору
        /// </summary>
        /// <param name="id">Уникальный идентификатор изображения</param>
        /// <param name="image">Изображение в формате ImageDto</param>
        /// <returns>Обновленное изображение</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ImageDto), 200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> UpdateImage(uint id, ImageDto image)
        {
            var imageToUpdate = _context.Images.SingleOrDefault(u => u.ImageId == id);

            if (imageToUpdate == null) return NotFound("Image by this ID not founded");

            imageToUpdate.FullName = image.FullName;
            imageToUpdate.Photo = Convert.FromBase64String(image.Photo);

            imageToUpdate.DateUpdate = DateTime.UtcNow;

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(user, TypeActionEnum.UpdateImage);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            var updatedImageDto = new ImageDto(imageToUpdate);
            return Ok(updatedImageDto);
        }

        // DELETE api/images/{id}
        /// <summary>
        /// Удаление изображение из системы
        /// </summary>
        /// <param name="id">Уникальный идентификатор удаляемого изображения</param>
        [HttpDelete("{id}")]
        [ProducesResponseType(201)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteImage(uint id)
        {
            var image = _context.Images.SingleOrDefault(u => u.ImageId == id);

            if (image == null) return NotFound("Image by this ID not founded");

            _context.Images.Remove(image);

            string login = _loggingService.GetUserLoginFromToken(Request.Cookies["AccessToken"]);

            if (login is not null)
            {
                var user = _context.Users.FirstOrDefault(u => u.Login == login);
                await _loggingService.AddHistoryRecordAsync(user, TypeActionEnum.DeleteImage);
            }
            else
            {
                await _context.SaveChangesAsync();
            }

            return Ok();
        }
    }
}
