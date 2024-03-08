
using GmwServer;
using Microsoft.EntityFrameworkCore;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{

    [Theory, MemberData(nameof(JoinRoomTestsData))]
    public async Task JoinRoomTests(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpJoinCode = (RoomJoinCode)test.Inputs["join code"]!;
        var inpUserId = (UserId)test.Inputs["user id"]!;
        var mockJoinCode = (RoomJoinCode)test.Inputs["mock normalized join code"]!;

        _roomJoinCodeProviderMock.Setup(e => e.NormalizeJoinCode(It.IsAny<RoomJoinCode>())).Returns(mockJoinCode);

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.JoinRoom(inpUserId, inpJoinCode, _roomJoinCodeProviderMock.Object);

        var expIsError = (bool)test.Expected["is error"]!;
        var expStatus = (HttpStatusCode)test.Expected["status"]!;

        _roomJoinCodeProviderMock.Verify(e => e.NormalizeJoinCode(It.IsAny<RoomJoinCode>()), Times.Once());

        actServiceResult.IsError.Should().Be(expIsError);
        actServiceResult.Status.Should().Be(expStatus);

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        if (expIsError){
            var expCountPlayer = (int)test.Expected["count player"]!;
            var expError = (string)test.Expected["error"]!;

            actServiceResult.Data.Should().BeNull();
            actServiceResult.Error.Should().NotBeNullOrEmpty()
                .And.Be(expError);

            (await
                (from p in db.Players
                where p.UserId == inpUserId
                select p)
                .CountAsync()
            ).Should().Be(expCountPlayer);

            return;
        }

        var expJoinTime = (DateTime)test.Expected["join time"]!;
        var expJoinTimeIsEarlier = (bool)test.Expected.GetValueOrDefault("join time is earlier", false)!;
        var expRoomId = (GameRoomId)test.Expected["room id"]!;

        actServiceResult.Error.Should().BeNullOrEmpty();
        actServiceResult.Data.Should().NotBeNull()
            .And.Be(expRoomId);

        var actPlayers = await
            (from p in db.Players
            where
                p.RoomId == expRoomId
                && p.UserId == inpUserId
            select p)
            .ToListAsync();
        actPlayers.Should().ContainSingle();

        var actPlayer = actPlayers.First();
        actPlayer.RoomJoinTime.Should().BeWithin(1.Minutes()).After(expJoinTime);

        (await db.GetRoomCurrentAsker(expRoomId))
            .Should().NotBeNull()
            .And.NotBeEquivalentTo(actPlayer, options =>
                options.Including(o => o.RoomId)
                    .Including(o => o.UserId));
    }

    public static IEnumerable<object[]> JoinRoomTestsData => BundleTestCases(
        new TestCase("Joined room successfully")
            .WithInput("user id", UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"))
            .WithInput("join code", new RoomJoinCode("aaaabbEb"))
            .WithInput("mock normalized join code", new RoomJoinCode("aaaabbEb"))
            .WithExpected("is error", false)
            .WithExpected("join time", DateTime.UtcNow)
            .WithExpected("room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("status", HttpStatusCode.Created)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Player is already in the room")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithInput("join code", new RoomJoinCode("aaaabbEb"))
            .WithInput("mock normalized join code", new RoomJoinCode("aaaabbEb"))
            .WithExpected("is error", false)
            .WithExpected("join time", DateTime.UtcNow)
            .WithExpected("room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("status", HttpStatusCode.Created)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Join code not registered")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithInput("join code", new RoomJoinCode("NaaabbEb"))
            .WithInput("mock normalized join code", new RoomJoinCode("NaaabbEb"))
            .WithExpected("count player", 2)
            .WithExpected("error", "Could not find room using given join code.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Join code has invalid character")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithInput("join code", new RoomJoinCode("aa?abbEb"))
            .WithInput("mock normalized join code", new RoomJoinCode("aa.abbEb"))
            .WithExpected("count player", 2)
            .WithExpected("error", "Could not find room using given join code.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Join code is empty string")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithInput("join code", new RoomJoinCode(""))
            .WithInput("mock normalized join code", new RoomJoinCode(""))
            .WithExpected("count player", 2)
            .WithExpected("error", "Could not find room using given join code.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithSetup("database", BasicTestData)

    );
}