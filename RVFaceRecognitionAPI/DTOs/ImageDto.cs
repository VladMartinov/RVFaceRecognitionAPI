using System.ComponentModel.DataAnnotations;

namespace RVFaceRecognitionAPI.DTOs
{
    public class ImageDto
    {
        public uint ImageId { get; set; }

        [Required]
        public string FullName { get; set; }
        public string Photo { get; set; }

        public ImageDto(Models.Image image)
        {
            ImageId = image.ImageId;
            FullName = image.FullName;
            Photo = Convert.ToBase64String(image.Photo);
        }

        public ImageDto()
        {
            // Empty constructor for deserialization
        }
    }
}
