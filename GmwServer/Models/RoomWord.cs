
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[PrimaryKey(nameof(LiteralWord), nameof(RoomId))]
[Index(nameof(RoomId), nameof(CompletedDateTime), IsUnique = true)]
public class RoomWord
{
    public string LiteralWord {get; init;} = string.Empty;

    public GameRoomId RoomId {get; init;} = null!;

    public UserId AskedByUserId {get; init;} = null!;

    public DateTime AskedDateTime {get; init;}

    public DateTime? CompletedDateTime {get; set;}


    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get; set;} = null!;

    [ForeignKey(nameof(AskedByUserId))]
    public User AskedByUser {get; set;} = null!;

    [ForeignKey(nameof(LiteralWord))]
    public Word Word {get; set;} = null!;

    #endregion

}