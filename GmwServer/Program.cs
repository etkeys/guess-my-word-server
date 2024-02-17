using GmwServer;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.Converters.Add(
            new StronglyTypedJsonConverterFactory()
        );
    });

#if !DEBUG
#error Remove SQLite connection
#endif
var dbConnection = new SqliteConnection("Data Source=:memory:");
dbConnection.Open();
builder.Services.AddDbContextFactory<GmwServerDbContext>(opts =>
    opts.UseSqlite(dbConnection));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IRoomJoinCodeProvider, RoomJoinCodeProvider>();
builder.Services.AddTransient<IGameRoomService, GameRoomService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
