using System.ComponentModel.DataAnnotations;

namespace RVFaceRecognitionAPI.DTOs
{
    public class ImageCreateDto
    {
        [Required]
        public string FullName { get; set; }
        public string Photo { get; set; }

        public ImageCreateDto(Models.Image image)
        {
            FullName = image.FullName;
            Photo = Convert.ToBase64String(image.Photo);
        }

        public ImageCreateDto()
        {
            // Empty constructor for deserialization
        }
    }
}
