using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace KTHub.Core.Client
{
    public sealed class RestClient : IRestClient, IDisposable
    {
        private readonly HttpClient _client;
        private readonly HashSet<Uri> _endpoints;

        public RestClient(IDictionary<string, string> defaultRequestHeaders = null, HttpMessageHandler handler = null, bool disposeHandler = true, TimeSpan? timeout = null, ulong? maxResponseContentBufferSize = null)
        {
            this._client = handler == null ? new HttpClient() : new HttpClient(handler, disposeHandler);
            this.AddDefaultHeaders(defaultRequestHeaders);
            this.AddRequestTimeout(timeout);
            this.AddMaxResponseBufferSize(maxResponseContentBufferSize);
            this._endpoints = new HashSet<Uri>();
        }

        public IDictionary<string, string> DefaultRequestHeaders => (IDictionary<string, string>)this._client.DefaultRequestHeaders.ToDictionary<KeyValuePair<string, IEnumerable<string>>, string, string>((Func<KeyValuePair<string, IEnumerable<string>>, string>)(x => x.Key), (Func<KeyValuePair<string, IEnumerable<string>>, string>)(x => x.Value.First<string>()));

        public TimeSpan Timeout => this._client.Timeout;

        public uint MaxResponseContentBufferSize => (uint)this._client.MaxResponseContentBufferSize;

        public Uri[] Endpoints
        {
            get
            {
                lock (this._endpoints)
                    return this._endpoints.ToArray<Uri>();
            }
        }

        public Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.GetAsync(requestUri);
        }

        public Task<HttpResponseMessage> GetAsync(
          Uri requestUri,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.GetAsync(requestUri, cToken);
        }

        public Task<HttpResponseMessage> GetAsync(
          Uri requestUri,
          HttpCompletionOption option)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.GetAsync(requestUri, option);
        }

        public Task<HttpResponseMessage> GetAsync(
          Uri requestUri,
          HttpCompletionOption option,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.GetAsync(requestUri, option, cToken);
        }

        public Task<HttpResponseMessage> PostAsync(
          Uri requestUri,
          HttpContent httpContent)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.PostAsync(requestUri, httpContent);
        }

        public Task<HttpResponseMessage> PostAsync(
          Uri requestUri,
          HttpContent httpContent,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.PostAsync(requestUri, httpContent, cToken);
        }

        public Task<HttpResponseMessage> PutAsync(
          Uri requestUri,
          HttpContent httpContent)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.PutAsync(requestUri, httpContent);
        }

        public Task<HttpResponseMessage> PutAsync(
          Uri requestUri,
          HttpContent httpContent,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.PutAsync(requestUri, httpContent, cToken);
        }

        public Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.DeleteAsync(requestUri);
        }

        public Task<HttpResponseMessage> DeleteAsync(
          Uri requestUri,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(requestUri);
            return this._client.DeleteAsync(requestUri, cToken);
        }

        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage message)
        {
            this.AddConnectionLeaseTimeout(message.RequestUri);
            return this._client.SendAsync(message);
        }

        public Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage message,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(message.RequestUri);
            return this._client.SendAsync(message, cToken);
        }

        public Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage message,
          HttpCompletionOption option)
        {
            this.AddConnectionLeaseTimeout(message.RequestUri);
            return this._client.SendAsync(message, option);
        }

        public Task<HttpResponseMessage> SendAsync(
          HttpRequestMessage message,
          HttpCompletionOption option,
          CancellationToken cToken)
        {
            this.AddConnectionLeaseTimeout(message.RequestUri);
            return this._client.SendAsync(message, option, cToken);
        }
        public void ClearEndpoints()
        {
            lock (this._endpoints)
                this._endpoints.Clear();
        }

        public void CancelPendingRequests() => this._client.CancelPendingRequests();

        public void Dispose()
        {
            this._client.Dispose();
            lock (this._endpoints)
                this._endpoints.Clear();
        }

        private void AddDefaultHeaders(IDictionary<string, string> headers)
        {
            if (headers == null)
                return;
            foreach (KeyValuePair<string, string> header in (IEnumerable<KeyValuePair<string, string>>)headers)
                this._client.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        private void AddRequestTimeout(TimeSpan? timeout)
        {
            if (!timeout.HasValue)
                return;
            this._client.Timeout = timeout.Value;
        }

        private void AddMaxResponseBufferSize(ulong? size)
        {
            if (!size.HasValue)
                return;
            this._client.MaxResponseContentBufferSize = (long)size.Value;
        }

        private void AddConnectionLeaseTimeout(Uri endpoint)
        {
            lock (this._endpoints)
            {
                if (this._endpoints.Contains(endpoint))
                    return;
                this._endpoints.Add(endpoint);
            }
        }
    }
}
