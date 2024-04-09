using Emgu.CV.Structure;
using Emgu.CV;

using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Drawing;
using System.Reflection;
using Emgu.CV.CvEnum;
using System.IO;
using Microsoft.IdentityModel.Tokens;

namespace RVFaceRecognitionAPI.Services
{
    public class SteamService
    {
        #region - Variables -
        private bool _isGetFace;
        private string _detectedFace;

        private readonly VideoCapture _videoCapture;
        private readonly Mat _frame;

        private readonly CascadeClassifier _faceCascadeClassifier;
        #endregion

        public SteamService()
        {
            _isGetFace = false;

            _videoCapture = new VideoCapture();

            _videoCapture.ImageGrabbed += ProcessFrame;
            _videoCapture.Start();

            _frame = new Mat();

            _faceCascadeClassifier = new CascadeClassifier(
                ExtractResourceToFile(
                    "RVFaceRecognitionAPI.Resources.haarcascade_frontalface_alt.xml",
                    "haarcascade_frontalface_alt.xml"
                )
            );
        }

        #region - Public Functions -
        public async Task SendImageToWebSocket(WebSocket webSocket, CancellationToken cancellationToken)
        {
            while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                if (_frame.NumberOfChannels != 3)
                    continue;

                using (var ms = new MemoryStream())
                {
                    // The best way to fix memory leaks and normal performance
                    Image<Bgr, byte> image = _frame.ToImage<Bgr, byte>();

                    // Start recognition faces on image
                    var grayImage = new Mat();
                    CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

                    CvInvoke.EqualizeHist(grayImage, grayImage);

                    Rectangle[] faces = _faceCascadeClassifier.DetectMultiScale(grayImage, 1.1, 3, Size.Empty, Size.Empty);

                    if (faces.Length > 0)
                    {
                        if (_isGetFace)
                        {
                            _isGetFace = false;

                            Image<Bgr, byte> imageToSave = image.Clone();

                            // Создайте новое изображение только с областью лица
                            Image<Bgr, byte> croppedImage = new Image<Bgr, byte>(faces[0].Size);
                            imageToSave.ROI = faces[0];
                            imageToSave.CopyTo(croppedImage);

                            // Сохраните изображение в MemoryStream
                            using (var msSaveFace = new MemoryStream())
                            {
                                Bitmap bitmapToSave = croppedImage.ToBitmap();
                                bitmapToSave.Save(msSaveFace, ImageFormat.Jpeg);

                                imageToSave.Dispose();
                                croppedImage.Dispose();
                                bitmapToSave.Dispose();

                                _detectedFace = Convert.ToBase64String(msSaveFace.ToArray());
                            }
                        }

                        foreach (var face in faces)
                        {
                            CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);
                        }
                    }
                    // End recognition faces on image

                    Bitmap bitmap = image.ToBitmap();

                    bitmap.Save(ms, ImageFormat.Jpeg);

                    grayImage.Dispose();
                    image.Dispose();
                    bitmap.Dispose();

                    byte[] imageData = ms.ToArray();
                    await webSocket.SendAsync(new ArraySegment<byte>(imageData, 0, imageData.Length), WebSocketMessageType.Binary, true, CancellationToken.None);
                }

                await Task.Delay(50, cancellationToken);
            }
        }

        public async Task<string> GetFirstDetectedFace()
        {
            DateTime startTime = DateTime.UtcNow;
            string face = string.Empty;
            
            _isGetFace = true;
            _detectedFace = string.Empty;

            while ((DateTime.UtcNow - startTime).TotalSeconds < 10)
            {
                if (!_detectedFace.IsNullOrEmpty())
                {
                    face = _detectedFace;
                    break;
                }

                await Task.Delay(500);
            }

            _isGetFace = false;
            _detectedFace = string.Empty;

            return face;
        }
        #endregion

        #region - Private Functions -
        private async void ProcessFrame(object sender, EventArgs e)
        {
            if (_videoCapture != null && _videoCapture.Ptr != IntPtr.Zero)
            {
                await Task.Run(() =>
                {
                    _videoCapture.Retrieve(_frame, 0);
                });
            }
        }

        private string ExtractResourceToFile(string resourceName, string fileName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceStream = assembly.GetManifestResourceStream(resourceName);

            if (resourceStream == null)
            {
                throw new Exception($"Resource {resourceName} not found.");
            }

            var filePath = Path.Combine(Path.GetTempPath(), fileName);

            using (var fileStream = File.Create(filePath))
            {
                resourceStream.CopyTo(fileStream);
            }

            return filePath;
        }
        #endregion
    }
}
