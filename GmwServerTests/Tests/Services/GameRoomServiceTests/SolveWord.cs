
using GmwServer;
using Microsoft.EntityFrameworkCore;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{
    [Theory, MemberData(nameof(SolveWordTestsData))]
    public async Task SolveWordTests(TestCase test) {
        await SetupDatabase();
        await ModifyDatabase(test.Setups);

        var inpGameRoom = (GameRoomId)test.Inputs["game room id"]!;
        var inpGuess = (string)test.Inputs["guess"]!;
        var inpUserId = (UserId)test.Inputs["user id"]!;

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.SolveWord(inpGameRoom, inpUserId, inpGuess);

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

        actServiceResult.Error.Should().BeNullOrEmpty();
        actServiceResult.Data.Should().NotBeNull();

        var actData = actServiceResult.Data!;

        var expIsCorrect = (bool)test.Expected["is correct"]!;
        var expSolvedDateTime = (DateTime)test.Expected.GetValueOrDefault("solved date time", default(DateTime))!;

        actData.IsGuessCorrect.Should().Be(expIsCorrect);

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var actRoomSolves = await
            (from rs in db.RoomSolves
            where
                rs.RoomId == inpGameRoom
                && rs.LiteralWord == inpGuess.ToLower()
                && rs.UserId == inpUserId
            select rs)
            .ToListAsync();

        if (!expIsCorrect){
            actRoomSolves.Should().BeEmpty();
            (await db.GetRoomActiveWord(inpGameRoom)).Should().NotBeNull();

            return;
        }

        actRoomSolves.Should().ContainSingle()
            .And.AllSatisfy(i => i.SolvedDateTime.Should().BeWithin(1.Minutes()).After(expSolvedDateTime));

        if (await db.CountPlayersNotSolvedRoomActiveWord(inpGameRoom) != 0) return;

        (await db.GetRoomActiveWord(inpGameRoom)).Should().BeNull();

        (await
            (from rw in db.RoomWords
            where
                rw.RoomId == inpGameRoom
                && rw.CompletedDateTime != null
            orderby rw.CompletedDateTime descending
            select rw.LiteralWord)
            .FirstAsync())
        .Should().Be(inpGuess.ToLower());

        var expCurrentAsker = (Player)test.Expected["new asker"]!;
        (await db.GetRoomCurrentAsker(inpGameRoom))
            .Should().BeEquivalentTo(expCurrentAsker, options =>
                options.Including(o => o.RoomId)
                    .Including(o => o.UserId));
    }

    public static IEnumerable<object[]> SolveWordTestsData => BundleTestCases(
        new TestCase("Guess is correct")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "skill")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("is correct", true)
            .WithExpected("is error", false)
            .WithExpected("new asker", new Player{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce")
            })
            .WithExpected("status", HttpStatusCode.OK)
            .WithExpected("solved date time", DateTime.UtcNow)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow
                        },
                    }}
                })


        ,new TestCase("Guess is correct, different casing")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "SkILl")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("is correct", true)
            .WithExpected("is error", false)
            .WithExpected("new asker", new Player{
                RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce")
            })
            .WithExpected("status", HttpStatusCode.OK)
            .WithExpected("solved date time", DateTime.UtcNow)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow
                        },
                    }}
                })


        ,new TestCase("Guess is incorrect")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "foo")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("is correct", false)
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.OK)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow
                        },
                    }}
                })


        ,new TestCase("Guess is incorrect, is empty string")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", string.Empty)
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("is correct", false)
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.OK)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow
                        },
                    }}
                })


        ,new TestCase("User is not a player in the room")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "skill")
            .WithInput("user id", UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"))
            .WithExpected("error", "User is not a player in the room.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.Forbidden)
            .WithSetup("database", BasicTestData)


        ,new TestCase("User is not a player in the room")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "skill")
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("error", "Player cannot solve the current word because they are the asker.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Room has no active word")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "media")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("error", "There is no active word to solve.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddMinutes(1)
                        },
                    }}
                })


        ,new TestCase("Room has no active word")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("guess", "skill")
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithExpected("error", "User has already solved the active word.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]> {
                    {"RoomSolves", new [] {
                        new RoomSolve{
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            LiteralWord = "skill",
                            UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                            SolvedDateTime = DateTime.UtcNow.AddMinutes(1)
                        }
                    }},
                    {"RoomWords", new [] {
                        new RoomWord {
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                        },
                    }},
                })

    );
}