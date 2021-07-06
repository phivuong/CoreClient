using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using KTHub.Core.Client.Models;
using KTHub.Core.Client.Serializer;
using KTHub.Core.Helper;
using KTHub.Core.Logging;
using KTHub.Core.Security;

namespace KTHub.Core.Client
{
    public class HttpApiClient : IHttpApiClient
    {
        private ApiConfigs _apiConfigs;
        private static IRestClient restClient = (IRestClient)new RestClient();

        public HttpApiClient(ApiConfigs apiConfigs) => this._apiConfigs = apiConfigs;

        public async Task<ResponseModel> ExcuteAsync<T>(
          string urlSend,
          HttpApiMethod method,
          T data,
          string appId,
          string publicKey)
        {
            try
            {
                bool isRequestQueryString = method.Equals((object)HttpApiMethod.GET) || method.Equals((object)HttpApiMethod.DELETE);
                string queryString = isRequestQueryString ? ((object)(T)data).ToQueryString() : (string)null;
                string saltValue = KTHubCrytography.GenerateNonce();
                string signature = this._apiConfigs.IsEncryptEnable ? KTHubCrytography.EncryptRSA(saltValue, publicKey) : string.Empty;
                Type t = data.GetType();
                PropertyInfo prop = t.GetProperty("TypeName");
                string typeName = t.IsEnum ? data.ToString() : (prop.IsNotNull() ? prop.GetValue((object)(T)data).ToString() : string.Empty);
                string queryStringEncrypt = this._apiConfigs.IsEncryptEnable ? KTHubCrytography.EncryptToString(queryString, saltValue) : queryString;
                RequestModel reqModel = new RequestModel()
                {
                    AppId = appId,
                    Signature = signature,
                    Data = isRequestQueryString || (object)(T)data == null ? (byte[])null : SerializerHelpers.Serialize<T>(data, this._apiConfigs.IsEncryptEnable ? saltValue : (string)null),
                    RequestParams = isRequestQueryString ? queryStringEncrypt : (string)null,
                    TypeName = typeName
                };
                ResponseModel responseModel = await this.ExcuteAsync(urlSend, reqModel, method);
                return responseModel;
            }
            catch (Exception ex)
            {
                string exMessage = "ExcuteAsync:" + urlSend + "." + ex.Message;
                InvokeLogging.WriteLog(new Exception(exMessage, ex.InnerException));
                return (ResponseModel)null;
            }
        }

        private async Task<ResponseModel> ExcuteAsync(
          string urlSend,
          RequestModel reqObj,
          HttpApiMethod method)
        {
            try
            {
                byte[] responseData = (byte[])null;
                HttpResponseMessage response = (HttpResponseMessage)null;
                HttpApiMethod httpApiMethod = method;
                switch (httpApiMethod)
                {
                    case HttpApiMethod.DELETE:
                    case HttpApiMethod.GET:
                        string queryString = urlSend + "?" + reqObj.ToQueryString();
                        if (method.Equals((object)HttpApiMethod.GET))
                            response = await HttpApiClient.restClient.GetAsync(new Uri(queryString));
                        else
                            response = await HttpApiClient.restClient.DeleteAsync(new Uri(queryString));
                        queryString = (string)null;
                        break;
                    case HttpApiMethod.POST:
                    case HttpApiMethod.PUT:
                        byte[] byteData = SerializerHelpers.Serialize<RequestModel>(reqObj, (string)null);
                        ByteArrayContent byteContent = new ByteArrayContent(byteData);
                        if (method.Equals((object)HttpApiMethod.POST))
                            response = await HttpApiClient.restClient.PostAsync(new Uri(urlSend), (HttpContent)byteContent);
                        else
                            response = await HttpApiClient.restClient.PutAsync(new Uri(urlSend), (HttpContent)byteContent);
                        byteData = (byte[])null;
                        byteContent = (ByteArrayContent)null;
                        break;
                }
                if (response != null && response.IsSuccessStatusCode)
                    responseData = await response.Content.ReadAsByteArrayAsync();
                return responseData != null ? SerializerHelpers.Deserialize<ResponseModel>(responseData, (string)null) : (ResponseModel)null;
            }
            catch (Exception ex)
            {
                string exMessage = "Excute:" + urlSend + "." + ex.Message;
                InvokeLogging.WriteLog(new Exception(exMessage, ex.InnerException));
                return (ResponseModel)null;
            }
        }
    }
}
