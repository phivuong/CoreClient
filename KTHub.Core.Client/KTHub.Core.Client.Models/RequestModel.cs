namespace KTHub.Core.Client.Models
{
    public class RequestModel
    {
        public virtual string AppId { get; set; }

        public virtual string Signature { get; set; }

        public virtual byte[] Data { get; set; }

        public virtual string RequestParams { get; set; }

        public virtual string TypeName { get; set; }
    }
}