using System.Net.Mail;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using GmwServer;

namespace GmwServerTests;

public class BaseTests: IDisposable
{
    protected readonly Mock<IDbContextFactory<GmwServerDbContext>> _dbContextFactoryMock = new(MockBehavior.Strict);
    private DbContextOptions<GmwServerDbContext>? _defaultDbContextOptions;
    private bool _disposed;
    private SqliteConnection? _testDbConnection;

    protected static IDictionary<string, object[]> BasicTestData =>
        new Dictionary<string, object[]>{
            {"GeneratedRoomJoinCodes", new[]{
                new JoinCode{Id = new RoomJoinCode("aaaabbbb")},
                new JoinCode{Id = new RoomJoinCode("aaaabbba")},
            }},
            {"Rooms", new [] {
                new GameRoom{
                    Id = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                    CreatedByUserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                    CreatedDate = DateTime.UtcNow,
                    JoinCode = new RoomJoinCode("aaaabbbb"),
                },
                new GameRoom{
                    Id = new GameRoomId(Guid.Parse("bbb14f6c-53e4-4329-a1ca-8d668d7022ca")),
                    CreatedByUserId = new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")),
                    CreatedDate = DateTime.UtcNow,
                    JoinCode = new RoomJoinCode("aaaabbba"),
                },
            }},
            {"Players", new [] {
                new Player{
                    RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                    UserId = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                    RoomJoinTime = DateTime.UtcNow,
                    IsAsker = true
                },
                new Player{
                    RoomId = new GameRoomId(Guid.Parse("bc428470-1c15-4822-880b-f90965036ae2")),
                    UserId = new UserId(Guid.Parse("1fce0ea5-5736-454d-a3b3-30ca9b163bce")),
                    RoomJoinTime = DateTime.UtcNow.AddMinutes(1),
                },
                new Player{
                    RoomId = new GameRoomId(Guid.Parse("bbb14f6c-53e4-4329-a1ca-8d668d7022ca")),
                    UserId = new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")),
                    RoomJoinTime = DateTime.UtcNow,
                    IsAsker = true
                }
            }},
            {"Users", new []{
                new User{
                    Id = new UserId(Guid.Parse("771dd88e-bcd4-42d2-ade6-0804926628f0")),
                    Email = new MailAddress("Alice@example.com"),
                    DisplayName = string.Empty,
                },
                new User{
                    Id = new UserId(Guid.Parse("1fce0ea5-5736-454d-a3b3-30ca9b163bce")),
                    Email = new MailAddress("Bob@example.com"),
                    DisplayName = string.Empty,
                },
                new User{
                    Id = new UserId(Guid.Parse("785d1043-c84f-4cb4-800b-16e7770d482c")),
                    Email = new MailAddress("Claire@example.com"),
                    DisplayName = string.Empty,
                },
            }}
        };


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

    protected async Task ModifyDatabase(
        DbContextOptions<GmwServerDbContext> dbOptions,
        IDictionary<string, object?> testSetups
    ){
        using var db = new GmwServerDbContext(dbOptions);

        if (testSetups.TryGetValue("database add", out var addData))
            ModifyDatabaseAdd(db, (IDictionary<string, object[]>)addData!);

        await db.SaveChangesAsync();
    }

    private void ModifyDatabaseAdd(
        GmwServerDbContext db,
        IDictionary<string, object[]> tables
    ){
        if (tables.TryGetValue("RoomWords", out var roomWordsData))
            foreach(var room in (RoomWord[])roomWordsData)
                db.RoomWords.Add(room);
    }

    protected async Task SetupDatabase(
        DbContextOptions<GmwServerDbContext> dbOptions,
        IDictionary<string, object?> testSetups
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

        if (tables!.TryGetValue("Players", out var playerData))
            foreach(var player in (Player[])playerData)
                db.Players.Add(player);

        if (tables!.TryGetValue("Rooms", out var roomsData))
            foreach(var room in (GameRoom[])roomsData)
                db.Rooms.Add(room);

        if (tables!.TryGetValue("Users", out var usersData))
            foreach(var user in (User[])usersData)
                db.Users.Add(user);

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