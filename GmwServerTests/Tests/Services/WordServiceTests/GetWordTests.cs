
using GmwServer;

namespace GmwServerTests;

public partial class WordServiceTests
{
    [Theory, MemberData(nameof(GetWordTestsData))]
    public async Task GetWordTests(TestCase test) {
        await SetupDatabase(DefaultDbContextOptions, test.Setups);

        var inpWord = (string)test.Inputs["word"]!;

        var actor = new WordService(_dbContextFactoryMock.Object);
        var actServiceResult = await actor.GetWord(inpWord);

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

        var expWord = (string)test.Expected["word"]!;
        var expPartOfSpeech = (PartOfSpeech)test.Expected["part of speech"]!;
        var expDefinitionIds = (int[])test.Expected["definition ids"]!;

        var actWordVm = actServiceResult.Data!;

        actWordVm.Word.Should().Be(expWord);
        actWordVm.PartOfSpeech.Should().Be(expPartOfSpeech);
        actWordVm.Definitions.Should().HaveCount(expDefinitionIds.Length);
        actWordVm.Definitions.Select(d => d.WordDefinitionId).Should().BeEquivalentTo(expDefinitionIds);
        actWordVm.Definitions.Select(d => d.DefintionText).Should().AllSatisfy(t => {t.Should().NotBeNullOrWhiteSpace();});
    }

    public static IEnumerable<object[]> GetWordTestsData => BundleTestCases(
        new TestCase("Word exists with one definition")
            .WithInput("word", "media")
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.OK)
            .WithExpected("word", "media")
            .WithExpected("part of speech", PartOfSpeech.Noun)
            .WithExpected("definition ids", new [] {1})


        ,new TestCase("Word exists with many definitions")
            .WithInput("word", "sell")
            .WithExpected("is error", false)
            .WithExpected("status", HttpStatusCode.OK)
            .WithExpected("word", "sell")
            .WithExpected("part of speech", PartOfSpeech.Verb)
            .WithExpected("definition ids", new [] {1, 2, 3})


        ,new TestCase("Word is not in database")
            .WithInput("word", "foo")
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithExpected("error", "Could not find word 'foo'.")


        ,new TestCase("Word is empty")
            .WithInput("word", string.Empty)
            .WithExpected("is error", true)
            .WithExpected("status", HttpStatusCode.NotFound)
            .WithExpected("error", "Could not find word ''.")
    );
}