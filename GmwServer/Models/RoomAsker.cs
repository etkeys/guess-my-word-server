
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmwServer;

public class RoomAsker
{
    [Key]
    public GameRoomId RoomId {get; init;} = null!;

    public UserId UserId {get; init;} = null!;


    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get; private set;} = null!;

    [ForeignKey(nameof(UserId))]
    public User User {get; private set;} = null!;

    #endregion
}