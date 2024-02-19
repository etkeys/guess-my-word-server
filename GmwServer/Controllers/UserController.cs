using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;

namespace GmwServer.Controllers;

[ApiController]
[Route("user")]
public class UserController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    public UserController(IServiceProvider serviceProvider){
        _serviceProvider = serviceProvider;
    }

    [HttpPost("createUser", Name = "CreateUser")]
    [ProducesResponseType(typeof(UserId), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(string), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CreateUser(string? email){
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest("Email address not provided.");

        if (!MailAddress.TryCreate(email, out var realEmail))
            return BadRequest("Email address is not in the proper form.");

        realEmail = new MailAddress($"{realEmail.User}@{realEmail.Host}".ToLowerInvariant());

        var svc = _serviceProvider.GetRequiredService<IUserService>();

        var result = await svc.CreateUser(realEmail);

        if (result.Status == HttpStatusCode.Created)
            return CreatedAtAction(nameof(GetUser), new {id = ((UserId)result.GetData()!).Value}, result.GetData());

        if (result.Status == HttpStatusCode.UnprocessableEntity)
            return UnprocessableEntity(result.GetError());

        throw new Exception($"Unhandled result with status '{result.Status}'.");
    }

    [HttpGet("{id}", Name = "GetUser")]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetUser(Guid userId){
        throw new NotImplementedException();
    }
}