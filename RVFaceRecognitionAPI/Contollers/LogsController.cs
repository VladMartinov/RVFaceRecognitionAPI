using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RVFaceRecognitionAPI.DTOs;
using RVFaceRecognitionAPI.Models;
using RVFaceRecognitionAPI.Services;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Authorize]
    [Route("api/logs")]
    public class LogsController : Controller
    {
        private readonly ApplicationContext _context;

        public LogsController(ApplicationContext context)
        {
            _context = context;
        }

        // GET api/logs
        /// <summary>
        /// Получение всех записей из истории активности.
        /// </summary>
        /// <returns>Список записей</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<HistoryRecordDto>), 200)]
        [ProducesResponseType(401)]
        public IActionResult GetHistoryRecords()
        {
            var historyRecord = _context.HistoryRecords
                                        .Include(hr => hr.TypeAction)
                                        .Include(hr => hr.User)
                                        .ToList();
            var historyRecordDtos = new List<HistoryRecordDto>();

            foreach (var record in historyRecord)
                historyRecordDtos.Add(new HistoryRecordDto(record));

            return Ok(historyRecordDtos);
        }
    }
}
