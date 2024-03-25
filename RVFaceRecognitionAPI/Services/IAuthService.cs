using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.Services
{
    public interface IAuthService
    {
        Task<LoginResponse> Login(LoginUser user);
        Task<LoginResponse> RefreshToken(RefreshTokenModel model);
    }
}
