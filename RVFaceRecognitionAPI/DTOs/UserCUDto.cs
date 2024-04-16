using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.DTOs
{
    public class UserCUDto
    {
        public ushort UserRoleId { get; set; }
        public UserRole? UserRole { get; set; }

        public ushort UserStatusId { get; set; }
        public UserStatus? UserStatus { get; set; }

        public string FullName { get; set; }
        public string? Photo { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }

        public UserCUDto(User user)
        {
            UserRoleId = user.UserRoleId;
            UserRole = user.UserRole;

            UserStatusId = user.UserStatusId;
            UserStatus = user.UserStatus;

            FullName = user.FullName;
            Photo = user.Photo is not null ? Convert.ToBase64String(user.Photo) : null;

            Login = user.Login;
            Password = user.Password;
        }

        public UserCUDto()
        {
            // Empty constructor for deserialization
        }
    }
}
