using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[EntityTypeConfiguration(typeof(WordConfigration))]
public class Word
{
    [Key]
    public string LiteralWord {get; init;} = null!;

    public PartOfSpeech PartOfSpeech {get; init;}
}