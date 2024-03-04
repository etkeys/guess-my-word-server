
namespace GmwServer;

public record DefinitionId(Guid Value): StronglyTyped<Guid>(Value)
{
    public static DefinitionId FromString(string valueAsString) =>
        new DefinitionId(Guid.Parse(valueAsString));

    public static DefinitionId New() => new DefinitionId(Guid.NewGuid());
}

public record GameRoomId(Guid Value): StronglyTyped<Guid>(Value)
{
    public static GameRoomId FromString(string valueAsString) =>
        new GameRoomId(Guid.Parse(valueAsString));

    public static GameRoomId New() => new GameRoomId(Guid.NewGuid());
}

public record RoomJoinCode(string Value): StronglyTyped<string>(Value);

public record UserId(Guid Value): StronglyTyped<Guid>(Value)
{
    public static UserId FromString(string valueAsString) =>
        new UserId(Guid.Parse(valueAsString));

    public static UserId New() => new UserId(Guid.NewGuid());
}