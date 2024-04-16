using System.ComponentModel.DataAnnotations;

namespace RVFaceRecognitionAPI.Models
{
    public enum UserRoleEnum
    {
        User = 1,
        Observer = 2,
        Admin = 3
    }

    public class UserRole
    {
        [Key]
        public ushort RoleId { get; set; }

        [Required]
        [MaxLength(20)]
        public string RoleTitle { get; set; }
    }
}
