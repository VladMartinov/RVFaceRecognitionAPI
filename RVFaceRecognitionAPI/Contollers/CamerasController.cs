using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RVFaceRecognitionAPI.Services;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/cameras")]
    public class CamerasController : Controller
    {
        SteamService _streamService;

        // Конструктор для инициализации контроллера
        public CamerasController(SteamService streamService)
        {
            _streamService = streamService;
        }

        // GET api/cameras/start-stream
        /// <summary>
        /// Метод для получения кода доступа к потоку изображений
        /// </summary>
        /// <param name="guid">Уникальный код для его обновления</param>
        /// <returns>Уникальный код доступа к потоку изображений</returns>
        [HttpGet]
        [ProducesResponseType(200)]
        [Route("start-stream")]
        public IActionResult StartCameraStream(Guid? guid = null)
        {
            Guid streamGuid = _streamService.StartImageStream(guid);
            return Ok(streamGuid);
        }

        // GET api/cameras/frames
        /// <summary>
        /// Метод для получения изображений от видеокамеры
        /// </summary>
        /// <param name="guid">Уникальный код потока</param>
        /// <returns>Бесконечный поток байтов кадров</returns>
        [HttpGet]
        [Route("frames")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult> Images(Guid guid)
        {
            Response.ContentType = "text/plain; charset=utf-8";
            int responseCode = await _streamService.StreamImageToAsync(Response.Body, guid);
        
            switch(responseCode)
            {
                case 404:
                    return NotFound();
                case 200:
                    return Ok();
                default:
                    return BadRequest(responseCode);
            }
        }

        // POST api/cameras/stope-stream
        /// <summary>
        /// Метод для остановки потока
        /// </summary>
        /// <param name="guid">Уникальный код потока</param>
        [HttpPost]
        [Route("stope-stream")]
        [ProducesResponseType(200)]
        public IActionResult StopCameraStream(Guid guid)
        {
            _streamService.StopImageStream(guid);
            return Ok();
        }
    }
}
