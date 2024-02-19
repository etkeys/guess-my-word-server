
using System.Net;

namespace GmwServer;

public static class ServiceResults
{
    public static IServiceResult Created<T>(T data) =>
        new SuccessfulServiceResult<T>(HttpStatusCode.Created, data);

    public static IServiceResult NotFound(string error) =>
        new ErrorServiceResult(HttpStatusCode.NotFound, error);

    public static IServiceResult Ok<T>(T data) =>
        new SuccessfulServiceResult<T>(HttpStatusCode.OK, data);

    public static IServiceResult UnprocessableEntity(string error) =>
        new ErrorServiceResult(HttpStatusCode.UnprocessableEntity, error);
}

public class ErrorServiceResult: IServiceResult
{
    private string _error;

    public ErrorServiceResult(HttpStatusCode status, string error){
        _error = error;
        Status = status;
    }

    public bool IsError => true;

    public HttpStatusCode Status {get;}

    public object? GetData() => null;

    public object? GetError() => _error;

}

public class SuccessfulServiceResult<T>: IServiceResult
{
    private readonly T _data;
    public SuccessfulServiceResult(HttpStatusCode status, T data){
        _data = data;
        Status = status;
    }
    public bool IsError => false;

    public HttpStatusCode Status {get;}

    public object? GetData() => _data;

    public object? GetError() => null;

}

public interface IServiceResult
{
    bool IsError {get;}
    HttpStatusCode Status{get;}

    object? GetError();

    object? GetData();
}
