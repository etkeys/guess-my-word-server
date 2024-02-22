
using System.Net;
using Moq;
using GmwServer;

namespace GmwServerTests;

public class ServiceResultBuilder
{
    private readonly Dictionary<string, object?> _data = new();
    public ServiceResultBuilder(){
        _data.Add("data", null);
        _data.Add("error", null);
        _data.Add("is error", false);
    }

    public IServiceResult Create() =>
        new MockServiceResult{
            Data = _data["data"],
            Error = _data["error"],
            IsError = (bool)_data["is error"]!,
            Status = (HttpStatusCode)_data["status"]!
        };

    public ServiceResultBuilder WithData(object? newValue, bool updateExclusiveValues = true){
        _data["data"] = newValue;

        if (updateExclusiveValues){
            _data["is error"] = false;
            _data["error"] = null;
        }

        return this;
    }

    public ServiceResultBuilder WithError(object? newValue, bool updateExclusiveValues = true){
        _data["error"] = newValue;

        if (updateExclusiveValues){
            _data["is error"] = true;
            _data["data"] = null;
        }

        return this;
    }

    public ServiceResultBuilder WithIsError(bool newValue){
        _data["is error"] = newValue;
        return this;
    }

    public ServiceResultBuilder WithStatus(HttpStatusCode newValue){
        if (!_data.TryAdd("status", newValue))
            _data["status"] = newValue;

        return this;
    }

    private class MockServiceResult: IServiceResult
    {
        public bool IsError {get; init;}
        public HttpStatusCode Status {get; init;}

        public object? Data {get; init;}

        public object? Error{get; init;}

        public object? GetData() => Data;

        public object? GetError() => Error;
    }
}