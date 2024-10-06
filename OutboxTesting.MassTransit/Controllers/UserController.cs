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
        var uri = Url.Action("Get", new {id = user.Id});

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

    [HttpPost("{id:int}/follow/{toFollow:int}")]
    public async Task<ActionResult> FollowUser(int id, int toFollow)
    {
        var ok = await userRepository.FollowUser(id, toFollow);
        if (!ok)
        {
            return UnprocessableEntity();
        }

        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<User>> Get(int id)
    {
        var user = await userRepository.GetUser(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    [HttpGet("multi")]
    public async Task<ActionResult<PaginatedResult<User>>> GetMulti(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var users = await userRepository.GetUsers(pageNumber, pageSize);
        return Ok(users);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var ok = await userRepository.DeleteUser(id);
        if (!ok)
        {
            return UnprocessableEntity();
        }

        return NoContent();
    }
}