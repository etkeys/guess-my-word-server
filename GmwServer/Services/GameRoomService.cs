
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GameRoomService: IGameRoomService
{
    private IDbContextFactory<GmwServerDbContext> _dbContextFactory;
    public GameRoomService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
    }

    public async Task<IServiceResult<RoomWord>> AddNewWord(
        GameRoomId roomId,
        UserId userId,
        string newWord,
        IEnumerable<DefinitionId> definitions){
        // TODO Controller must validation that word is not null or whitespace

        using var db = await _dbContextFactory.CreateDbContextAsync();

        if (!await IsUserRoomAsker(db, roomId, userId))
            return ServiceResults.UnprocessableEntity<RoomWord>("User is not the current asker.");

        if (await RoomHasActiveWord(db, roomId))
            return ServiceResults.UnprocessableEntity<RoomWord>("Room already has an active word.");

        if (!definitions.Any())
            return ServiceResults.UnprocessableEntity<RoomWord>("No definitions to use as hints provided.");

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
            return ServiceResults.UnprocessableEntity<RoomWord>($"Provided word, '{newWord}', cannot be used.");

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

        var badDefinitionIds = await Task.Run(() =>
            (from id in definitions
            select id)
            .Except(
                from d in db.Definitions
                where d.LiteralWord == newWord
                select d.Id
            )
            .ToList());

        if (badDefinitionIds.Any())
            return ServiceResults.UnprocessableEntity<RoomWord>($"Received invalid definitions for word '{newWord}'.");

        var hints = definitions.Distinct().Select((d, index) => new RoomHint{
            RoomId = roomId,
            Sequence = index,
            DefinitionId = d
        });

        db.RoomHints.AddRange(hints);

        await db.SaveChangesAsync();

        return ServiceResults.Created(newUsedWord);
    }

    public async Task<IServiceResult<GameRoomId>> CreateRoom(UserId requestingUserId, IRoomJoinCodeProvider jcProvider){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var requestingUser = await
            (from u in db.Users
            where u.Id == requestingUserId
            select u)
            .FirstOrDefaultAsync();
        if (requestingUser is null)
            return ServiceResults.Forbidden<GameRoomId>("Requesting user is not registered.");


        var newRoom = new GameRoom{
            CreatedDate = DateTime.UtcNow,
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

        var joinCode = jcProvider.GetRoomJoinCode();
        var inserted = 0;
        while(inserted != 1){
            try{
                db.JoinCodes.Add(new JoinCode{
                    Id = joinCode,
                    RoomId = newRoom.Id
                });
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

        return ServiceResults.Created(newRoom.Id);
    }

    public async Task<IServiceResult<GameRoom>> GetRoomStatus(GameRoomId id){
        // TODO User making the request must be in the room
        using var db = await _dbContextFactory.CreateDbContextAsync();

        var result = await
            (from r in db.Rooms
            where r.Id == id
            select r)
            .FirstOrDefaultAsync();

        return result is not null
            ? ServiceResults.Ok(result)
            : ServiceResults.NotFound<GameRoom>($"Could not find room with id '{id.Value}'.");
    }

    public async Task<IServiceResult<GameRoomId>> JoinRoom(
        UserId userId,
        RoomJoinCode joinCode,
        IRoomJoinCodeProvider jcProvider
    ){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        joinCode = jcProvider.NormalizeJoinCode(joinCode);

        var roomId = await
            (from r in db.JoinCodes
            where r.Id == joinCode
            select r.RoomId)
            .FirstOrDefaultAsync();

        if (roomId is null)
            return ServiceResults.NotFound<GameRoomId>("Could not find room using given join code.");

        var isUserAlreadyPlayer = await
            (from p in db.Players
            where
                p.RoomId == roomId
                && p.UserId == userId
            select true)
            .AnyAsync();
        if (isUserAlreadyPlayer)
            return ServiceResults.Created(roomId);

        var newPlayer = new Player{
            RoomId = roomId,
            UserId = userId,
            RoomJoinTime = DateTime.UtcNow
        };

        db.Players.Add(newPlayer);
        await db.SaveChangesAsync();

        return ServiceResults.Created(roomId);
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
    Task<IServiceResult<RoomWord>> AddNewWord(
        GameRoomId roomId,
        UserId userId,
        string newCurrentWord,
        IEnumerable<DefinitionId> definitions);
    Task<IServiceResult<GameRoomId>> CreateRoom(UserId requestingUserId, IRoomJoinCodeProvider jcProvider);
    Task<IServiceResult<GameRoom>> GetRoomStatus(GameRoomId id);
    Task<IServiceResult<GameRoomId>> JoinRoom(UserId userId, RoomJoinCode joinCode, IRoomJoinCodeProvider jcProvider);

}