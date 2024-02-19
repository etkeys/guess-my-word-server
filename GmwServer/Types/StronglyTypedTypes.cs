
namespace GmwServer;

public record GameRoomId(Guid Value): StronglyTyped<Guid>(Value);

public record RoomJoinCode(string Value): StronglyTyped<string>(Value);

public record UserId(Guid Value): StronglyTyped<Guid>(Value);