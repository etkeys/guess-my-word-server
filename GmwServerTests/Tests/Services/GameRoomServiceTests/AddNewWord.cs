
using System.Net;
using System.Net.Mail;
using GmwServer;
using Microsoft.EntityFrameworkCore;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{

    [Theory, MemberData(nameof(AddNewWordTestsData))]
    public async Task AddNewWordTest(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);
        await ModifyDatabase(DefaultDbContextOptions, test.Setups);

        var inpRoomId = (GameRoomId)test.Inputs["game room id"]!;
        var inpUserId = (UserId)test.Inputs["user id"]!;
        var inpNewWord = (string)test.Inputs["word"]!;

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.AddNewWord(inpRoomId, inpUserId, inpNewWord);

        var expServiceResult = (IServiceResult)test.Expected["service result"]!;
        var expRoomWordCount = (int)test.Expected["room word count"]!;
        var expActiveWord = (string)test.Expected["active word"]!;

        Assert.Equal(expServiceResult, actServiceResult, new ServiceResultEqaulityComparer());

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        var roomWords = await
            (from rw in db.RoomWords
            where rw.RoomId == inpRoomId
            select rw)
            .ToListAsync();

        Assert.Equal(expRoomWordCount, roomWords.Count());
        if (expRoomWordCount < 1) return;

        var roomActiveWord = from w in roomWords where w.CompletedDateTime == null select w;
        if (expActiveWord is null)
            Assert.Empty(roomActiveWord);
        else{
            Assert.Single(roomActiveWord);
            Assert.Equal(expActiveWord, roomActiveWord.First().LiteralWord);
            Assert.Equal(inpUserId, roomActiveWord.First().AskedByUserId);
        }
    }

    public static IEnumerable<object[]> AddNewWordTestsData => BundleTestCases(
        new TestCase("New word added")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")))
            .WithInput("word", "skill")
            .WithExpected("active word", "skill")
            .WithExpected("room word count", 1)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.Created)
                    .WithIsError(false)
                    .Create())
            .WithSetup("database", BasicTestData)


        ,new TestCase("New word added, room has no active word")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")))
            .WithInput("word", "media")
            .WithExpected("active word", "media")
            .WithExpected("room word count", 2)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.Created)
                    .WithIsError(false)
                    .Create())
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                            AskedByUserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })


        ,new TestCase("New word added, same word but different room")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bbb14f6c-53e4-4329-a1ca-8d668d7022ca")))
            .WithInput("user id", new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")))
            .WithInput("word", "skill")
            .WithExpected("active word", "skill")
            .WithExpected("room word count", 1)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.Created)
                    .WithIsError(false)
                    .Create())
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                            AskedByUserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })



        ,new TestCase("User is not player in room")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")))
            .WithInput("word", "media")
            .WithExpected("active word", null)
            .WithExpected("room word count", 0)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("User is not the current asker.")
                    .Create())
            .WithSetup("database", BasicTestData)


        ,new TestCase("User is not the current asker")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")))
            .WithInput("word", "media")
            .WithExpected("active word", null)
            .WithExpected("room word count", 0)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("User is not the current asker.")
                    .Create())
            .WithSetup("database", BasicTestData)


        ,new TestCase("Room already has an acive word")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")))
            .WithInput("word", "media")
            .WithExpected("active word", "skill")
            .WithExpected("room word count", 1)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("Room already has an active word.")
                    .Create())
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                            AskedByUserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }}
                })


        ,new TestCase("Word is not in the word list")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")))
            .WithInput("word", "foo")
            .WithExpected("active word", null)
            .WithExpected("room word count", 0)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("Provided word, 'foo', cannot be used.")
                    .Create())
            .WithSetup("database", BasicTestData)


        ,new TestCase("Word has been asked previously")
            .WithInput("game room id", new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")))
            .WithInput("user id", new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")))
            .WithInput("word", "skill")
            .WithExpected("active word", null)
            .WithExpected("room word count", 1)
            .WithExpected(
                "service result",
                new ServiceResultBuilder()
                    .WithStatus(HttpStatusCode.UnprocessableEntity)
                    .WithError("Provided word, 'skill', cannot be used.")
                    .Create())
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                            AskedByUserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })
    );
}