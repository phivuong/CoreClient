using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KTHub.Core.Client
{
    public interface IRestClient : IDisposable
    {
        IDictionary<string, string> DefaultRequestHeaders { get; }

        TimeSpan Timeout { get; }

        uint MaxResponseContentBufferSize { get; }

        Uri[] Endpoints { get; }

        Task<HttpResponseMessage> GetAsync(Uri requestUri);

        Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cToken);

        Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption option);

        Task<HttpResponseMessage> GetAsync(Uri requestUri,HttpCompletionOption option, CancellationToken cToken);

        Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent httpContent);

        Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent httpContent, CancellationToken cToken);
        Task<HttpResponseMessage> PostAsync(Uri requestUri, ByteArrayContent httpContent);

        Task<HttpResponseMessage> PostAsync(Uri requestUri, ByteArrayContent httpContent, CancellationToken cToken);

        Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent httpContent);

        Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent httpContent, CancellationToken cToken);

        Task<HttpResponseMessage> DeleteAsync(Uri requestUri);

        Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cToken);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, CancellationToken cToken);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, HttpCompletionOption option);

        Task<HttpResponseMessage> SendAsync(HttpRequestMessage message, HttpCompletionOption option, CancellationToken cToken);

        void ClearEndpoints();

        void CancelPendingRequests();
    }
}
