using Moq;
using GmwServer;

namespace GmwServerTests;

public partial class UserControllerTests: ControllerTests
{

    private readonly Mock<IUserService> _userServiceMock = new(MockBehavior.Strict);

}