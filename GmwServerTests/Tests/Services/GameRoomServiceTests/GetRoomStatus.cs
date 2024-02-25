
using System.Net.Mail;
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

        var exp = (IServiceResult)testCase.Expected["service result"]!;

        act.Status.Should().Be(exp.Status);
        act.IsError.Should().Be(exp.IsError);

        if (act.IsError){
            act.GetData().Should().BeNull();
            act.GetError().Should().NotBeNull()
                .And.BeOfType<string>();
            ((string)act.GetError()!).Should().NotBeNullOrWhiteSpace();
            return;
        }

        // Don't need to test any further than ID because
        // we are insterting the record...If we don't get
        // the same ID then the rest of the properties are
        // not going to matter.
        act.GetError().Should().BeNull();
        act.GetData().Should().NotBeNull()
            .And.BeOfType<GameRoom>();
        ((GameRoom)act.GetData()!).Id.Should().Be(((GameRoom)exp.GetData()!).Id);
    }

    public static IEnumerable<object[]> GetRoomStatusTestsData => BundleTestCases(
        new TestCase("Found match only one record")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = GameRoomId.FromString("977e4665-6acb-42c3-9259-93933d2f9290"),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedByUserId = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            CreatedDate = DateTime.UtcNow,

                        },
                    }},
                    {"Users", new[] {
                        new User{
                            Id = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }}
                }
            )
            .WithInput("game room id", GameRoomId.FromString("977e4665-6acb-42c3-9259-93933d2f9290"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.OK)
                    .WithData(new GameRoom{
                            Id = GameRoomId.FromString("977e4665-6acb-42c3-9259-93933d2f9290"),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedDate = DateTime.UtcNow,
                        })
                        .Create())


        , new TestCase("Found match many records")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = GameRoomId.FromString("977e4665-6acb-42c3-9259-93933d2f9290"),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedByUserId = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            CreatedDate = DateTime.UtcNow,
                        },
                        new GameRoom{
                            Id = GameRoomId.FromString("0877aa3c-598e-42a2-b3e3-6bea82d42968"),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedByUserId = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            CreatedDate = DateTime.UtcNow,
                        },
                    }},
                    {"Users", new[] {
                        new User{
                            Id = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }}
                }
            )
            .WithInput("game room id", GameRoomId.FromString("0877aa3c-598e-42a2-b3e3-6bea82d42968"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.OK)
                    .WithData(new GameRoom{
                            Id = GameRoomId.FromString("0877aa3c-598e-42a2-b3e3-6bea82d42968"),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedDate = DateTime.UtcNow,
                        })
                        .Create())


        , new TestCase("No match many records")
            .WithSetup(
                "database",
                new Dictionary<string, object[]>{
                    {"Rooms", new[] {
                        new GameRoom{
                            Id = GameRoomId.FromString("977e4665-6acb-42c3-9259-93933d2f9290"),
                            JoinCode = new RoomJoinCode("aaaabbbb"),
                            CreatedByUserId = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            CreatedDate = DateTime.UtcNow,
                        },
                        new GameRoom{
                            Id = GameRoomId.FromString("0877aa3c-598e-42a2-b3e3-6bea82d42968"),
                            JoinCode = new RoomJoinCode("bbbbaaaa"),
                            CreatedByUserId = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            CreatedDate = DateTime.UtcNow,
                        },
                    }},
                    {"Users", new[] {
                        new User{
                            Id = UserId.FromString("13422776-bc7a-4197-aafe-88f972c6ace8"),
                            Email = new MailAddress("john.doe@example.com"),
                            DisplayName = "john.doe"
                        }
                    }}
                }
            )
            .WithInput("game room id", GameRoomId.FromString("0b8b021e-40b5-43f8-a261-7c1c39d3661c"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.NotFound)
                    .WithIsError(true)
                    .Create())


        , new TestCase("No match no records")
            .WithInput("game room id", GameRoomId.FromString("0b8b021e-40b5-43f8-a261-7c1c39d3661c"))
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.NotFound)
                    .WithIsError(true)
                    .Create())

    );
}