
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[EntityTypeConfiguration(typeof(DefinitionConfiguration))]
[Index(nameof(LiteralWord), nameof(WordDefinitionId), IsUnique = true)]
public class Definition
{
    [Key]
    public DefinitionId Id {get; init;} = DefinitionId.New();

    public string LiteralWord {get; init;} = null!;

    public int WordDefinitionId {get; init;}

    public string DefinitionText {get; init;} = null!;


    #region Navigation properties

    [ForeignKey(nameof(LiteralWord))]
    public Word Word {get; init;} = null!;

    #endregion
}
