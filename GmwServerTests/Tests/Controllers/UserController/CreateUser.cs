
using System.Net.Mail;
using Moq;
using GmwServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace GmwServerTests;

public partial class UserControllerTests
{
    [Theory, MemberData(nameof(CreateUserTestsData))]
    public async Task CreateUserTests(TestCase test) {
        _userServiceMock.Setup(e => e.CreateUser(It.IsAny<MailAddress>()))
            .ReturnsAsync((IServiceResult)test.Setups["service result"]!);

        var sc = new ServiceCollection();
        sc.AddTransient<IUserService>(_ => _userServiceMock.Object);

        var inpEmail = (string)test.Inputs["email"]!;

        var actor = new UserController(sc.BuildServiceProvider());
        var a = await actor.CreateUser(inpEmail);

        AssertActionResults(test.Expected, a);
    }


    public static IEnumerable<object[]> CreateUserTestsData => BundleTestCases(
        new TestCase("Created-User created")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
                    .Create())
            .WithInput("email", "john.doe@example.com")
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("type", typeof(UserId))
            .WithExpected("value", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))


        ,new TestCase("Bad Request-Provided email address is null.")
            .WithSetup("service result", null)
            .WithInput("email", null)
            .WithExpected("status", HttpStatusCode.BadRequest)
            .WithExpected("value", "Email address not provided.")


        ,new TestCase("Bad Request-Provided email address is whitespace.")
            .WithSetup("service result", null)
            .WithInput("email", "  ")
            .WithExpected("status", HttpStatusCode.BadRequest)
            .WithExpected("value", "Email address not provided.")


        ,new TestCase("Bad Request-Provided email address is not well formed.")
            .WithSetup("service result", null)
            .WithInput("email", "john.doe@email@example.com")
            .WithExpected("status", HttpStatusCode.BadRequest)
            .WithExpected("value", "Email address is not in the proper form.")


        ,new TestCase("Unprocessable-Email already exists")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("Email address already registered.")
                    .Create())
            .WithInput("email", "john.doe@example.com")
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithExpected("value", "Email address already registered.")


        ,new TestCase("Problem-unexpected result")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.OK)
                    .Create())
            .WithInput("email", "john.doe@example.com")
            .WithExpected("status", HttpStatusCode.InternalServerError)
            .WithExpected("value", "Processing ended with unexpected result: OK.")
    );
}