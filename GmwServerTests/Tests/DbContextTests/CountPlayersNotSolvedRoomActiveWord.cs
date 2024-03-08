

namespace GmwServerTests;

public partial class DbContextTests
{
    [Theory, MemberData(nameof(CountPlayersNotSolvedRoomActiveWordTestsData))]
    public async Task CountPlayersNotSolvedRoomActiveWordTests(TestCase test) {
        await SetupDatabase();
        await ModifyDatabase(test.Setups);

        var inpGameRoom = (GameRoomId)test.Inputs["game room id"]!;

        using var db = GetDbContext();

        var act = await db.CountPlayersNotSolvedRoomActiveWord(inpGameRoom);
        var exp = (int)test.Expected["result"]!;

        act.Should().Be(exp);
    }

    public static IEnumerable<object[]> CountPlayersNotSolvedRoomActiveWordTestsData => BundleTestCases(
        new TestCase("All players have solved")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("result", 0)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomSolves", new []{
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                        },
                    }},
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }},
                })


        ,new TestCase("All players have solved, not in other rooms")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("result", 0)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomSolves", new []{
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                        },
                    }},
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        },
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                        },
                    }},
                })


        ,new TestCase("Not all players have solved")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("result", 1)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomSolves", new []{
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                        },
                    }},
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }},
                })


        ,new TestCase("Not all players have solved, many room words")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("result", 1)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomSolves", new []{
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                            SolvedDateTime = DateTime.UtcNow.AddMinutes(1)
                        },
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                            SolvedDateTime = DateTime.UtcNow.AddMinutes(2)
                        },
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            LiteralWord = "media",
                            UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                            SolvedDateTime = DateTime.UtcNow.AddMinutes(4)
                        },
                    }},
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddMinutes(2)
                        },
                        new RoomWord{
                            LiteralWord = "media",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow.AddMinutes(3),
                        },
                    }},
                })


        ,new TestCase("No players have solved")
            .WithInput("game room id", GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"))
            .WithExpected("result", 2)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                            AskedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }},
                })
    );
}