
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[EntityTypeConfiguration(typeof(DefinitionConfiguration))]
[PrimaryKey(nameof(LiteralWord), nameof(WordDefinitionId))]
public class Definition
{
    public string LiteralWord {get; init;} = null!;

    public int WordDefinitionId {get; init;}

    public string DefinitionText {get; init;} = null!;


    #region Navigation properties

    [ForeignKey(nameof(LiteralWord))]
    public Word Word {get; init;} = null!;

    #endregion
}
