using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OutboxTesting.MassTransit.Models;
using OutboxTesting.MassTransit.PubSub;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(IUserRepository userRepository, IBus bus) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<User>> CreateUser()
    {
        var user = await userRepository.CreateUser();
        var uri = Url.Action("Get", new {id = user.Id.EncodedValue});

        return Created(uri, user);
    }

    public class CreateMultiUserRequestBody
    {
        public int NumUsers { get; set; }
    }

    [HttpPost("multi")]
    public async Task<ActionResult> CreateMultiUser([FromBody]CreateMultiUserRequestBody body)
    {
        await bus.Publish(new GenerateMultiUser(body.NumUsers));
        return NoContent();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<User>> Get(string id)
    {
        var hashedId = new HashedId(id);
        var user = await userRepository.GetUser(hashedId.Value);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var hashedId = new HashedId(id);
        var user = await userRepository.GetUser(hashedId.Value);

        if (user is null)
        {
            return NotFound();
        }

        await userRepository.DeleteUser(hashedId.Value);

        return NoContent();
    }
}