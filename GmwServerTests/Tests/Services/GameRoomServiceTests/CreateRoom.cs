using System.Net;
using System.Net.Mail;
using GmwServer;
using Microsoft.EntityFrameworkCore;

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
        var act = await actor.CreateRoom(inpRequestingUserId, _roomJoinCodeProviderMock.Object);

        Assert.Equal((HttpStatusCode)test.Expected["status"]!, act.Status);
        Assert.Equal((bool)test.Expected["is error"]!, act.IsError);

        if (act.IsError){
            Assert.Null(act.GetData());
            Assert.NotNull(act.GetError());
            Assert.Equal(test.Expected["error"]!, act.GetError());
            return;
        }

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actRooms = await (from r in db.Rooms where r.Id == ((GameRoomId)act.GetData()!) select r).ToListAsync();
        Assert.Single(actRooms);

        var actRoom = actRooms.First();

        var expCreatedDate = (DateTime)test.Expected["created date"]!;
        var expCreatedByUserId = (UserId)test.Expected["created by user"]!;
        var expJoinCode = (RoomJoinCode)test.Expected["join code"]!;

        Assert.Equal(expCreatedDate.Date, actRoom.CreatedDate.Date);
        Assert.Equal(expCreatedByUserId, actRoom.CreatedByUserId);
        Assert.Equal(expJoinCode, actRoom.JoinCode);

        var actPlayers = await (from p in db.Players where p.Room == actRoom select p).ToListAsync();
        Assert.Single(actPlayers);

        var actPlayer = actPlayers.First();

        Assert.Equal(expCreatedByUserId, actPlayer.UserId);
        Assert.Equal(expCreatedDate.Date, actPlayer.RoomJoinTime.Date);
        Assert.True(actPlayer.IsAsker);
    }

    public static IEnumerable<object[]> CreateRoomTestsData => BundleTestCases(
        new TestCase("Join code doesn't exist")
            .WithSetup(
                "database",
                new Dictionary<string, object[]> {
                    {"Users", new[]{
                        new User{
                            Id = new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }}
                }
            )
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("created date", DateTime.UtcNow)
            .WithExpected("created by user", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("ayVN90if"))
            .WithExpected("status", HttpStatusCode.Created)


        ,new TestCase("First join code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")}}},
                    {"Users", new[]{
                        new User{
                            Id = new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE"})
            .WithInput("requesting user id", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("created date", DateTime.UtcNow)
            .WithExpected("created by user", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("t918dhbE"))
            .WithExpected("status", HttpStatusCode.Created)


        ,new TestCase("First join and second code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")},
                        new JoinCode{Id = new RoomJoinCode("t918dhbE")}}},
                    {"Users", new[]{
                        new User{
                            Id = new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE", "7agtu991"})
            .WithInput("requesting user id", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("created date", DateTime.UtcNow)
            .WithExpected("created by user", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("is error", false)
            .WithExpected("join code", new RoomJoinCode("7agtu991"))
            .WithExpected("status", HttpStatusCode.Created)


        ,new TestCase("Requesting user does not exist")
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", new UserId(Guid.Parse("ce568790-e5ae-4b9a-9afd-089703d71b2a")))
            .WithExpected("is error", true)
            .WithExpected("error", "Requesting user is not registered.")
            .WithExpected("status", HttpStatusCode.Forbidden)
    );
}