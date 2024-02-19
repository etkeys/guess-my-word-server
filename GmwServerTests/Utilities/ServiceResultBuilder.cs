
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

    public IServiceResult Create(){
        var mock = new Mock<IServiceResult>();

        mock.Setup(e => e.IsError).Returns((bool)_data["is error"]!);
        mock.Setup(e => e.Status).Returns((HttpStatusCode)_data["status"]!);

        mock.Setup(e => e.GetData()).Returns(_data["data"]);
        mock.Setup(e => e.GetError()).Returns(_data["error"]);

        return mock.Object;
    }

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
}