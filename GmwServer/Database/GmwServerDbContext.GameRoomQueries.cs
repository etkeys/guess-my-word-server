
using Microsoft.EntityFrameworkCore;

namespace GmwServer;

public partial class GmwServerDbContext
{

    public async Task<int> CountPlayersNotSolvedRoomActiveWord(GameRoomId roomId) {
        var word = await GetRoomActiveWord(roomId);

        if (word is null) return 0;

        // Left join players to those that have solved the active word.
        return await
            (from p in Players
            from ra in RoomAskers
                .Where(r => p.RoomId == r.RoomId && p.UserId == r.UserId)
                .DefaultIfEmpty()
            from rs in RoomSolves
                .Where(r =>
                    p.RoomId == r.RoomId
                    && p.UserId == r.UserId
                    && r.LiteralWord == word.LiteralWord)
                .DefaultIfEmpty()
            where
                p.RoomId == roomId
                && ra == null
                && rs == null
            select p)
        .CountAsync();
    }

    public Task<RoomWord?> GetRoomActiveWord(GameRoomId roomId) =>
        (from rw in RoomWords
        where
            rw.RoomId == roomId
            && rw.CompletedDateTime == null
        select rw)
        .FirstOrDefaultAsync();

    public async Task<Player> GetRoomCurrentAsker(GameRoomId roomId) =>
        // Get the player that is in the RoomAskers tables
        await
            (from ra in RoomAskers
            join p in Players on
                new {ra.RoomId, ra.UserId} equals
                new {p.RoomId, p.UserId}
            where ra.RoomId == roomId
            select p)
            .FirstOrDefaultAsync()
        ??
        // Get the player who asked the current word
        await
            (from rw in RoomWords
            join p in Players on
                new {rw.RoomId, UserId = rw.AskedByUserId} equals 
                new {p.RoomId, p.UserId}
            where
                rw.RoomId == roomId
                && rw.CompletedDateTime == null
            select p)
            .FirstOrDefaultAsync()
        ??
        // Get the first player in the player list for this room
        await
            (from p in Players
            where p.RoomId == roomId
            orderby p.RoomJoinTime
            select p)
            .FirstAsync();

    public async Task<Player> GetRoomNextAsker(GameRoomId roomId) {
        var current = await GetRoomCurrentAsker(roomId);

        var next = await
            (from p in Players
            where
                p.RoomId == roomId
                && p.RoomJoinTime > current.RoomJoinTime
            orderby p.RoomJoinTime
            select p)
            .FirstOrDefaultAsync()
            ??
            await
            (from p in Players
            where p.RoomId == roomId
            orderby p.RoomJoinTime
            select p)
            .FirstAsync();

        return next;
    }

}