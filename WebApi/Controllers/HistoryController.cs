using DbThings;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class HistoryController : ControllerBase
{
    private readonly ILogger<HistoryController> _logger;
    private readonly EventsRepository _eventsRepository;

    public HistoryController(ILogger<HistoryController> logger, EventsRepository eventsRepository)
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