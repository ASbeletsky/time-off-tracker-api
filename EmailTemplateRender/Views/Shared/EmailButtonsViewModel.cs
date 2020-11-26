namespace EmailTemplateRender.Views.Shared
{
    public class EmailButtonsViewModel
    {
        public EmailButtonsViewModel(string approveUrl, string rejectUrl)
        {
            ApproveUrl = approveUrl;
            RejecteUrl = rejectUrl;
        }

        public string ApproveUrl { get; set; }
        public string RejecteUrl { get; set; }
    }
}
