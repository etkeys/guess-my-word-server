
using GmwServer;

namespace GmwServerTests;

public partial class DbContextTests: BaseTests
{

    private GmwServerDbContext GetDbContext() => new GmwServerDbContext(DefaultDbContextOptions);

}