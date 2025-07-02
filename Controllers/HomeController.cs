using Microsoft.AspNetCore.Mvc;
using RigaMetro.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Data;


namespace RigaMetro.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly MetroDbContext _db;
    
    public HomeController(ILogger<HomeController> logger,  IConfiguration configuration, MetroDbContext db) {
        _logger = logger;
        _configuration = configuration;
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        ViewData["MapboxToken"] = _configuration["MapBox:ApiKey"];

        var lines = await _db.Lines
            .Select(l => new LineViewModel {
                LineID = l.LineID,
                Name   = l.Name,
                Color  = l.Color
            })
            .ToListAsync();

        var stations = await _db.Stations
            .Select(s => new StationViewModel {
                StationID = s.StationID,
                Name      = s.Name,
                Latitude  = s.Latitude,
                Longitude = s.Longitude
            })
            .ToListAsync();

        var model = new MapDataViewModel {
            Lines    = lines,
            Stations = stations
        };

        ViewData["MapDataJson"] = System.Text.Json.JsonSerializer.Serialize(model);
        return View(model);
    }
    
    public async Task<IActionResult> DebugData() {
        var lines = await _db.Lines
            .AsNoTracking()
            .Select(l => new LineViewModel
            {
                LineID = l.LineID,
                Name   = l.Name,
                Color  = l.Color
            })
            .ToListAsync();

        var stations = await _db.Stations
            .AsNoTracking()
            .Select(s => new StationViewModel
            {
                StationID = s.StationID,
                Name      = s.Name,
                Latitude  = s.Latitude,
                Longitude = s.Longitude
            })
            .ToListAsync();

        var model = new MapDataViewModel {
            Lines    = lines,
            Stations = stations
        };

        return View(model);
    }
    
    public IActionResult History() {
        return View();
    }
}