using Emgu.CV.Structure;
using Emgu.CV;

using System.Drawing.Imaging;
using System.Net.WebSockets;
using System.Drawing;
using System.Reflection;
using Emgu.CV.CvEnum;
using Microsoft.IdentityModel.Tokens;
using RVFaceRecognitionAPI.Models;
using Emgu.CV.Face;
using Emgu.CV.Util;

namespace RVFaceRecognitionAPI.Services
{
    public class StreamService : IStreamService
    {
        #region - Variables -
        private bool _isGetFace;
        private string _detectedFace;

        private readonly ApplicationContext _context;

        private readonly VideoCapture _videoCapture;
        private readonly Mat _frame;

        private List<int> _personsLabes;
        private List<string> _personsNames;
        private List<Image<Gray, byte>> _trainedFaces;

        private readonly CascadeClassifier _faceCascadeClassifier;
        private EigenFaceRecognizer? _recognizer;

        private int _maxWidth;
        private int _maxHeight;
        #endregion

        public StreamService(IServiceProvider serviceProvider)
        {
            _isGetFace = false;

            _context = serviceProvider.GetRequiredService<ApplicationContext>();

            _videoCapture = new VideoCapture();

            _videoCapture.ImageGrabbed += ProcessFrame;
            _videoCapture.Start();

            _frame = new Mat();

            _personsLabes = new List<int>();
            _personsNames = new List<string>();
            _trainedFaces = new List<Image<Gray, Byte>>();

            _faceCascadeClassifier = new CascadeClassifier(
                ExtractResourceToFile(
                    "RVFaceRecognitionAPI.Resources.haarcascade_frontalface_alt.xml",
                    "haarcascade_frontalface_alt.xml"
                )
            );

            TrainByImagesFromDataBase();
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

                            /* Recognize the face */
                            if (_recognizer is not null)
                            {
                                /* Add Persone */
                                Image<Bgr, byte> resultImage = image.Convert<Bgr, byte>();
                                resultImage.ROI = face;

                                Image<Gray, byte> grayFaceResult = resultImage.Convert<Gray, byte>().Resize(_maxWidth, _maxHeight, Inter.Cubic);
                                
                                CvInvoke.EqualizeHist(grayFaceResult, grayFaceResult);
                                
                                var result = _recognizer.Predict(grayFaceResult);

                                /* - Here results found known faces - */
                                if (result.Label != -1 && result.Distance < 8500)
                                {
                                    CvInvoke.PutText(image, _personsNames[result.Label], new Point(face.X - 2, face.Y - 2),
                                        FontFace.HersheyComplex, 1.0, new Bgr(Color.Orange).MCvScalar);

                                    CvInvoke.Rectangle(image, face, new Bgr(Color.Green).MCvScalar, 2);
                                }
                                /* - Here results did not found any know faces - */
                                else
                                {
                                    CvInvoke.PutText(image, "Unknown", new Point(face.X - 2, face.Y - 2),
                                        FontFace.HersheyComplex, 1.0, new Bgr(Color.Orange).MCvScalar);

                                    CvInvoke.Rectangle(image, face, new Bgr(Color.Red).MCvScalar, 2);
                                }

                                resultImage.Dispose();
                                grayFaceResult.Dispose();
                            }

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

        private void TrainByImagesFromDataBase()
        {
            if (_recognizer is not null)
            {
                _recognizer.Dispose();
                _recognizer = null;
            }

            foreach (var image in _trainedFaces)
                image.Dispose();

            _personsLabes.Clear();
            _personsNames.Clear();
            _trainedFaces.Clear();

            var imagePathFiles = new List<string>();

            _maxWidth = 0;
            _maxHeight = 0;

            foreach (var image in _context.Images)
            {
                // Save the image file to a temporary path
                _personsNames.Add(image.FullName);

                string tempImagePath = Path.GetTempFileName();
                File.WriteAllBytes(tempImagePath, image.Photo);
                imagePathFiles.Add(tempImagePath);

                // Read the image file using Emgu.
                var trainedImage = new Image<Bgr, byte>(tempImagePath);

                if (trainedImage.Width > _maxWidth)
                    _maxWidth = trainedImage.Width;

                if (trainedImage.Height > _maxHeight)
                    _maxHeight = trainedImage.Height;

                trainedImage.Dispose();
            }

            if (_maxWidth % 4 != 0) _maxWidth += 4 - _maxWidth % 4;
            if (_maxHeight % 4 != 0) _maxHeight += 4 - _maxHeight % 4;

            try
            {
                for (int i = 0; i < imagePathFiles.Count; i++)
                {
                    // Read the image file using Emgu.
                    var trainedImage = new Image<Bgr, byte>(imagePathFiles[i]);

                    // Resize the image to the largest dimensions
                    trainedImage = trainedImage.Resize(_maxWidth, _maxHeight, Inter.Cubic);

                    // Convert the image to Gray scale
                    Image<Gray, byte> grayImage = trainedImage.Convert<Gray, byte>();

                    CvInvoke.EqualizeHist(grayImage, grayImage);

                    _trainedFaces.Add(grayImage);
                    _personsLabes.Add(i);
                }

                if (_trainedFaces.Count() > 0)
                {
                    // Main function. We will train recognizer on the images and set the border
                    _recognizer = new EigenFaceRecognizer(_trainedFaces.Count);

                    var vectorOfLabels = new VectorOfInt();
                    var vectorOfFaces = new VectorOfMat();

                    vectorOfLabels.Push(_personsLabes.ToArray());
                    vectorOfFaces.Push(_trainedFaces.ToArray());

                    _recognizer.Train(vectorOfFaces, vectorOfLabels);
                }

                return;
            }
            catch (Exception ex)
            {
                return;
            }
        }
        #endregion
    }
}
