using Moq;
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomControllerTests: ControllerTests
{
    private readonly Mock<IGameRoomService> _gameRoomServiceMock = new(MockBehavior.Strict);

}