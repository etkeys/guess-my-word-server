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

        var expServiceResult = (IServiceResult<GameRoomId>)test.Expected["service result"]!;

        actServiceResult.Should().Be(
            expServiceResult,
            new ServiceResultEqaulityComparer<GameRoomId>(
                dataComparer: (_, y) => true
            ));

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actRooms = await
            (from r in db.Rooms where r.Id == actServiceResult.Data! select r)
            .ToListAsync();

        if (expServiceResult.IsError){
            actRooms.Should().BeEmpty();
            return;
        }

        var expRoom = (GameRoom)test.Expected["game room"]!;

        actRooms.Should().ContainSingle()
            .And.AllSatisfy(a => {
                a.CreatedDate.Should().BeWithin(1.Minutes()).After(expRoom.CreatedDate);
                a.CreatedByUserId.Should().Be(expRoom.CreatedByUserId);
                a.JoinCode.Should().Be(expRoom.JoinCode);
            });

        (await (from p in db.Players where p.Room == actRooms.First() select p)
            .ToListAsync())
            .Should().ContainSingle()
            .And.AllSatisfy(a => {
                a.UserId.Should().Be(expRoom.CreatedByUserId);
                a.RoomJoinTime.Should().BeWithin(1.Minutes()).After(expRoom.CreatedDate);
                a.IsAsker.Should().BeTrue();
            });
    }

    public static IEnumerable<object[]> CreateRoomTestsData => BundleTestCases(
        new TestCase("Join code doesn't exist")
            .WithSetup(
                "database",
                new Dictionary<string, object[]> {
                    {"Users", new[]{
                        new User{
                            Id = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }}
                }
            )
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<GameRoomId>()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new GameRoomId(Guid.Empty))
                    .Create())
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                CreatedDate = DateTime.UtcNow,
                JoinCode = new RoomJoinCode("ayVN90if")
            })


        ,new TestCase("First join code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")}}},
                    {"Users", new[]{
                        new User{
                            Id = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE"})
            .WithInput("requesting user id", UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<GameRoomId>()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new GameRoomId(Guid.Empty))
                    .Create())
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                CreatedDate = DateTime.UtcNow,
                JoinCode = new RoomJoinCode("t918dhbE")
            })


        ,new TestCase("First join and second code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")},
                        new JoinCode{Id = new RoomJoinCode("t918dhbE")}}},
                    {"Users", new[]{
                        new User{
                            Id = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE", "7agtu991"})
            .WithInput("requesting user id", UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<GameRoomId>()
                    .WithStatus(HttpStatusCode.Created)
                    .WithData(new GameRoomId(Guid.Empty))
                    .Create())
            .WithExpected("game room", new GameRoom{
                Id = new GameRoomId(Guid.Empty),
                CreatedByUserId = UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"),
                CreatedDate = DateTime.UtcNow,
                JoinCode = new RoomJoinCode("7agtu991")
            })


        ,new TestCase("Requesting user does not exist")
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithInput("requesting user id", UserId.FromString("ce568790-e5ae-4b9a-9afd-089703d71b2a"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder<GameRoomId>()
                    .WithStatus(HttpStatusCode.Forbidden)
                    .WithError("Requesting user is not registered.")
                    .Create())
    );
}