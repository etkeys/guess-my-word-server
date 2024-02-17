using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[Index(nameof(GmwServer.JoinCode))]
public class GameRoom
{
    [Key]
    public GameRoomId Id {get; init;} = new GameRoomId(Guid.NewGuid());
    public DateTime CreatedDate {get; init;}

    [Column(TypeName = "nvarchar(8)")]
    public RoomJoinCode? JoinCode {get; init;}
    public string? CurrentWord {get; set;}



}