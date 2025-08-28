using DbThings;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ILogger<WeatherForecastController> _logger;
    private readonly EventsRepository _eventsRepository;

    public WeatherForecastController(ILogger<WeatherForecastController> logger, EventsRepository eventsRepository)
    {
        _logger = logger;
        _eventsRepository = eventsRepository;
    }

    [HttpGet]
    public async Task GetAll()
    {
        await _eventsRepository.GetAll();
    }
}