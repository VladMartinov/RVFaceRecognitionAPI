using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVFaceRecognitionAPI.Models
{
    public enum UserRoleEnum
    {
        User = 1,
        Observer = 2,
        Admin = 3
    }

    public enum UserStatusEnum
    {
        Active = 1,
        Blocked = 2,
        Removed = 3
    }

    public class User
    {
        [Key]
        public uint UserId { get; set; }

        public required ushort UserRole { get; set; }
        public required ushort UserStatus { get; set; }

        [MaxLength(155)]
        public required string FullName { get; set; }
        public byte[]? Photo { get; set; }
        [MaxLength(20)]
        public required string Login { get; set; }
        public required string Password { get; set; }

        public string? RefreshToken {  get; set; }
        public DateTime RefreshTokenExpiry { get; set; }

        public ICollection<HistoryRecord> HistoryRecords { get; set; }
    }
}
