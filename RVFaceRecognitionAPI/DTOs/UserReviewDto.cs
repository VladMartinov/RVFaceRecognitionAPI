using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.DTOs
{
    public class UserReviewDto
    {
        public uint UserId { get; set; }

        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public byte[]? Photo { get; set; }

        public UserReviewDto(User user)
        {
            UserId = user.UserId;

            UserRole = user.UserRoleId;
            UserStatus = user.UserStatusId;

            FullName = user.FullName;
            Photo = user.Photo;
        }

        public UserReviewDto()
        {
            // Empty constructor for deserialization
        }
    }
}
