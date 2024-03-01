
using System.Net;
using Moq;
using GmwServer;

namespace GmwServerTests;

[Obsolete("Store properties individually")]
public class ServiceResultBuilder<T>
{
    private readonly Dictionary<string, object?> _data = new();
    public ServiceResultBuilder(){
        _data.Add("data", null);
        _data.Add("error", null);
        _data.Add("is error", false);
    }

    public IServiceResult<T> Create() =>
        new MockServiceResult{
            Data = (T)_data["data"]!,
            Error = (string)_data["error"]!,
            IsError = (bool)_data["is error"]!,
            Status = (HttpStatusCode)_data["status"]!
        };

    public ServiceResultBuilder<T> WithData(object? newValue, bool updateExclusiveValues = true){
        _data["data"] = newValue;

        if (updateExclusiveValues){
            _data["is error"] = false;
            _data["error"] = null;
        }

        return this;
    }

    public ServiceResultBuilder<T> WithError(object? newValue, bool updateExclusiveValues = true){
        _data["error"] = newValue;

        if (updateExclusiveValues){
            _data["is error"] = true;
            _data["data"] = null;
        }

        return this;
    }

    public ServiceResultBuilder<T> WithIsError(bool newValue){
        _data["is error"] = newValue;
        return this;
    }

    public ServiceResultBuilder<T> WithStatus(HttpStatusCode newValue){
        if (!_data.TryAdd("status", newValue))
            _data["status"] = newValue;

        return this;
    }


    private class MockServiceResult: IServiceResult<T>
    {
        public T? Data {get; init;}
        public string? Error {get; init;}
        public bool IsError {get; init;}
        public HttpStatusCode Status {get; init;}
    }
}