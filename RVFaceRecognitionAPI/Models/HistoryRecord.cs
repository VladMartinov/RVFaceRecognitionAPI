using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RVFaceRecognitionAPI.Models
{
    public class HistoryRecord
    {
        [Key]
        public uint HistoryRecordId { get; set; }

        public DateTime DateAction { get; set; }

        [ForeignKey(nameof(TypeAction))]
        public uint TypeActionId { get; set; }
        public TypeAction TypeAction { get; set; }

        [ForeignKey(nameof(User))]
        public uint UserId { get; set; }
        public User User { get; set; }
    }
}
