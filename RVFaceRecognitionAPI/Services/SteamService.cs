using Emgu.CV.Structure;
using Emgu.CV;

using System.Drawing.Imaging;
using System.Text;

using RVFaceRecognitionAPI.Models;
using Microsoft.AspNetCore.Mvc;

namespace RVFaceRecognitionAPI.Services
{
    public class SteamService
    {
        #region - Variables -
        private readonly Dictionary<Guid, StreamInfo> _streamTasks;
        private readonly VideoCapture _videoCapture;
        
        private Mat _frame;
        #endregion

        public SteamService()
        {
            _streamTasks = new Dictionary<Guid, StreamInfo>();
            _videoCapture = new VideoCapture();

            _videoCapture.ImageGrabbed += ProcessFrame;
            _videoCapture.Start();
        }

        #region - Public Functions -
        public Guid StartImageStream(Guid? guid = null)
        {
            var now = DateTime.UtcNow;
            var keysToRemove = _streamTasks.Where(kvp => now.Subtract(kvp.Value.DateLastAccess).TotalHours >= 1)
                                           .Select(kvp => kvp.Key)
                                           .ToList();

            foreach (var key in keysToRemove)
            {
                _streamTasks.Remove(key);
            }

            if (guid != null && _streamTasks.ContainsKey((Guid) guid))
            {
                _streamTasks[(Guid) guid].DateLastAccess = DateTime.UtcNow;

                return (Guid) guid;
            }

            var streamGuid = Guid.NewGuid();
            var cancellationTokenSource = new CancellationTokenSource();
            var streamInfo = new StreamInfo { CancellationTokenSource = cancellationTokenSource, DateLastAccess = DateTime.UtcNow };

            _streamTasks.Add(streamGuid, streamInfo);

            return streamGuid;
        }

        public void StopImageStream(Guid streamGuid)
        {
            if (_streamTasks.ContainsKey(streamGuid))
            {
                _streamTasks[streamGuid].CancellationTokenSource.Cancel();
                _streamTasks.Remove(streamGuid);
            }
        }

        public async Task<int> StreamImageToAsync(Stream stream, Guid guid)
        {
            StreamInfo info = _streamTasks.ContainsKey(guid) ? _streamTasks[guid] : null;

            if (info is null || DateTime.UtcNow.Subtract(info.DateLastAccess).TotalHours >= 1) return 404;

            while (_frame is null)
                await Task.Delay(100);

            await Task.Yield(); // отдаем управление другим задачам

            List<Task> tasks = new List<Task>();

            while (!info.CancellationTokenSource.IsCancellationRequested)
            {
                Mat currentFrame = _frame.Clone();
                info.DateLastAccess = DateTime.UtcNow;

                if (currentFrame.NumberOfChannels != 3)
                {
                    await Task.Delay(100);
                    continue;
                }

                Task task = Task.Run(async () =>
                {
                    using (var ms = new MemoryStream())
                    {
                        currentFrame.ToImage<Bgr, byte>()
                            .ToBitmap()
                            .Save(ms, ImageFormat.Jpeg);

                        byte[] imageData = ms.ToArray();

                        await WriteValueToStream(stream, string.Empty, imageData);
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);
            return 200;
        }
        #endregion

        #region - Private Functions -
        private async Task WriteValueToStream(Stream stream, string value, byte[]? valueAsBytes = null)
        {
            valueAsBytes ??= Encoding.UTF8.GetBytes(value);
            
            await stream.WriteAsync(valueAsBytes, 0, valueAsBytes.Length);
            await stream.FlushAsync();
        }

        private async void ProcessFrame(object sender, EventArgs e)
        {
            if (_videoCapture != null && _videoCapture.Ptr != IntPtr.Zero)
            {
                _frame = new Mat();
                await Task.Run(() =>
                {
                    _videoCapture.Retrieve(_frame, 0);
                });
            }
        }
        #endregion
    }
}
