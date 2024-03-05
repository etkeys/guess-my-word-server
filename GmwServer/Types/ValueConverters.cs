using System.Net.Mail;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GmwServer;

public class DefinitionIdValueConverter: ValueConverter<DefinitionId, Guid>
{
    public DefinitionIdValueConverter(): base(v => v.Value, v => new DefinitionId(v)){ }
}

public class GameRoomIdValueConverter: ValueConverter<GameRoomId, Guid>
{
    public GameRoomIdValueConverter(): base(v => v.Value, v => new GameRoomId(v)){ }
}

public class MailAddressValueConverter: ValueConverter<MailAddress, string>
{
    public MailAddressValueConverter(): base(v => $"{v.User}@{v.Host}", v => new MailAddress(v)) { }
}

public class MailAddressValueComparer: ValueComparer<MailAddress>
{
    // I don't think this comparer works for SQLite
    public MailAddressValueComparer():
        base(
            (m1, m2) => m1!.User.Equals(m2!.User, StringComparison.InvariantCultureIgnoreCase)
                        && m1!.Host.Equals(m2!.Host, StringComparison.InvariantCultureIgnoreCase),
            m => $"{m.User}@{m.Host}".GetHashCode(StringComparison.InvariantCultureIgnoreCase),
            m => m) { }
}

public class RoomJoinCodeValueConverter: ValueConverter<RoomJoinCode, string>
{
    public RoomJoinCodeValueConverter(): base(v => v.Value, v => new RoomJoinCode(v)){ }
}

public class RoomSolveIdValueConverter: ValueConverter<RoomSolveId, Guid>
{
    public RoomSolveIdValueConverter(): base(v => v.Value, v => new RoomSolveId(v)){ }
}

public class UserIdValueConverter: ValueConverter<UserId, Guid>
{
    public UserIdValueConverter(): base(v => v.Value, v => new UserId(v)){ }
}