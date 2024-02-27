
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class WordService
{
    private IDbContextFactory<GmwServerDbContext> _dbContextFactory;
    public WordService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IServiceResult<WordWithDefinitionsVm>> GetWord(string word) {
        // TODO Controller needs to validate input
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var result = await
            (from w in db.Words
            where w.LiteralWord == word
            select new WordWithDefinitionsVm{
                Word = w.LiteralWord,
                PartOfSpeech = w.PartOfSpeech,
                Definitions = w.Definitions.Select(d => new DefinitionVm{
                        WordDefinitionId = d.WordDefinitionId,
                        DefintionText = d.DefinitionText,
                    })
            })
            .FirstOrDefaultAsync();

        if (result is null)
            return ServiceResults.NotFound<WordWithDefinitionsVm>($"Could not find word '{word}'.");

        return ServiceResults.Ok(result);
    }
}