using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OutboxTesting.MassTransit.PubSub;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController(UserRepository userRepository, IPublishEndpoint publishEndpoint) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<Models.User>> CreateUser()
    {
        var user = await userRepository.CreateUser();
        var uri = Url.Action("Get", new {id = user.Id});

        return Created(uri, user);
    }

    public class CreateMultiUserRequestBody
    {
        public int NumUsers { get; set; }
    }

    [HttpPost("multi")]
    public async Task<ActionResult> CreateMultiUser(CreateMultiUserRequestBody body)
    {
        await publishEndpoint.Publish(new GenerateMultiUser(body.NumUsers));
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Models.User>> Get(int id)
    {
        var user = await userRepository.GetUser(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var user = await userRepository.GetUser(id);

        if (user is null)
        {
            return NotFound();
        }

        await userRepository.DeleteUser(id);

        return NoContent();
    }
}