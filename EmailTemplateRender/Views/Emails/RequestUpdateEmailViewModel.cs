using ApiModels.Models;

namespace EmailTemplateRender.Views.Emails
{
    public class RequestEmailViewModel
    {
        public RequestEmailViewModel(RequestDataForEmailModel data, string approveUrl = null, string rejectUrl = null)
        {
            Data = data;
            ApproveUrl = approveUrl;
            RejecteUrl = rejectUrl;
        }

        public string ApproveUrl { get; set; }
        public string RejecteUrl { get; set; }
        public RequestDataForEmailModel Data { get; set; }
    }
}
