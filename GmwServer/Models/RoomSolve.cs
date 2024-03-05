
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

[Index(nameof(RoomId), nameof(LiteralWord), nameof(UserId), IsUnique = true)]
public class RoomSolve
{
    [Key]
    public RoomSolveId Id {get; init;} = RoomSolveId.New();

    public GameRoomId RoomId {get; init;} = null!;

    public string LiteralWord {get; init;} = null!;

    public UserId UserId {get; init;} = null!;

    public DateTime SolvedDateTime {get; init;}


    #region Navigation properties

    [ForeignKey(nameof(RoomId))]
    public GameRoom Room {get; private set;} = null!;

    [ForeignKey(nameof(LiteralWord))]
    public Word Word {get; private set;} = null!;

    [ForeignKey(nameof(UserId))]
    public User User {get; private set;} = null!;

    #endregion

}