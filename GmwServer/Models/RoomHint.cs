
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[PrimaryKey(nameof(RoomId), nameof(LiteralWord), nameof(WordDefinitionId), nameof(Sequence))]
public class RoomHint
{
    public GameRoomId RoomId {get; init;} = null!;
    public string LiteralWord {get; init;} = null!;
    public int WordDefinitionId {get; init;}
    public int Sequence {get; init;}

    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get; set;} = null!;

    [ForeignKey(nameof(LiteralWord))]
    public Word Word{get; set;} = null!;


    #endregion
}