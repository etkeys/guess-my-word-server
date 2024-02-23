
using GmwServer;

namespace GmwServerTests;

public partial class GameRoomServiceTests: BaseTests
{
    private readonly Mock<IRoomJoinCodeProvider> _roomJoinCodeProviderMock = new(MockBehavior.Strict);

}