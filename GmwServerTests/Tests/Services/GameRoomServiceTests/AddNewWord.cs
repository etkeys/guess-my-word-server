
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
        var inpDefinitionIds = (DefinitionId[])test.Inputs["definition ids"]!;

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.AddNewWord(inpRoomId, inpUserId, inpNewWord, inpDefinitionIds);

        var expIsError = (bool)test.Expected["is error"]!;
        var expStatus = (HttpStatusCode)test.Expected["status"]!;

        actServiceResult.IsError.Should().Be(expIsError);
        actServiceResult.Status.Should().Be(expStatus);

        using var db = new GmwServerDbContext(DefaultDbContextOptions);

        if (expIsError){
            var expError = (string)test.Expected["error"]!;

            actServiceResult.Data.Should().BeNull();
            actServiceResult.Error.Should().NotBeNullOrEmpty()
                .And.Be(expError);

            // if we got an error, then the input word should not be
            // in RoomWords
            (await (
                from rw in db.RoomWords
                where
                    rw.RoomId == inpRoomId
                    && rw.LiteralWord == inpNewWord
                    && rw.CompletedDateTime == null
                select rw
            ).AnyAsync())
            .Should().BeFalse();

            // if we got an error, then the input word should not be
            // in RoomHints
            (await (
                from rh in db.RoomHints
                join d in db.Definitions on rh.DefinitionId equals d.Id
                where
                    rh.RoomId == inpRoomId
                    && d.LiteralWord == inpNewWord
                select rh
            ).AnyAsync())
            .Should().BeFalse();

            return;
        }

        actServiceResult.Error.Should().BeNullOrEmpty();
        actServiceResult.Data.Should().NotBeNull();

        var actData = actServiceResult.Data!;

        var expAskedDateTime = (DateTime)test.Expected["asked date time"]!;
        var expHints = (DefinitionId[])test.Expected["hints"]!;
        var expRoomId = (GameRoomId)test.Expected["game room id"]!;
        var expUserId = (UserId)test.Expected["user id"]!;
        var expWord = (string)test.Expected["word"]!;

        actData.AskedByUserId.Should().Be(expUserId);
        actData.AskedDateTime.Should().BeWithin(1.Minutes()).After(expAskedDateTime);
        actData.LiteralWord.Should().Be(expWord);
        actData.RoomId.Should().Be(expRoomId);

        (await
            (from rw in db.RoomWords
            where
                rw.RoomId == expRoomId
                && rw.LiteralWord == expWord
                && rw.AskedByUserId == expUserId
                && rw.CompletedDateTime == null
            select rw)
            .CountAsync()
        ).Should().Be(1, "because our input word should be the only active word for the room");

        // We should have the expected listing of room hints
        (await
            (from rh in db.RoomHints
            where rh.RoomId == expRoomId
            orderby rh.Sequence
            select rh.DefinitionId)
            .ToListAsync()
        ).Should().Equal(expHints, "because we should have stored the provided hints in the correct order.");

    }

    public static IEnumerable<object[]> AddNewWordTestsData => BundleTestCases(
        new TestCase("New word added")
            .WithInput(
                "definition ids",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "skill")
            .WithExpected("asked date time", DateTime.UtcNow)
            .WithExpected("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected(
                "hints",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("word", "skill")
            .WithSetup("database", BasicTestData)


        ,new TestCase("New word added, room has no active word")
            .WithInput(
                "definition ids",
                new []{"2dc96c83-3329-48fe-9677-e9eb9f443b38"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "media")
            .WithExpected("asked date time", DateTime.UtcNow)
            .WithExpected("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected(
                "hints",
                new []{"2dc96c83-3329-48fe-9677-e9eb9f443b38"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("word", "media")
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })


        ,new TestCase("New word added, same word but different room")
            .WithInput(
                "definition ids",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"))
            .WithInput("user id", UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"))
            .WithInput("word", "skill")
            .WithExpected("asked date time", DateTime.UtcNow)
            .WithExpected("game room id", GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"))
            .WithExpected("is error", false)
            .WithExpected(
                "hints",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("user id", UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"))
            .WithExpected("word", "skill")
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })


        ,new TestCase("User is not player in room")
            .WithInput(
                "definition ids",
                new []{"2dc96c83-3329-48fe-9677-e9eb9f443b38"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"))
            .WithInput("word", "media")
            .WithExpected("error", "User is not the current asker.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)


        ,new TestCase("User is not the current asker")
            .WithInput(
                "definition ids",
                new []{"2dc96c83-3329-48fe-9677-e9eb9f443b38"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"))
            .WithInput("word", "media")
            .WithExpected("error", "User is not the current asker.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Room already has an acive word")
            .WithInput(
                "definition ids",
                new []{"2dc96c83-3329-48fe-9677-e9eb9f443b38"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "media")
            .WithExpected("error", "Room already has an active word.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                        }
                    }}
                })


        ,new TestCase("Word is not in the word list")
            .WithInput(
                "definition ids",
                new []{"52c4358c-ee2f-48c7-a5ff-d86a55a09ad0"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "foo")
            .WithExpected("error", "Provided word, 'foo', cannot be used.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)


        ,new TestCase("Word has been asked previously")
            .WithInput(
                "definition ids",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "skill")
            .WithExpected("error", "Provided word, 'skill', cannot be used.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)
            .WithSetup(
                "database add",
                new Dictionary<string, object[]>{
                    {"RoomWords", new []{
                        new RoomWord{
                            LiteralWord = "skill",
                            RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                            AskedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                            AskedDateTime = DateTime.UtcNow,
                            CompletedDateTime = DateTime.UtcNow.AddSeconds(1)
                        }
                    }}
                })


        ,new TestCase("New word added, definition ids contains duplicates")
            .WithInput(
                "definition ids",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb", "5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "skill")
            .WithExpected("asked date time", DateTime.UtcNow)
            .WithExpected("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected(
                "hints",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.Created)
            .WithExpected("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithExpected("word", "skill")
            .WithSetup("database", BasicTestData)


        ,new TestCase("Defintion ids contain invalid id")
            .WithInput(
                "definition ids",
                new []{"c4b28ddc-d912-4bc2-8765-5c3c1e27a6bb","11a98d1d-f12b-4a18-ad93-4c7257ff784d","ecdff69a-780b-40cc-9219-d82b8c6568f5", "5e911a3e-a1a5-4ff7-9030-42ec60421a6b"}
                    .Select(i => DefinitionId.FromString(i))
                    .ToArray())
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "skill")
            .WithExpected("error", "Received invalid definitions for word 'skill'.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)


        ,new TestCase("No definition ids provided")
            .WithInput("definition ids", new DefinitionId[] {})
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithInput("user id", UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"))
            .WithInput("word", "skill")
            .WithExpected("error", "No definitions to use as hints provided.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.UnprocessableEntity)
            .WithSetup("database", BasicTestData)

    );
}