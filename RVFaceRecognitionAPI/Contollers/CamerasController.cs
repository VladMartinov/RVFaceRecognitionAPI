using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using RVFaceRecognitionAPI.Services;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/cameras")]
    public class CamerasController : Controller
    {
        private readonly SteamService _streamService;

        public CamerasController(SteamService streamService)
        {
            _streamService = streamService;
        }

        // GET api/cameras/face
        /// <summary>
        /// Получение первого обнаруженного лица.
        /// </summary>
        /// <returns>Лицо в base64 формате</returns>
        [HttpGet]
        [Route("face")]
        [ProducesResponseType(200)]
        [ProducesResponseType(401)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetFirstDetectedFaceAsync()
        {
            string firstFace = await _streamService.GetFirstDetectedFace();
            
            if (!firstFace.IsNullOrEmpty())
            {
                return Ok(firstFace);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
