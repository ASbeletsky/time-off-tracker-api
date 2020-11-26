using System.Collections.Generic;

namespace ApiModels.Models
{
    public class RequestDataForEmailModel
    {
        public string RequestType { get; set; }
        public string AuthorFullName { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string Duration { get; set; }
        public string Comment { get; set; }
        public string ApprovedFullNames { get; set; }
        public string RejectedBy { get; set; }
        public string RejectComment { get; set; }
    }
}
