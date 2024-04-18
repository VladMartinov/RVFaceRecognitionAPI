using RVFaceRecognitionAPI.Models;

namespace RVFaceRecognitionAPI.DTOs
{
    public class HistoryRecordDto
    {
        public DateTime DateAction { get; set; }

        public string TypeActionTitle { get; set; }
        public string UserFullName { get; set; }

        public HistoryRecordDto(HistoryRecord historyRecord)
        {
            DateAction = historyRecord.DateAction;

            TypeActionTitle = historyRecord.TypeAction.ActionDescription;
            UserFullName = historyRecord.User.FullName;
        }
    }
}
