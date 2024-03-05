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
                    Id = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                    CreatedByUserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                    CreatedDate = DateTime.UtcNow,
                },
                new GameRoom{
                    Id = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                    CreatedByUserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                    CreatedDate = DateTime.UtcNow,
                },
            }},
            {"JoinCodes", new [] {
                new JoinCode {
                    Id = new RoomJoinCode("aaaabbEb"),
                    RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                },
                new JoinCode {
                    Id = new RoomJoinCode("aaaabbNa"),
                    RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                }
            }},
            {"Players", new [] {
                new Player{
                    RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                    UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                    RoomJoinTime = DateTime.UtcNow,
                    IsAsker = true
                },
                new Player{
                    RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                    UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                    RoomJoinTime = DateTime.UtcNow.AddSeconds(30),
                },
                new Player{
                    RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                    UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                    RoomJoinTime = DateTime.UtcNow,
                    IsAsker = true
                }
            }},
            {"Users", new []{
                new User{
                    Id = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                    Email = new MailAddress("Alice@example.com"),
                    DisplayName = string.Empty,
                },
                new User{
                    Id = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                    Email = new MailAddress("Bob@example.com"),
                    DisplayName = string.Empty,
                },
                new User{
                    Id = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
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

    protected async Task<Dictionary<string, int>> GetDatabaseTableCounts(
        DbContextOptions<GmwServerDbContext> dbOptions
    ){
        using var db = new GmwServerDbContext(dbOptions);
        var result = new Dictionary<string, int>{
            {nameof(db.Definitions), await db.Definitions.CountAsync()},
            {nameof(db.JoinCodes), await db.JoinCodes.CountAsync()},
            {nameof(db.Players), await db.Players.CountAsync()},
            {nameof(db.Rooms), await db.Rooms.CountAsync()},
            {nameof(db.RoomHints), await db.RoomHints.CountAsync()},
            {nameof(db.RoomWords), await db.RoomWords.CountAsync()},
            {nameof(db.Users), await db.Users.CountAsync()},
            {nameof(db.Words), await db.Words.CountAsync()},
        };

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

    protected Task ModifyDatabase(IDictionary<string, object?> testSetups) =>
        ModifyDatabase(DefaultDbContextOptions, testSetups);

    private void ModifyDatabaseAdd(
        GmwServerDbContext db,
        IDictionary<string, object[]> tables
    ){
        if (tables.TryGetValue("RoomWords", out var roomWordsData))
            foreach(var room in (RoomWord[])roomWordsData)
                db.RoomWords.Add(room);
    }

    protected Task SetupDatabase(
        DbContextOptions<GmwServerDbContext> dbOptions,
        IDictionary<string, object?> testSetups
    ){
        if (!testSetups.TryGetValue("database", out var maybeTables)){
            SetupDbContextFactoryMock(dbOptions);
            return Task.Run(() => {});
        }

        var tables = maybeTables as Dictionary<string, object[]>;

        return SetupDatabaseImpl(dbOptions, tables!);
    }

    protected Task SetupDatabase() =>
        SetupDatabaseImpl(DefaultDbContextOptions, BasicTestData);

    private async Task SetupDatabaseImpl(
        DbContextOptions<GmwServerDbContext> dbOptions,
        IDictionary<string, object[]> tables
    ){
        SetupDbContextFactoryMock(dbOptions);

        using var db = new GmwServerDbContext(dbOptions);

        if (tables!.TryGetValue("JoinCodes", out var joinCodesData))
            foreach(var code in (JoinCode[])joinCodesData)
                db.JoinCodes.Add(code);

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

    private void SetupDbContextFactoryMock(DbContextOptions<GmwServerDbContext> dbOptions){
        _dbContextFactoryMock.Setup(e => e.CreateDbContext()).Returns(new GmwServerDbContext(dbOptions));
        _dbContextFactoryMock.Setup(e => e.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new GmwServerDbContext(dbOptions));
    }

    public virtual void Dispose(){
        if (_disposed) return;

        if (_testDbConnection is not null && _testDbConnection.State == System.Data.ConnectionState.Open)
            _testDbConnection.Close();

        _testDbConnection = null;

        _disposed = true;
    }
}