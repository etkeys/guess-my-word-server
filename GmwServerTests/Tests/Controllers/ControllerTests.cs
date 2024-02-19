
using System.Net;
using Microsoft.AspNetCore.Mvc;
using ExpectedCollection = System.Collections.Generic.Dictionary<string, object?>;

namespace GmwServerTests;

public class ControllerTests: BaseTests
{
    protected void AssertActionResults(
        ExpectedCollection expected,
        IActionResult actual,
        params string[] dontHandle){
        switch(actual){
            case BadRequestObjectResult brorAct:
                if(!dontHandle.Contains(nameof(BadRequestObjectResult)))
                    AssertBadRequestObjectResult(expected, brorAct);
                break;
            case CreatedAtActionResult caarAct:
                if(!dontHandle.Contains(nameof(CreatedAtActionResult)))
                AssertCreatedAtActionResult(expected, caarAct);
                break;
            case NotFoundObjectResult nforAct:
                if(!dontHandle.Contains(nameof(NotFoundObjectResult)))
                AssertNotFoundObjecResult(expected, nforAct);
                break;
            case OkObjectResult oorAct:
                if(!dontHandle.Contains(nameof(OkObjectResult)))
                    AssertOkObjectResult(expected, oorAct);
                break;
            case UnprocessableEntityObjectResult ueorAct:
                if(!dontHandle.Contains(nameof(UnprocessableEntityObjectResult)))
                    AssertUnprocessableEntityObjectResult(expected, ueorAct);
                break;
            case ObjectResult orAct:
                if(!dontHandle.Contains(nameof(ObjectResult)))
                AssertObjectResult(expected, orAct);
                break;
            default:
                Assert.Fail($"Result type not handled: {actual.GetType()}");
                break;
        }
    }

    protected virtual void AssertBadRequestObjectResult(ExpectedCollection expected, BadRequestObjectResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType<string>(actual.Value);

            var expValue = (string)expected["value"]!;
            Assert.Equal(expValue, (string)actual.Value);
    }

    protected virtual void AssertCreatedAtActionResult(ExpectedCollection expected, CreatedAtActionResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;
            var expType = (Type)expected["type"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType(expType, actual.Value);

            var expValue = Convert.ChangeType(expected["value"]!, expType);
            var actValue = Convert.ChangeType(actual.Value, expType);
            Assert.Equal(expValue, actValue);
    }

    protected virtual void AssertNotFoundObjecResult(ExpectedCollection expected, NotFoundObjectResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType<string>(actual.Value);
            Assert.False(string.IsNullOrWhiteSpace((string)actual.Value));

            var expValue = (string)expected["value"]!;
            Assert.Equal(expValue, actual.Value);
    }

    protected virtual void AssertObjectResult(ExpectedCollection expected, ObjectResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType<ProblemDetails>(actual.Value);

            var expValue = (string)expected["value"]!;
            Assert.Equal(expValue, ((ProblemDetails)actual.Value).Detail);
    }

    protected virtual void AssertOkObjectResult(ExpectedCollection expected, OkObjectResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;
            var expType = (Type)expected["type"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType(expType, actual.Value);

            var expValue = Convert.ChangeType(expected["value"]!, expType);
            var actValue = Convert.ChangeType(actual, expType);
            Assert.Equal(expValue, actValue);
    }

    protected virtual void AssertUnprocessableEntityObjectResult(ExpectedCollection expected, UnprocessableEntityObjectResult actual){
            var expStatusCode = (HttpStatusCode)expected["status"]!;

            Assert.Equal((int)expStatusCode, actual.StatusCode);
            Assert.NotNull(actual.Value);
            Assert.IsType<string>(actual.Value);

            var expValue = (string)expected["value"]!;
            Assert.Equal(expValue, (string)actual.Value);
    }
}