using System.Net.Mail;
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

    public DbSet<Definition> Definitions {get; set;}
    public DbSet<JoinCode> GeneratedRoomJoinCodes {get; set;}
    public DbSet<Player> Players {get; set;}
    public DbSet<GameRoom> Rooms {get; set;}
    public DbSet<RoomWord> RoomWords {get; set;}
    public DbSet<User> Users {get; set;}
    public DbSet<Word> Words {get; set;}


    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<GameRoomId>().HaveConversion<GameRoomIdValueConverter>();
        configurationBuilder.Properties<MailAddress>().HaveConversion<MailAddressValueConverter, MailAddressValueComparer>();
        configurationBuilder.Properties<RoomJoinCode>().HaveConversion<RoomJoinCodeValueConverter>();
        configurationBuilder.Properties<UserId>().HaveConversion<UserIdValueConverter>();
    }
}