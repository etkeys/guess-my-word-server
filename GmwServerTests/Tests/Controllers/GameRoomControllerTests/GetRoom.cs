using System.Net;
using Moq;
using GmwServer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;

namespace GmwServerTests;

public partial class GameRoomControllerTests
{

    [Theory, MemberData(nameof(GetRoomTestsData))]
    public async Task GetRoomTests(TestCase test){
        _gameRoomServiceMock.Setup(e => e.GetRoomStatus(It.IsAny<GameRoomId>()))
            .ReturnsAsync((IServiceResult)test.Setups["service result"]!);

        var sc = new ServiceCollection();
        sc.AddTransient<IRoomJoinCodeProvider>(_ => new Mock<IRoomJoinCodeProvider>(MockBehavior.Strict).Object);
        sc.AddTransient<IGameRoomService>(_ => _gameRoomServiceMock.Object);

        var actor = new GameRoomController(sc.BuildServiceProvider());
        var a = await actor.GetRoom((Guid)test.Inputs["game room id"]!);

        AssertActionResults(test.Expected, a, nameof(OkObjectResult));

        var expStatusCode = (HttpStatusCode)test.Expected["status"]!;

        // Have to handle OkObjectResult ourselves because of how we do the final
        // equality compare.
        if (a is OkObjectResult oorAct){
            Assert.Equal((int)expStatusCode, oorAct.StatusCode);
            Assert.NotNull(oorAct.Value);
            Assert.IsType<GameRoom>(oorAct.Value);

            var oorExpId = (GameRoomId)test.Expected["value"]!;
            Assert.Equal(oorExpId, ((GameRoom)oorAct.Value).Id);
        }
    }

    public static IEnumerable<object[]> GetRoomTestsData => BundleTestCases(
        new TestCase("Found room")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.OK)
                    .WithData(new GameRoom{
                        Id = new GameRoomId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")),
                        JoinCode = new RoomJoinCode("aaaabbbb"),
                        CreatedDate = DateTime.UtcNow,
                        CurrentWord = null,
                    })
                    .Create())
            .WithInput("game room id", Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5"))
            .WithExpected("status", HttpStatusCode.OK)
            .WithExpected("value", new GameRoomId(Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")))


        // not found
        ,new TestCase("Found room")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.NotFound)
                    .WithError($"Could not find room with id '{Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")}'.")
                    .Create())
            .WithInput("game room id", Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5"))
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithExpected("value", $"Could not find room with id '{Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5")}'.")

        // unexpected result
        ,new TestCase("Unexpected result")
            .WithSetup(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.NoContent)
                    .Create())
            .WithInput("game room id", Guid.Parse("48e29af1-b0b7-45e8-bd56-fe2c882be1a5"))
            .WithExpected("status", HttpStatusCode.InternalServerError)
            .WithExpected("value", "Processing ended with unexpected result: NoContent.")

    );
}