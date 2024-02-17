
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{

    [Theory, MemberData(nameof(GetRoomStatusTestsData))]
    public async Task GetRoomStatusTests(TestCase testCase){
        var inpRoomId = (GameRoomId)testCase.Inputs["game room id"]!;

        await SetupDatabase(DefaultDbContextOptions, testCase.Setups);

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var act = await actor.GetRoomStatus(inpRoomId);

        var exp = (GameRoom)testCase.Expected["game room"]!;

        if (exp is null){
            Assert.Null(act);
            return;
        }

        // Don't need to test any further than ID because
        // we are insterting the record...If we don't get
        // the same ID then the rest of the properties are
        // not going to matter.
        Assert.Equal(exp.Id, act!.Id);
    }

    public static IEnumerable<object[]> GetRoomStatusTestsData => BundleTestCases(
        new TestCase("Found match only one record")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("977e4665-6acb-42c3-9259-93933d2f9290")),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        },
                    }}
                }
            )
            .WithInput("game room id", new GameRoomId(Guid.Parse("977e4665-6acb-42c3-9259-93933d2f9290")))
            .WithExpected("game room", 
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("977e4665-6acb-42c3-9259-93933d2f9290")),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        })


        , new TestCase("Found match many records")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("977e4665-6acb-42c3-9259-93933d2f9290")),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        },
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("0877aa3c-598e-42a2-b3e3-6bea82d42968")),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        },
                    }}
                }
            )
            .WithInput("game room id", new GameRoomId(Guid.Parse("0877aa3c-598e-42a2-b3e3-6bea82d42968")))
            .WithExpected("game room", 
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("0877aa3c-598e-42a2-b3e3-6bea82d42968")),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        })


        , new TestCase("No match many records")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("977e4665-6acb-42c3-9259-93933d2f9290")),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        },
                        new GameRoom{
                            Id = new GameRoomId(Guid.Parse("0877aa3c-598e-42a2-b3e3-6bea82d42968")),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedDate = DateTime.UtcNow,
                            CurrentWord = null,
                        },
                    }}
                }
            )
            .WithInput("game room id", new GameRoomId(Guid.Parse("0b8b021e-40b5-43f8-a261-7c1c39d3661c")))
            .WithExpected("game room", null)


        , new TestCase("No match no records")
            .WithInput("game room id", new GameRoomId(Guid.Parse("0b8b021e-40b5-43f8-a261-7c1c39d3661c")))
            .WithExpected("game room", null)

    );
}