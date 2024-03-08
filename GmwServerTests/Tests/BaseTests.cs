using System.Net.Mail;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using GmwServer;

namespace GmwServerTests;

public class BaseTests: IDisposable
{
    protected readonly Mock<IDbContextFactory<GmwServerDbContext>> _dbContextFactoryMock = new(MockBehavior.Strict);
    private DbContextOptions<GmwServerDbContext>? _defaultDbContextOptions;
    protected bool Disposed {get; private set;}
    private SqliteConnection? _testDbConnection;

    protected static IDictionary<string, object[]> BasicTestData =>
        new Dictionary<string, object[]>{
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
                new GameRoom{
                    Id = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    CreatedByUserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
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
                },
                new JoinCode {
                    Id = new RoomJoinCode("bbbbaaaa"),
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                },
            }},
            {"Players", new [] {
                new Player{
                    RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                    UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                    RoomJoinTime = DateTime.UtcNow,
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
                },
                new Player{
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    UserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                    RoomJoinTime = DateTime.UtcNow,
                },
                new Player{
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                    RoomJoinTime = DateTime.UtcNow.AddTicks(1),
                },
                new Player{
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    UserId = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                    RoomJoinTime = DateTime.UtcNow.AddTicks(2),
                },
            }},
            {"RoomAskers", new []{
                new RoomAsker{
                    RoomId = GameRoomId.FromString("bc428470-1c15-4822-880b-f90965036ae2"),
                    UserId = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                },
                new RoomAsker{
                    RoomId = GameRoomId.FromString("bbb14f6c-53e4-4329-a1ca-8d668d7022ca"),
                    UserId = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                },
                new RoomAsker{
                    RoomId = GameRoomId.FromString("a2b2c272-cc61-4445-a4d1-c810d7550f25"),
                    UserId = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                },
            }},
            {"Users", new []{
                new User{
                    Id = UserId.FromString("771dd88e-bcd4-42d2-ade6-0804926628f0"),
                    Email = new MailAddress("Alice@example.com"),
                    DisplayName = "Alice",
                },
                new User{
                    Id = UserId.FromString("1fce0ea5-5736-454d-a3b3-30ca9b163bce"),
                    Email = new MailAddress("Bob@example.com"),
                    DisplayName = "Bob",
                },
                new User{
                    Id = UserId.FromString("785d1043-c84f-4cb4-800b-16e7770d482c"),
                    Email = new MailAddress("Claire@example.com"),
                    DisplayName = "Claire",
                },
                new User{
                    Id = UserId.FromString("ed33e038-2935-471b-b602-7a6b140ba0a4"),
                    Email = new MailAddress("Dan@example.com"),
                    DisplayName = "Dan",
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
            {nameof(db.RoomAskers), await db.RoomAskers.CountAsync()},
            {nameof(db.RoomHints), await db.RoomHints.CountAsync()},
            {nameof(db.RoomSolves), await db.RoomSolves.CountAsync()},
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

        if (testSetups.TryGetValue("database delete", out var deleteData))
            ModifyDatabaseDelete(db, (IDictionary<string, object[]>)deleteData!);

        await db.SaveChangesAsync();

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
        if (tables.TryGetValue("RoomAskers", out var roomAskersData))
            db.RoomAskers.AddRange((RoomAsker[])roomAskersData);

        if (tables.TryGetValue("RoomSolves", out var roomSolvesData))
            db.RoomSolves.AddRange((RoomSolve[])roomSolvesData);

        if (tables.TryGetValue("RoomWords", out var roomWordsData))
            foreach(var room in (RoomWord[])roomWordsData)
                db.RoomWords.Add(room);
    }

    private void ModifyDatabaseDelete(
        GmwServerDbContext db,
        IDictionary<string, object[]> tables
    ){
        if (tables.TryGetValue("Players", out var playerData))
            db.Players.RemoveRange((Player[])playerData);

        if (tables.TryGetValue("RoomAskers", out var roomAskerData))
            db.RoomAskers.RemoveRange((RoomAsker[])roomAskerData);
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

        if (tables.TryGetValue("RoomAskers", out var roomAskerData))
            db.RoomAskers.AddRange((RoomAsker[])roomAskerData);

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
        if (Disposed) return;

        if (_testDbConnection is not null && _testDbConnection.State == System.Data.ConnectionState.Open)
            _testDbConnection.Close();

        _testDbConnection = null;

        Disposed = true;
    }
}