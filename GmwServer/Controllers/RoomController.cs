
using System.Net;
using Microsoft.AspNetCore.Mvc;

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
    [ProducesResponseType(typeof(GameRoomId), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoom(){
        //TODO how to defend against spam?

        var svc = _serviceProvider.GetRequiredService<IGameRoomService>();
        var jcProvider = _serviceProvider.GetRequiredService<IRoomJoinCodeProvider>();

        var result = await svc.CreateRoom(jcProvider);

        if (result.Status == HttpStatusCode.Created)
            return CreatedAtAction(nameof(GetRoom), new {id = ((GameRoomId)result.GetData()!).Value}, result.GetData());

        throw new Exception($"Unhandled result with status '{result.Status}'.");
    }

    [HttpGet("{id}", Name="GetRoom")]
    [ProducesResponseType(typeof(GameRoom), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    // TODO how to fix the first arugment?
    public async Task<IActionResult> GetRoom(Guid id){
        var svc = _serviceProvider.GetRequiredService<IGameRoomService>();

        var result = await svc.GetRoomStatus(new GameRoomId(id));

        if (result.Status == HttpStatusCode.OK)
            return Ok(result.GetData());

        if (result.Status == HttpStatusCode.NotFound)
            return NotFound(result.GetError());

        throw new Exception($"Unhandled result with status '{result.Status}'.");

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