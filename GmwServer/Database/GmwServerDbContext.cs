using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public class GmwServerDbContext: DbContext
{
    public GmwServerDbContext(DbContextOptions<GmwServerDbContext> options): base(options){
#if !DEBUG
#error EnsureCreated needs to be removed and migrations need to be added
#endif
        Database.EnsureCreated();
    }

    public DbSet<JoinCode> GeneratedRoomJoinCodes {get; set;}
    public DbSet<GameRoom> Rooms {get; set;}

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<GameRoomId>().HaveConversion<GameRoomIdValueConverter>();
        configurationBuilder.Properties<RoomJoinCode>().HaveConversion<RoomJoinCodeValueConverter>();
    }
}