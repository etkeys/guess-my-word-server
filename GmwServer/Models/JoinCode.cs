using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GmwServer;

public class JoinCode
{
    [Key]
    [Column(TypeName = "nvarchar(8)")]
    public RoomJoinCode Id {get; init;} = null!;
}