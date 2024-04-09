using System.ComponentModel.DataAnnotations;

namespace RVFaceRecognitionAPI.Models
{
    public class Image
    {
        [Key]
        public uint ImageId { get; set; }

        [MaxLength(155)]
        public required string FullName { get; set; }
        public byte[] Photo { get; set; }

        public DateTime DateCreate { get; set; }
        public DateTime? DateUpdate { get; set; }
    }
}
