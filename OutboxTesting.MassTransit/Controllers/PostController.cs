using MassTransit;
using Microsoft.AspNetCore.Mvc;
using OutboxTesting.MassTransit.Services;

namespace OutboxTesting.MassTransit.Controllers;

[ApiController]
[Route("[controller]")]
public class PostController(IPostRepository postRepository, IPublishEndpoint publishEndpoint) : ControllerBase
{
    public class CreatePostRequestBody
    {
        public int UserId { get; set; }
    }

    [HttpPost]
    public async Task<ActionResult> CreatePost([FromBody] CreatePostRequestBody body)
    {
        var post = await postRepository.CreatePost(body.UserId);
        var uri = Url.Action("Get", new {id = post.Id});

        return Created(uri, post);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult> Get(int id)
    {
        var post = await postRepository.GetPost(id);
        if (post is null)
        {
            return NotFound();
        }

        return Ok(post);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var ok = await postRepository.DeletePost(id);
        if (!ok)
        {
            return UnprocessableEntity(); 
        }

        return NoContent();
    }

    public class LikePostRequestBody
    {
        public int UserId { get; set; }
    }

    [HttpPost("{id:int}/like")]
    public async Task<ActionResult> Like(
        int id,
        [FromBody] LikePostRequestBody body)
    {
        var ok = await postRepository.LikePost(id, body.UserId);
        if (!ok)
        {
            return UnprocessableEntity();
        }

        return NoContent();
    }
}