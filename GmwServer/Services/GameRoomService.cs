
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GameRoomService: IGameRoomService
{
    private IDbContextFactory<GmwServerDbContext> _dbContextFactory;
    public GameRoomService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IServiceResult> AddNewWord(GameRoomId roomId, UserId userId, string newWord){
        // TODO Controller must validation that word is not null or whitespace

        using var db = await _dbContextFactory.CreateDbContextAsync();

        if (!await IsUserRoomAsker(db, roomId, userId))
            return ServiceResults.UnprocessableEntity("User is not the current asker.");

        if (await RoomHasActiveWord(db, roomId))
            return ServiceResults.UnprocessableEntity("Room already has an active word.");

        // Word must be in the known word list
        // Word must not have been used in this room before
        var isWordUsable = await
            (from w in db.Words
            where w.LiteralWord == newWord
            select w.LiteralWord)
            .Except(
                from rw in db.RoomWords
                where
                    rw.RoomId == roomId
                    && rw.LiteralWord == newWord
                select rw.LiteralWord)
            .AnyAsync();

        if (!isWordUsable)
            return ServiceResults.UnprocessableEntity($"Provided word, '{newWord}', cannot be used.");

        // TODO Word must not be in global exclusion list (WordsGlobalForbidden)
        // TODO Word must not be in room exclusion list (WordsRoomForbidden)
        // TODO Word must not be in any player exclusion list (WordsUserForbidden)

        var newUsedWord = new RoomWord{
            LiteralWord = newWord,
            RoomId = roomId,
            AskedByUserId = userId,
            AskedDateTime = DateTime.UtcNow
        };
        db.RoomWords.Add(newUsedWord);

        // TODO must pass a collection of definitions to use as hints

        await db.SaveChangesAsync();

        return ServiceResults.Created();
    }

    public async Task<IServiceResult> CreateRoom(UserId requestingUserId, IRoomJoinCodeProvider jcProvider){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var requestingUser = await
            (from u in db.Users
            where u.Id == requestingUserId
            select u)
            .FirstOrDefaultAsync();
        if (requestingUser is null)
            return ServiceResults.Forbidden("Requesting user is not registered.");

        var joinCode = jcProvider.GetRoomJoinCode();
        var inserted = 0;
        while(inserted != 1){
            try{
                db.GeneratedRoomJoinCodes.Add(new JoinCode{Id = joinCode});
                inserted = await db.SaveChangesAsync();
            }
            catch(DbUpdateException ex){
                // if the save failed due to unique constraint violation,
                // get a new join code and try again.
                db.ChangeTracker.Entries()
                    .Where(e => e.Entity is not null)
                    .ToList()
                    .ForEach(e => e.State = EntityState.Detached);

                if (db.Database.IsSqlite()
                    && (ex.InnerException?.Message.Contains("UNIQUE constraint failed") ?? false))
                    joinCode = jcProvider.GetRoomJoinCode();
                else
                    throw;
            }
        }

        var newRoom = new GameRoom{
            Id = new GameRoomId(Guid.NewGuid()),
            CreatedDate = DateTime.UtcNow,
            JoinCode = joinCode,
            CreatedByUserId = requestingUser.Id
        };
        db.Rooms.Add(newRoom);

        var newPlayer = new Player{
            RoomId = newRoom.Id,
            UserId = requestingUser.Id,
            IsAsker = true,
            RoomJoinTime = newRoom.CreatedDate
        };
        db.Players.Add(newPlayer);

        await db.SaveChangesAsync();

        return ServiceResults.Created(newRoom.Id);
    }

    public async Task<IServiceResult> GetRoomStatus(GameRoomId id){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var result = await
            (from r in db.Rooms
            where r.Id == id
            select r)
            .FirstOrDefaultAsync();

        return result is not null
            ? ServiceResults.Ok(result)
            : ServiceResults.NotFound($"Could not find room with id '{id.Value}'.");
    }

    private Task<bool> IsUserRoomAsker(GmwServerDbContext db, GameRoomId roomId, UserId userId) =>
        (from p in db.Players
        where
            p.RoomId == roomId
            && p.UserId == userId
            && p.IsAsker
        select true)
        .AnyAsync();

    private Task<bool> RoomHasActiveWord(GmwServerDbContext db, GameRoomId roomId) =>
        (from wu in db.RoomWords
        where
            wu.RoomId == roomId
            && wu.CompletedDateTime == null
        select true)
        .AnyAsync();



}

public interface IGameRoomService
{
    Task<IServiceResult> AddNewWord(GameRoomId roomId, UserId userId, string newCurrentWord);
    Task<IServiceResult> CreateRoom(UserId requestingUserId, IRoomJoinCodeProvider jcProvider);
    Task<IServiceResult> GetRoomStatus(GameRoomId id);

}