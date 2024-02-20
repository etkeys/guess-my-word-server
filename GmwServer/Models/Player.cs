using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[PrimaryKey(nameof(RoomId), nameof(UserId))]
public class Player
{
    public GameRoomId RoomId {get; init;} = null!;

    public UserId UserId {get; init;} = null!;

    public DateTime RoomJoinTime {get; init;}

    public bool IsAsker {get; init;}

    #region Navigation properties

    [ForeignKey("RoomId")]
    public GameRoom Room {get; init;} = null!;
    [ForeignKey("UserId")]
    public User User {get; init;} = null!;

    #endregion
}