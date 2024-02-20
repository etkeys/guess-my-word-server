
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GameRoomService: IGameRoomService
{
    private IDbContextFactory<GmwServerDbContext> _dbContextFactory;
    public GameRoomService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
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

}

public interface IGameRoomService
{
    Task<IServiceResult> CreateRoom(UserId requestingUserId, IRoomJoinCodeProvider jcProvider);

    Task<IServiceResult> GetRoomStatus(GameRoomId id);
}