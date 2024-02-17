
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GameRoomService: IGameRoomService
{
    private IDbContextFactory<GmwServerDbContext> _dbContextFactory;
    public GameRoomService(IDbContextFactory<GmwServerDbContext> dbContextFactory){
        _dbContextFactory = dbContextFactory;
    }

    public async Task<GameRoomId?> CreateRoom(IRoomJoinCodeProvider jcProvider){
        using var db = await _dbContextFactory.CreateDbContextAsync();

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

        var room = new GameRoom{
            Id = new GameRoomId(Guid.NewGuid()),
            CreatedDate = DateTime.UtcNow,
            JoinCode = joinCode,
        };

        db.Rooms.Add(room);
        await db.SaveChangesAsync();

        return room.Id;
    }

    public async Task<GameRoom?> GetRoomStatus(GameRoomId id){
        using var db = await _dbContextFactory.CreateDbContextAsync();

        return await
            (from r in db.Rooms
            where r.Id == id
            select r)
            .FirstOrDefaultAsync();
    }

}

public interface IGameRoomService
{
    Task<GameRoomId?> CreateRoom(IRoomJoinCodeProvider jcProvider);

    Task<GameRoom?> GetRoomStatus(GameRoomId id);
}