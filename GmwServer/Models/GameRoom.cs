using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[Index(nameof(GmwServer.JoinCode))]
public class GameRoom
{
    [Key]
    public GameRoomId Id {get; init;} = new GameRoomId(Guid.NewGuid());
    public DateTime CreatedDate {get; init;}
    public RoomJoinCode? JoinCode {get; init;}
    public string? CurrentWord {get; set;}
    public UserId CreatedByUserId {get; init;} = null!;

    #region Navigation properties

    [ForeignKey("CreatedByUserId")]
    public User CreatedByUser {get; init;} = null!;

    #endregion
}