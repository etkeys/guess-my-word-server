using System.Net.Mail;
using Microsoft.EntityFrameworkCore;
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{
    [Theory, MemberData(nameof(CreateRoomTestsData))]
    public async Task CreateRoomTests(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpJoinCodes = new Queue<RoomJoinCode>(
            ((string[])test.Inputs["join codes"]!)
                .Select(s => new RoomJoinCode(s)));

        var inpRequestingUserId = (UserId)test.Inputs["requesting user id"]!;

        _roomJoinCodeProviderMock.Setup(e => e.GetRoomJoinCode()).Returns(() => inpJoinCodes.Dequeue());

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.CreateRoom(inpRequestingUserId, _roomJoinCodeProviderMock.Object);

        var expIsError = (bool)test.Expected["is error"]!;
        var expStatus = (HttpStatusCode)test.Expected["status"]!;

        actServiceResult.IsError.Should().Be(expIsError);
        actServiceResult.Status.Should().Be(expStatus);

        if (expIsError){
            var expError = (string)test.Expected["error"]!;

            actServiceResult.Data.Should().BeNull();
            actServiceResult.Error.Should().NotBeNullOrEmpty()
                .And.Be(expError);

            return;
        }

        actServiceResult.Data.Should().NotBeNull();

        var expJoinCode = (RoomJoinCode)test.Expected["join code"]!;
        var expRoom = (GameRoom)test.Expected["game room"]!;

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actRooms = await
            (from r in db.Rooms where r.Id == actServiceResult.Data! select r)
            .ToListAsync();

        actRooms.Should().ContainSingle()
            .And.AllSatisfy(a => {
                a.CreatedDate.Should().BeWithin(1.Minutes()).After(expRoom.CreatedDate);
                a.CreatedByUserId.Should().Be(expRoom.CreatedByUserId);
            });

        (await
            (from j in db.JoinCodes
            where j.Id == expJoinCode
            select j)
            .ToListAsync())
        .Should().ContainSingle("because there should be only one entry in JoinCodes.");

        (await
            (from p in db.Players
            where p.Room == actRooms.First()
            select p)
        .ToListAsync())
        .Should().ContainSingle()
            .And.AllSatisfy(a => {
                a.UserId.Should().Be(expRoom.CreatedByUserId);
                a.RoomJoinTime.Should().BeWithin(1.Minutes()).After(expRoom.CreatedDate);
            });

        (await db.GetRoomCurrentAsker(actRooms.First().Id))
            .Should().NotBeNull()
            .And.BeEquivalentTo(
                new Player{
                    UserId = expRoom.CreatedByUserId
                },
                options => options.Including(o => o.UserId));
    }

    public static IEnumerable<object[]> CreateRoomTestsData => BundleTestCases(
        new TestCase("Join code doesn't exist")
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                CreatedDate = DateTime.UtcNow,
            })
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("ayVN90if"))
            .WithExpected("status", HttpStatusCode.Created)
            .WithSetup("database", BasicTestData)


        ,new TestCase("First join code already exists")
            .WithInput("join codes", new [] {"aaaabbEb", "t918dhbE"})
            .WithInput("requesting user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                CreatedDate = DateTime.UtcNow,
            })
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("t918dhbE"))
            .WithExpected("status", HttpStatusCode.Created)
            .WithSetup("database", BasicTestData)


        ,new TestCase("First join and second code already exists")
            .WithInput("join codes", new [] {"aaaabbEb", "aaaabbNa", "7agtu991"})
            .WithInput("requesting user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                CreatedDate = DateTime.UtcNow,
            })
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("7agtu991"))
            .WithExpected("status", HttpStatusCode.Created)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Requesting user does not exist")
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"))
            .WithExpected("error", "Requesting user is not registered.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.Forbidden)
    );
}