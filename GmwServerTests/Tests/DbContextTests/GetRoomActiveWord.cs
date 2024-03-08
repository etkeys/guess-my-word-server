
using GmwServer;

namespace GmwServerTests;

public partial class DbContextTests
{

    [Theory, MemberData(nameof(GetRoomActiveWordTestsData))]
    public async Task GetRoomActiveWordTests(TestCase test){
        await SetupDatabase();
        await ModifyDatabase(test.Setups);

        var inpGameRoom = (GameRoomId)test.Inputs["game room id"]!;

        using var db = GetDbContext();

        var act = await db.GetRoomActiveWord(inpGameRoom);
        var exp = (RoomWord)test.Expected["room word"]!;

        act.Should().BeEquivalentTo(exp, options =>
            options.Including(o => o.LiteralWord)
                .Including(o => o.RoomId)
                .Including(o => o.AskedByUserId)
                .Including(o => o.CompletedDateTime));
    }

    public static IEnumerable<object[]> GetRoomActiveWordTestsData => BundleTestCases(
        new TestCase("Room has active word")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("room word", new RoomWord{
                LiteralWord = "skill",
                RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                AskedDateTime = default,
            })
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }},
                })


        ,new TestCase("Many rooms have active word")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("room word", new RoomWord{
                LiteralWord = "skill",
                RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                AskedDateTime = default,
            })
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                        },
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        },
                    }},
                })


        ,new TestCase("Room has room words but none active")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("room word", null)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddMinutes(19)
                        },
                        new RoomWord{
                            LiteralWord = "media",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow.AddMinutes(20),
                            CompletedDateTime = DateTime.UtcNow.AddMinutes(30)
                        },
                    }},
                })


        ,new TestCase("Room has no room words")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("room word", null)
    );
}