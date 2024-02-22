
using System.Net;

namespace GmwServer;

public static class ServiceResults
{

    public static IServiceResult Created() =>
        new SuccessfulServiceResult(HttpStatusCode.Created);

    public static IServiceResult Created<T>(T data) =>
        new SuccessfulServiceResult<T>(HttpStatusCode.Created, data);

    public static IServiceResult Forbidden(string error) =>
        new ErrorServiceResult(HttpStatusCode.Forbidden, error);

    public static IServiceResult NotFound(string error) =>
        new ErrorServiceResult(HttpStatusCode.NotFound, error);

    public static IServiceResult Ok<T>(T data) =>
        new SuccessfulServiceResult<T>(HttpStatusCode.OK, data);

    public static IServiceResult UnprocessableEntity(string error) =>
        new ErrorServiceResult(HttpStatusCode.UnprocessableEntity, error);
}

public class ErrorServiceResult: IServiceResult
{

    public ErrorServiceResult(HttpStatusCode status, string error){
        Error = error;
        Status = status;
    }

    public object Error {get;}
    public bool IsError => true;

    public HttpStatusCode Status {get;}

    public object? GetData() => null;

    public object? GetError() => Error;

}

public class SuccessfulServiceResult: IServiceResult
{
    public SuccessfulServiceResult(HttpStatusCode status){
        Status = status;
    }

    public bool IsError => false;

    public HttpStatusCode Status {get;}

    public object? GetData() => null;

    public object? GetError() => null;
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
