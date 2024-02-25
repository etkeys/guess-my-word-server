
using System.Net;

namespace GmwServer;

public static class ServiceResults
{
    public static IServiceResult<T> Created<T>(T data) =>
        new ConcreteObjectServiceResult<T>(data, HttpStatusCode.Created);

    public static IServiceResult<T> Forbidden<T>(string error) =>
        new ConcreteObjectServiceResult<T>(HttpStatusCode.Forbidden, error);

    public static IServiceResult<T> NotFound<T>(string error) =>
        new ConcreteObjectServiceResult<T>(HttpStatusCode.NotFound, error);

    public static IServiceResult<T> Ok<T>(T data) =>
        new ConcreteObjectServiceResult<T>(data, HttpStatusCode.OK);

    public static IServiceResult<T> UnprocessableEntity<T>(string error) =>
        new ConcreteObjectServiceResult<T>(HttpStatusCode.UnprocessableEntity, error);

    private class ConcreteServiceResult: IServiceResult
    {
        public ConcreteServiceResult(HttpStatusCode status){
            IsError = false;
            Status = status;
        }

        public ConcreteServiceResult(HttpStatusCode status, string error){
            Error = error;
            IsError = true;
            Status = status;
        }

        public string? Error {get;}
        public bool IsError {get;}
        public HttpStatusCode Status {get;}
    }

    private class ConcreteObjectServiceResult<T> : IServiceResult<T>
    {
        public ConcreteObjectServiceResult(T data, HttpStatusCode status){
            Data = data;
            Error = null;
            Status = status;
        }

        public ConcreteObjectServiceResult(HttpStatusCode status, string error){
            Data = default(T);
            Error = error;
            Status = status;
        }
        public T? Data {get;}
        public string? Error {get;}
        public bool IsError => !string.IsNullOrWhiteSpace(Error);
        public HttpStatusCode Status {get;}
    }
}


public interface IServiceResult
{
    string? Error {get;}
    bool IsError {get;}
    HttpStatusCode Status{get;}
}

public interface IServiceResult<T>: IServiceResult
{
    T? Data {get;}
}
