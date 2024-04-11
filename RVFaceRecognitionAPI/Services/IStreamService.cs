using System.Net.WebSockets;

namespace RVFaceRecognitionAPI.Services
{
    public interface IStreamService
    {
        Task SendImageToWebSocket(WebSocket webSocket, CancellationToken cancellationToken);
        Task<string> GetFirstDetectedFace();
    }
}
