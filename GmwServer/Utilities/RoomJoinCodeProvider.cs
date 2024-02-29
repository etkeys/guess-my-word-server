
namespace GmwServer;

public class RoomJoinCodeProvider: IRoomJoinCodeProvider
{
    private const int JoinCodeLength = 8;
    private static readonly Random _rng = new ();
    private static readonly Dictionary<string, char> _roomCodeDigits = new(StringComparer.InvariantCultureIgnoreCase){
        {"a", 'a'},
        {"b", 'b'},
        {"c", 'c'},
        {"d", 'd'},
        {"e", 'E'},
        {"f", 'f'},
        {"g", 'g'},
        {"h", 'h'},
        {"i", 'i'},
        {"j", 'J'},
        {"k", 'k'},
        {"l", 'L'},
        {"m", 'm'},
        {"n", 'N'},
        {"o", 'o'},
        {"p", 'p'},
        {"q", 'Q'},
        {"r", 'R'},
        {"s", 's'},
        {"t", 't'},
        {"u", 'u'},
        {"v", 'V'},
        {"w", 'w'},
        {"x", 'x'},
        {"y", 'y'},
        {"z", 'z'},
        {"0", '0'},
        {"1", '1'},
        {"2", '2'},
        {"3", '3'},
        {"4", '4'},
        {"5", '5'},
        {"6", '6'},
        {"7", '7'},
        {"8", '8'},
        {"9", '9'},
    };
    private static readonly object _syncRoot = new();

    public RoomJoinCode GetRoomJoinCode(){
        var possibleDigits = _roomCodeDigits.Values.ToArray();
        lock(_syncRoot){
            var resultParts = Enumerable.Range(0, JoinCodeLength)
                .Select(i => possibleDigits[_rng.Next(possibleDigits.Length)]);

            return new RoomJoinCode(string.Join("", resultParts));
        }
    }

    public RoomJoinCode NormalizeJoinCode(RoomJoinCode input){
        var chars = input.Value.Select(i =>
            _roomCodeDigits.TryGetValue(i.ToString(), out var @char)
                ? @char
                : '.'
            )
            .ToArray();

        return new RoomJoinCode(new string(chars));
    }
}

public interface IRoomJoinCodeProvider
{
    RoomJoinCode GetRoomJoinCode();
    RoomJoinCode NormalizeJoinCode(RoomJoinCode input);
}