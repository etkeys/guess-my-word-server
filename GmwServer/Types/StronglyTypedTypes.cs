
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace GmwServer;

public record GameRoomId(Guid Value): StronglyTyped<Guid>(Value);
public class GameRoomIdValueConverter: ValueConverter<GameRoomId, Guid>
{
    public GameRoomIdValueConverter(): base(v => v.Value, v => new GameRoomId(v)){ }
}


public record RoomJoinCode(string Value): StronglyTyped<string>(Value);

public class RoomJoinCodeValueConverter: ValueConverter<RoomJoinCode, string>
{
    public RoomJoinCodeValueConverter(): base(v => v.Value, v => new RoomJoinCode(v)){ }
}

