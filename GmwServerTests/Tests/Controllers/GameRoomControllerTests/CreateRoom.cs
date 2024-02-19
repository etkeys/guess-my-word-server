using System.Net;
using Moq;
using GmwServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace GmwServerTests;

public partial class GameRoomControllerTests
{

    [Theory, MemberData(nameof(CreateRoomTestsData))]
    public async Task CreateRoomTests(TestCase test){
        _gameRoomServiceMock.Setup(e => e.CreateRoom(It.IsAny<IRoomJoinCodeProvider>()))
            .ReturnsAsync((IServiceResult)test.Setups["service result"]!);

        var sc = new ServiceCollection();
        sc.AddTransient<IRoomJoinCodeProvider>(_ => new Mock<IRoomJoinCodeProvider>(MockBehavior.Strict).Object);
        sc.AddTransient<IGameRoomService>(_ => _gameRoomServiceMock.Object);

        var actor = new GameRoomController(sc.BuildServiceProvider());
        var a = await actor.CreateRoom();

        AssertActionResults(test.Expected, a);
    }

    public static IEnumerable<object[]> CreateRoomTestsData => BundleTestCases(
        new TestCase("Room created successfully.")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new GameRoomId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")))
                    .Create())
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("type", typeof(GameRoomId))
            .WithExpected("value", new GameRoomId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")))


        ,new TestCase("Unexpected result")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.OK)
                    .Create())
            .WithExpected("status", HttpStatusCode.InternalServerError)
            .WithExpected("value", "Processing ended with unexpected result: OK.")
    );
}