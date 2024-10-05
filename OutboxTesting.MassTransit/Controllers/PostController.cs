using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OutboxTesting.MassTransit.Models;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController(IPostRepository postRepository, IPublishEndpoint publishEndpoint) : ControllerBase
{
    public class CreatePostRequestBody
    {
        public string UserId { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostRequestBody body)
    {
        var hashedUserId = new HashedId(body.UserId);
        var post = await postRepository.CreatePost(hashedUserId.Value);
        var uri = Url.Action("Get", new {id = post.Id.EncodedValue});

        return Created(uri, post);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> Get(string id)
    {
        var hashedId = new HashedId(id);
        var post = await postRepository.GetPost(hashedId.Value);

        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(string id)
    {
        var hashedId = new HashedId(id);
        var post = await postRepository.GetPost(hashedId.Value);

        if (post is null)
        {
            return NotFound();
        }

        await postRepository.DeletePost(hashedId.Value);

        return NoContent();
    }

    public class LikePostRequestBody
    {
        public string UserId { get; set; }
    }

    [HttpPost("{id}/like")]
    public async Task<ActionResult> Like(
        string id,
        [FromBody] LikePostRequestBody body)
    {
        var hashedId = new HashedId(id);
        var post = await postRepository.GetPost(hashedId.Value);

        if (post is null)
        {
            return NotFound();
        }

        var hashedUserId = new HashedId(body.UserId);
        var ok = await postRepository.LikePost(hashedId.Value, hashedUserId.Value);

        if (!ok)
        {
            return BadRequest();
        }

        return NoContent();
    }
}