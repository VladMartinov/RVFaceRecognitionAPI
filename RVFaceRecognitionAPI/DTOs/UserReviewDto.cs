namespace RVFaceRecognitionAPI.DTOs
{
    public class UserReviewDto
    {
        public uint UserId { get; set; }

        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public byte[]? Photo { get; set; }

        public UserReviewDto(Models.User user)
        {
            UserId = user.UserId;

            UserRole = user.UserRole;
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
