using Emgu.CV;
using Emgu.CV.Structure;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace RVFaceRecognitionAPI.Contollers
{
    [Produces("application/json")]
    [ApiController]
    [Route("api/cameras")]
    public class CamerasController : Controller
    {
        private VideoCapture _videoCapture;
        private Mat _frame;

        // Конструктор для инициализации контроллера
        public CamerasController()
        {
            _videoCapture = new VideoCapture(); // Инициализация камеры
            _videoCapture.ImageGrabbed += ProcessFrame;
            _videoCapture.Start();
        }

        // Метод для обработки кадров и возвращения потока
        private void ProcessFrame(object sender, EventArgs e)
        {
            if (_videoCapture != null && _videoCapture.Ptr != IntPtr.Zero)
            {
                _frame = new Mat();
                _videoCapture.Retrieve(_frame, 0);
            }
        }

        // Метод для получения бесконечного потока кадров из данной камеры
        [HttpGet("CameraStream")]
        public IActionResult CameraStream()
        {
            if (_frame != null)
            {
                var image = _frame.ToImage<Bgr, Byte>().ToBitmap();
                var imageStream = ImageToStream(image);
                return File(imageStream, "image/jpeg");
            }
            else
            {
                return NotFound("No frames available");
            }
        }

        // Метод для преобразования изображения в поток для возврата клиенту
        private static byte[] ImageToStream(System.Drawing.Image image)
        {
            using (var ms = new MemoryStream())
            {
                image.Save(ms, System.Drawing.Imaging.ImageFormat.Jpeg);
                return ms.ToArray();
            }
        }

        // Переопределение метода Dispose для освобождения ресурсов
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _videoCapture?.Stop(); // Остановка камеры
                _videoCapture?.Dispose(); // Освобождение ресурсов камеры
                _frame?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
