using System;
using System.Net;
using System.Threading.Tasks;
using KTHub.Core.Client.Models;
using KTHub.Core.Client.Serializer;
using KTHub.Core.Helper;

namespace KTHub.Core.Client
{
    public class BaseApiClient
    {
        private IHttpApiClient httpClient;

        protected ApiConfigs ApiConfigs { get; set; }

        protected string ApiResponseMessage { get; set; }

        protected HttpStatusCode ApiResponseStatus { get; set; }

        protected bool IsExcuteError => this.ApiResponseStatus != HttpStatusCode.OK;

        protected string GetUrlSend(string controllerUri)
        {
            if (this.ApiConfigs.IsNull()) {
                throw new Exception("ApiConfigs is not null");
            }
            return this.ApiConfigs.AppHostUri.CombineUriToString(controllerUri);
        }

        #region ctor
        public BaseApiClient(ApiConfigs apiConfigs)
        {
            ApiConfigs = !apiConfigs.IsNull() ? apiConfigs : throw new Exception("ApiConfigs is not null");
            httpClient = new HttpApiClient(apiConfigs);
        }
        #endregion

        #region Async Function
        protected virtual async Task<ApiResponse<TResponse>> GetAsync<TResponse, TRequest>(string urlSend, TRequest model, ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.GET, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(string urlSend, TRequest model, ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.POST, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(string urlSend, TRequest model, ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.PUT, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> DeleteAsync<TResponse, TRequest>(string urlSend, TRequest model, ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.DELETE, model, responseData);
            return apiResponse;
        }
        #endregion


        private async Task<ApiResponse<TResponse>> SendAsync<TResponse, TRequest>(string urlSend, HttpApiMethod method, TRequest model, ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse;
            try
            {
                ApiResponseMessage = string.Empty;
                ApiResponseStatus = HttpStatusCode.InternalServerError;
                ResponseModel respond = await this.httpClient.ExcuteAsync<TRequest>(urlSend, method, model, this.ApiConfigs.AppId, this.ApiConfigs.AppPublicKey);
                if (respond != null)
                {
                    ApiResponseMessage = respond.ResponseMessage;
                    ApiResponseStatus = respond.ResponseStatus;
                    if (respond.ResponseStatus == HttpStatusCode.OK)
                    {
                        responseData = new ApiResponse<TResponse>()
                        {
                            Message = respond.ResponseData == null ? "No data found" : "The request has been met",
                            Code = (int)respond.ResponseStatus,
                            SystemMessage = respond.ResponseData != null ? "The request has been met" : string.Empty,
                            Data = respond.ResponseData != null ? SerializerHelpers.Deserialize<TResponse>(respond.ResponseData, this.ApiConfigs.IsEncryptEnable ? this.ApiConfigs.AppSecret : (string)null) : default(TResponse)
                        };
                    }
                    else
                    {
                        responseData = new ApiResponse<TResponse>()
                        {
                            Message = respond.ResponseMessage,
                            Code = (int)respond.ResponseStatus,
                            SystemMessage = "An error occurred while processing Service Api",
                            Data = default(TResponse)
                        };
                    }
                }
                else
                {
                    ApiResponseMessage = "Unable to connect to Service API";
                    ApiResponseStatus = HttpStatusCode.ServiceUnavailable;
                    responseData = new ApiResponse<TResponse>()
                    {
                        Message = "",
                        Code = (int)ApiResponseStatus,
                        SystemMessage = ApiResponseMessage,
                        Data = default(TResponse)
                    };
                }
                apiResponse = responseData;
            }
            //catch (Exception ex)
            //{
            //    ex.AddSource(string.Format((string)ConstValue.ErrorSourceFormat, (object)this.GetType().Name, (object)MethodExtensions.GetCallerMemberName(nameof(SendAsync))));
            //    throw ex;
            //}
            catch (Exception ex)
            {
                //string errorSource = string.Format((string)ConstValue.ErrorSourceFormat, (object)this.GetType().Name, (object)MethodExtensions.GetCallerMemberName(nameof(SendAsync)));
                //throw new KTHubException((ErrorSeverity)4, (ErrorCode)5, errorSource, ex.Message, ex.StackTrace);
                throw new Exception("SendAsync get error with Ex: " + ex + "\nApiResponseStatus: " + ApiResponseStatus + "\nApiResponseMessage: " + ApiResponseMessage);
            }
            return apiResponse;
        }
    }
}
