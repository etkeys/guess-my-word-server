
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;
using GmwServer;

namespace GmwServerTests;

public class BaseTests: IDisposable
{
    protected readonly Mock<IDbContextFactory<GmwServerDbContext>> _dbContextFactoryMock = new(MockBehavior.Strict);
    private DbContextOptions<GmwServerDbContext>? _defaultDbContextOptions;
    private bool _disposed;
    private SqliteConnection? _testDbConnection;

    protected DbContextOptions<GmwServerDbContext> DefaultDbContextOptions =>
        _defaultDbContextOptions ??= CreateDbContextOptions();

    protected static IEnumerable<object[]> BundleTestCases(params TestCase[] testCases) =>
        testCases.Select(tc => new object[]{tc});

    protected DbContextOptions<GmwServerDbContext> CreateDbContextOptions(bool ensureDbCreated = true) {
        _testDbConnection = new SqliteConnection("Data Source=:memory:");
        _testDbConnection.Open();


        var result = new DbContextOptionsBuilder<GmwServerDbContext>()
            .UseSqlite(_testDbConnection)
            .Options;

        if (ensureDbCreated){
            using var db = new GmwServerDbContext(result);
            db.Database.EnsureCreated();
        }

        return result;
    }

    protected async Task SetupDatabase(
        DbContextOptions<GmwServerDbContext> dbOptions,
        Dictionary<string, object?> testSetups
    ){
        _dbContextFactoryMock.Setup(e => e.CreateDbContext()).Returns(new GmwServerDbContext(dbOptions));
        _dbContextFactoryMock.Setup(e => e.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new GmwServerDbContext(dbOptions));

        if (!testSetups.TryGetValue("database", out var maybeTables)) return;

        var tables = maybeTables as Dictionary<string, object[]>;

        using var db = new GmwServerDbContext(dbOptions);

        if (tables!.TryGetValue("GeneratedRoomJoinCodes", out var genJoinCodesData))
            foreach(var code in (JoinCode[])genJoinCodesData)
                db.GeneratedRoomJoinCodes.Add(code);

        if (tables!.TryGetValue("Rooms", out var roomsData))
            foreach(var room in (GameRoom[])roomsData)
                db.Rooms.Add(room);

        await db.SaveChangesAsync();

    }

    public virtual void Dispose(){
        if (_disposed) return;

        if (_testDbConnection is not null && _testDbConnection.State == System.Data.ConnectionState.Open)
            _testDbConnection.Close();

        _testDbConnection = null;

        _disposed = true;
    }
}