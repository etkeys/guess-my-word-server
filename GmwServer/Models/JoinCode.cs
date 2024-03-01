using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmwServer;

public class JoinCode
{
    [Key]
    public RoomJoinCode Id {get; init;} = null!;

    public GameRoomId RoomId {get; init;} = null!;


    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get;} = null!;

    #endregion
}