
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[PrimaryKey(nameof(RoomId), nameof(Sequence), nameof(DefinitionId))]
public class RoomHint
{
    public GameRoomId RoomId {get; init;} = null!;
    public int Sequence {get; init;}
    public DefinitionId DefinitionId {get; init;} = null!;

    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get; set;} = null!;

    [ForeignKey(nameof(DefinitionId))]
    public Definition Definition {get; set;} = null!;

    #endregion
}