using System.Net;

public class ApiResponse<T>
{
    public ApiResponse() => this.Code = 200;

    public int Code { get; set; }

    public HttpStatusCode Status => (HttpStatusCode)this.Code;

    public string Message { get; set; }

    public string SystemMessage { get; set; }

    public T Data { get; set; }

    public bool IsSuccess => this.Code.Equals(200);
}
