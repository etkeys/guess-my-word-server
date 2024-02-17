
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GmwServer.Controllers;

[ApiController]
[Route("room")]
public class GameRoomController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    public GameRoomController(IServiceProvider serviceProvider){
        _serviceProvider = serviceProvider;
    }

    [HttpPost("createRoom", Name = "CreateRoom")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<GameRoomId>> CreateRoom(){
        //TODO how to defend against spam?

        var svc = _serviceProvider.GetRequiredService<IGameRoomService>();
        var jcProvider = _serviceProvider.GetRequiredService<IRoomJoinCodeProvider>();

        var result = await svc.CreateRoom(jcProvider);

        if (result is null)
            throw new Exception("Unable to create an new room.");

        return CreatedAtAction(nameof(GetRoom), new {id = result.Value}, result);
    }

    [HttpGet("{id}", Name="GetRoom")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    // TODO how to fix the first arugment?
    public async Task<ActionResult<GameRoom>> GetRoom(Guid id){
        var svc = _serviceProvider.GetRequiredService<IGameRoomService>();

        var result = await svc.GetRoomStatus(new GameRoomId(id));

        return result is null
            ? NotFound()
            : Ok(result);

        // TODO
        /*
        How to make a better structure for 404 responses. Currently it's this:
        {
            "type": "string",
            "title": "string",
            "status": 0,
            "detail": "string",
            "instance": "string",
            "additionalProp1": "string",
            "additionalProp2": "string",
            "additionalProp3": "string"
        }
        */
    }
}