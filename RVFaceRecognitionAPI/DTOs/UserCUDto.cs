﻿namespace RVFaceRecognitionAPI.DTOs
{
    public class UserCUDto
    {
        public ushort UserRole { get; set; }
        public ushort UserStatus { get; set; }

        public string FullName { get; set; }
        public byte[]? Photo { get; set; }

        public string Login { get; set; }
        public string Password { get; set; }

        public UserCUDto(Models.User user)
        {
            UserRole = user.UserRole;
            UserStatus = user.UserStatus;

            FullName = user.FullName;
            Photo = user.Photo;

            Login = user.Login;
            Password = user.Password;
        }

        public UserCUDto()
        {
            // Empty constructor for deserialization
        }
    }
}