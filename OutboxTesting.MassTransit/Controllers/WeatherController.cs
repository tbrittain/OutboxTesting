using Microsoft.AspNetCore.Mvc;

namespace OutboxTesting.MassTransit.Controllers;

public record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int) (TemperatureC / 0.5556);
}

[ApiController]
[Route("[controller]")]
public class WeatherController : ControllerBase
{
    private readonly string[] _summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    [HttpGet("forecast")]
    public async Task<ActionResult<WeatherForecast[]>> GetForecast()
    {
        await Task.Delay(10);

        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast(
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    _summaries[Random.Shared.Next(_summaries.Length)]
                ))
            .ToArray();

        return Ok(forecast);
    }
}