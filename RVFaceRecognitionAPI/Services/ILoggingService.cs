using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.Services
{
    public interface ILoggingService
    {
        Task AddHistoryRecordAsync(User user, TypeActionEnum typeActionEnum);
        string? GetUserLoginFromToken(string token);
    }
}
