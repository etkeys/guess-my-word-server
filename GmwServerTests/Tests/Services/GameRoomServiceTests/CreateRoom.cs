using System.Net;
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{
    [Theory, MemberData(nameof(CreateRoomTestsData))]
    public async Task CreateRoomTests(TestCase testCase){

        var inpJoinCodes = new Queue<RoomJoinCode>(
            ((string[])testCase.Inputs["join codes"]!)
                .Select(s => new RoomJoinCode(s)));

        await SetupDatabase(DefaultDbContextOptions, testCase.Setups);

        _roomJoinCodeProviderMock.Setup(e => e.GetRoomJoinCode()).Returns(() => inpJoinCodes.Dequeue());

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var act = await actor.CreateRoom(_roomJoinCodeProviderMock.Object);

        Assert.Equal(HttpStatusCode.Created, act.Status);
        Assert.False(act.IsError);
        Assert.NotNull(act.GetData());
        Assert.Null(act.GetError());

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actRooms = from r in db.Rooms where r.Id == ((GameRoomId)act.GetData()!) select r;
        Assert.Single(actRooms);

        var actRoom = actRooms.First();

        var expCreatedDate = (DateTime)testCase.Expected["created date"]!;
        var expJoinCode = (RoomJoinCode)testCase.Expected["join code"]!;

        Assert.Equal(expCreatedDate, actRoom.CreatedDate.Date);
        Assert.Equal(expJoinCode, actRoom.JoinCode);
        Assert.Null(actRoom.CurrentWord);
    }

    public static IEnumerable<object[]> CreateRoomTestsData => BundleTestCases(
        new TestCase("Join code doesn't exist")
            .WithInput("join codes", new [] {"ayVN90if"})
            .WithExpected("created date", DateTime.UtcNow.Date)
            .WithExpected("join code", new RoomJoinCode("ayVN90if"))


        ,new TestCase("First join code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")}}},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE"})
            .WithExpected("created date", DateTime.UtcNow.Date)
            .WithExpected("join code", new RoomJoinCode("t918dhbE"))


        ,new TestCase("First join code already exists")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"GeneratedRoomJoinCodes", new []{
                        new JoinCode{Id = new RoomJoinCode("ayVN90if")},
                        new JoinCode{Id = new RoomJoinCode("t918dhbE")}}},
            })
            .WithInput("join codes", new [] {"ayVN90if", "t918dhbE", "7agtu991"})
            .WithExpected("created date", DateTime.UtcNow.Date)
            .WithExpected("join code", new RoomJoinCode("7agtu991"))
    );
}