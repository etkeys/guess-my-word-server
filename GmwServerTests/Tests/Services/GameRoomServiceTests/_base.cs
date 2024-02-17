
using GmwServer;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GmwServerTests;

public partial class GameRoomServiceTests: BaseTests
{
    private readonly Mock<IRoomJoinCodeProvider> _roomJoinCodeProviderMock = new(MockBehavior.Strict);

}