﻿using System.ComponentModel.DataAnnotations;

namespace RVFaceRecognitionAPI.Models
{
    public enum UserStatusEnum
    {
        Active = 1,
        Blocked = 2,
        Removed = 3
    }

    public class UserStatus
    {
        [Key]
        public ushort StatusId { get; set; }

        [Required]
        [MaxLength(20)]
        public string StatusTitle { get; set; }
    }
}
