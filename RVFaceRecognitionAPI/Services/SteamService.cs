using Emgu.CV.Structure;
using Emgu.CV;

using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Drawing;
using System.Reflection;
using Emgu.CV.CvEnum;

namespace RVFaceRecognitionAPI.Services
{
    public class SteamService
    {
        #region - Variables -
        private readonly VideoCapture _videoCapture;
        private readonly Mat _frame;

        private readonly CascadeClassifier _faceCascadeClassifier;
        #endregion

        public SteamService()
        {
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
                    // Memory leak
                    // _frame.ToImage<Bgr, byte>().ToBitmap().Save(ms, ImageFormat.Jpeg);

                    // Fix memory leak, but performance drop
                    // image = null;
                    // bitmap = null;
                    // GC.Collect();

                    // The best way to fix memory leaks and normal performance
                    Image<Bgr, byte> image = _frame.ToImage<Bgr, byte>();

                    // Start recognition faces on image
                    var grayImage = new Mat();
                    CvInvoke.CvtColor(image, grayImage, ColorConversion.Bgr2Gray);

                    CvInvoke.EqualizeHist(grayImage, grayImage);

                    Rectangle[] faces = _faceCascadeClassifier.DetectMultiScale(grayImage, 1.1, 3, Size.Empty, Size.Empty);

                    if (faces.Length > 0)
                    {
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
