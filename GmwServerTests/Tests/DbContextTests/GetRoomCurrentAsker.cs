

namespace GmwServerTests;

public partial class DbContextTests
{
    [Theory, MemberData(nameof(GetRoomCurrentAskerTestsData))]
    public async Task GetRoomCurrentAskerTests(TestCase test) {
        await SetupDatabase();
        await ModifyDatabase(test.Setups);

        var inpGameRoom = (GameRoomId)test.Inputs["game room id"]!;

        using var db = GetDbContext();

        var act = await db.GetRoomCurrentAsker(inpGameRoom);
        var exp = (RoomAsker)test.Expected["asker"]!;

        act.Should().BeEquivalentTo(exp, options =>
            options.Including(o => o.RoomId)
                .Including(o => o.UserId));
    }

    public static IEnumerable<object[]> GetRoomCurrentAskerTestsData => BundleTestCases(
        new TestCase("Found using RoomsAskers")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("asker", new RoomAsker{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
            })


        , new TestCase("Found using RoomWords")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("asker", new RoomAsker{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
            })
            .WithSetup(
                "database delete",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                    }},
                })
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                            AskedByUserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }}
                })


        , new TestCase("Found using RoomWords")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("asker", new RoomAsker{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
            })
            .WithSetup(
                "database delete",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                    }},
                })

    );
}