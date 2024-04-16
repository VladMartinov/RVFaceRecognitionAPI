using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.DTOs
{
    public class UserReviewDto
    {
        public uint UserId { get; set; }

        public ushort UserRoleId { get; set; }
        public UserRole? UserRole { get; set; }

        public ushort UserStatusId { get; set; }
        public UserStatus? UserStatus { get; set; }

        public string FullName { get; set; }
        public byte[]? Photo { get; set; }

        public UserReviewDto(User user)
        {
            UserId = user.UserId;

            UserRoleId = user.UserRoleId;
            UserRole = user.UserRole;

            UserStatusId = user.UserStatusId;
            UserStatus = user.UserStatus;

            FullName = user.FullName;
            Photo = user.Photo;
        }

        public UserReviewDto()
        {
            // Empty constructor for deserialization
        }
    }
}
