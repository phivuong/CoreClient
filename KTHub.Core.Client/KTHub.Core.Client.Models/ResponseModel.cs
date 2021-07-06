using System;
using System.Net;

namespace KTHub.Core.Client.Models
{
    public class ResponseModel
    {
        public byte[] ResponseData { get; set; }

        public string ResponseMessage { get; set; }

        public HttpStatusCode ResponseStatus { get; set; }
    }
}
