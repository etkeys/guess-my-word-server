using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GameRoom
{
    [Key]
    public GameRoomId Id {get; init;} = new GameRoomId(Guid.NewGuid());
    public DateTime CreatedDate {get; init;}
    public UserId CreatedByUserId {get; init;} = null!;

    #region Navigation properties

    [ForeignKey("CreatedByUserId")]
    public User CreatedByUser {get; init;} = null!;

    public JoinCode? JoinCode {get; private set;}

    #endregion
}