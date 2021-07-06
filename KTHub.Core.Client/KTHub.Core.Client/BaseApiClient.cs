using System;
using System.Net;
using System.Threading.Tasks;
using KTHub.Core.Client.Models;
using KTHub.Core.Client.Serializer;
using KTHub.Core.Helper;
using KTHub.Core.Logging;

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
            if (this.ApiConfigs.IsNull())
                throw new Exception("ApiConfigs is not null");
            return this.ApiConfigs.AppHostUri.CombineUriToString(controllerUri);
        }

        public BaseApiClient(ApiConfigs apiConfigs)
        {
            this.ApiConfigs = !apiConfigs.IsNull() ? apiConfigs : throw new Exception("ApiConfigs is not null");
            this.httpClient = (IHttpApiClient)new HttpApiClient(apiConfigs);
        }

        protected virtual async Task<ApiResponse<TResponse>> GetAsync<TResponse, TRequest>(
          string urlSend,
          TRequest model,
          ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.GET, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> PostAsync<TResponse, TRequest>(
          string urlSend,
          TRequest model,
          ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.POST, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> PutAsync<TResponse, TRequest>(
          string urlSend,
          TRequest model,
          ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.PUT, model, responseData);
            return apiResponse;
        }

        protected virtual async Task<ApiResponse<TResponse>> DeleteAsync<TResponse, TRequest>(
          string urlSend,
          TRequest model,
          ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse = await this.SendAsync<TResponse, TRequest>(urlSend, HttpApiMethod.DELETE, model, responseData);
            return apiResponse;
        }

        private async Task<ApiResponse<TResponse>> SendAsync<TResponse, TRequest>(
          string urlSend,
          HttpApiMethod method,
          TRequest model,
          ApiResponse<TResponse> responseData = null)
        {
            ApiResponse<TResponse> apiResponse;
            try
            {
                this.ApiResponseMessage = string.Empty;
                this.ApiResponseStatus = HttpStatusCode.InternalServerError;
                ResponseModel respond = await this.httpClient.ExcuteAsync<TRequest>(urlSend, method, model, this.ApiConfigs.AppId, this.ApiConfigs.AppPublicKey);
                if (respond != null)
                {
                    this.ApiResponseMessage = respond.ResponseMessage;
                    this.ApiResponseStatus = respond.ResponseStatus;
                    if (respond.ResponseStatus == HttpStatusCode.OK)
                        responseData = new ApiResponse<TResponse>()
                        {
                            Message = respond.ResponseData == null ? "Không tìm thấy dữ liệu" : "Đã đáp ứng yêu cầu",
                            Code = (int)respond.ResponseStatus,
                            SystemMessage = respond.ResponseData != null ? "Đã đáp ứng yêu cầu" : string.Empty,
                            Data = respond.ResponseData != null ? SerializerHelpers.Deserialize<TResponse>(respond.ResponseData, this.ApiConfigs.IsEncryptEnable ? this.ApiConfigs.AppSecret : (string)null) : default(TResponse)
                        };
                    else
                        responseData = new ApiResponse<TResponse>()
                        {
                            Message = respond.ResponseMessage,
                            Code = (int)respond.ResponseStatus,
                            SystemMessage = "Xảy ra lỗi trong quá trình xử lý Service Api",
                            Data = default(TResponse)
                        };
                }
                else
                {
                    this.ApiResponseMessage = "Không kết nối được Service Api";
                    this.ApiResponseStatus = HttpStatusCode.ServiceUnavailable;
                    responseData = new ApiResponse<TResponse>()
                    {
                        Message = "",
                        Code = (int)this.ApiResponseStatus,
                        SystemMessage = this.ApiResponseMessage,
                        Data = default(TResponse)
                    };
                }
                apiResponse = responseData;
            }
            catch (KTHubException ex)
            {
                ex.AddSource(string.Format((string)ConstValue.ErrorSourceFormat, (object)this.GetType().Name, (object)MethodExtensions.GetCallerMemberName(nameof(SendAsync))));
                throw ex;
            }
            catch (Exception ex)
            {
                string errorSource = string.Format((string)ConstValue.ErrorSourceFormat, (object)this.GetType().Name, (object)MethodExtensions.GetCallerMemberName(nameof(SendAsync)));
                throw new KTHubException((ErrorSeverity)4, (ErrorCode)5, errorSource, ex.Message, ex.StackTrace);
            }
            return apiResponse;
        }
    }
}
