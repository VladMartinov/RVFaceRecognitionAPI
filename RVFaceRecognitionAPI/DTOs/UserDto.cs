namespace RVFaceRecognitionAPI.DTOs
{
    public class UserDto
    {
        public uint UserId { get; set; }

        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public byte[]? Photo { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }

        public UserDto(Models.User user)
        {
            UserId = user.UserId;

            UserRole = user.UserRole;
            UserStatus = user.UserStatus;

            FullName = user.FullName;
            Photo = user.Photo;

            Login = user.Login;
            Password = user.Password;
        }

        public UserDto()
        {
            // Empty constructor for deserialization
        }
    }
}
