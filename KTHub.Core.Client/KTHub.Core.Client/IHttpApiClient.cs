using System;
using System.Threading.Tasks;
using KTHub.Core.Client.Models;

namespace KTHub.Core.Client
{
    public interface IHttpApiClient
    {
        Task<ResponseModel> ExcuteAsync<T>(
          string urlSend,
          HttpApiMethod method,
          T data,
          string appId,
          string publicKey);
    }
}
