
namespace GmwServerTests;

public partial class DbContextTests
{
    [Theory, MemberData(nameof(GetRoomNextAskerTestsData))]
    public async Task GetRoomNextAskerTests(TestCase test) {
        await SetupDatabase();
        await ModifyDatabase(test.Setups);

        var inpGameRoom = (GameRoomId)test.Inputs["game room id"]!;

        using var db = GetDbContext();

        var act = await db.GetRoomNextAsker(inpGameRoom);
        var exp = (Player)test.Expected["next asker"]!;

        act.Should().BeEquivalentTo(exp, options =>
            options.Including(o => o.RoomId)
                .Including(o => o.UserId));
    }

    public static IEnumerable<object[]> GetRoomNextAskerTestsData => BundleTestCases(
        // Two players, current is first
        new TestCase("Two players, current is first")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("next asker", new Player {
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
            })

        // Two players, current is last
        ,new TestCase("Two players, current is last")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("next asker", new Player{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
            })
            .WithSetup(
                "database delete",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                        },
                    }}
                })
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                        },
                    }}
                })

        // Three players, current is middle
        ,new TestCase("Three players, current is middle")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("next asker", new Player{
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
            })
            .WithSetup(
                "database delete",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            UserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                        },
                    }}
                })
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomAskers", new [] {
                        new RoomAsker{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                    }}
                })

    );
}