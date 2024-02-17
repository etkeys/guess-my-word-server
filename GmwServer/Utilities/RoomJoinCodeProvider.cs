
namespace GmwServer;

public interface IRoomJoinCodeProvider
{
    RoomJoinCode GetRoomJoinCode();
}

public class RoomJoinCodeProvider: IRoomJoinCodeProvider
{
    private static readonly Random _rng = new ();
    private readonly static char[] _roomCodeDigits =
        ['a', 'b', 'c', 'd', 'E', 'f', 'g', 'h', 'i', 'j', 'k', 'L', 'm', 'N',
        'o', 'p', 'Q', 'R', 's', 't', 'u', 'V', 'w', 'x', 'y', 'z', '0','1','2'
        ,'3','4','5','6','7','8','9'];
    private static readonly object _syncRoot = new();

    public RoomJoinCode GetRoomJoinCode(){
        lock(_syncRoot){
            var resultParts = Enumerable.Range(0, 8)
                .Select(i => _roomCodeDigits[_rng.Next(36)]);

            return new RoomJoinCode(string.Join("", resultParts));
        }
    }
}