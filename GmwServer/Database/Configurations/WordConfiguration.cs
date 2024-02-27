
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GmwServer;

public class WordConfigration: IEntityTypeConfiguration<Word>
{
    public void Configure(EntityTypeBuilder<Word> builder){
        builder.Property(p => p.PartOfSpeech)
            .HasConversion<string>();

        builder.HasData(
            new {LiteralWord = "steak", PartOfSpeech = PartOfSpeech.Noun},
            new {LiteralWord = "media", PartOfSpeech = PartOfSpeech.Noun},
            new {LiteralWord = "variety", PartOfSpeech = PartOfSpeech.Noun},
            new {LiteralWord = "skill", PartOfSpeech = PartOfSpeech.Noun},
            new {LiteralWord = "family", PartOfSpeech = PartOfSpeech.Noun},

            new {LiteralWord = "embrace", PartOfSpeech = PartOfSpeech.Verb},
            new {LiteralWord = "welcome", PartOfSpeech = PartOfSpeech.Verb},
            new {LiteralWord = "give", PartOfSpeech = PartOfSpeech.Verb},
            new {LiteralWord = "sell", PartOfSpeech = PartOfSpeech.Verb},
            new {LiteralWord = "point", PartOfSpeech = PartOfSpeech.Verb}
        );
    }
}