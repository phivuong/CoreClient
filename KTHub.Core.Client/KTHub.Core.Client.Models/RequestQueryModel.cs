namespace KTHub.Core.Client.Models
{
    public class RequestQueryModel
    {
        public string AppId { get; set; }

        public string Signature { get; set; }

        public string RequestParams { get; set; }

        public string DicParams { get; set; }
    }
}