
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomServiceTests
{

    [Theory, MemberData(nameof(GetRoomStatusTestsData))]
    public async Task GetRoomStatusTests(TestCase test){
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpRoomId = (GameRoomId)test.Inputs["game room id"]!;

        var actor = new GameRoomService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.GetRoomStatus(inpRoomId);

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

        var expRoomId = (GameRoomId)test.Expected["game room id"]!;

        actServiceResult.Error.Should().BeNullOrEmpty();
        actServiceResult.Data.Should().NotBeNull();

        // Don't need to test any further than ID because
        // If we don't get the same ID then the rest of the
        //properties are not going to matter.
        var actData = actServiceResult.Data!;
        actData.Id.Should().Be(expRoomId);

    }

    public static IEnumerable<object[]> GetRoomStatusTestsData => BundleTestCases(
        new TestCase("Found match")
            .WithInput("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("is error", false)
            .WithExpected("game room id", GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"))
            .WithExpected("status", HttpStatusCode.OK)
            .WithSetup("database", BasicTestData)


        ,new TestCase("No match")
            .WithInput("game room id", GameRoomId.FromString("5f32a934-f794-4ca0-9601-872e20d39046"))
            .WithExpected("error", "Could not find room with id '5f32a934-f794-4ca0-9601-872e20d39046'.")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithSetup("database", BasicTestData)

    );
}