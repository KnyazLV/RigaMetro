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
    
    public async Task<IActionResult> Index() {
        var model = await CreateMapDataViewModel();
        ViewData["MapboxToken"] = _configuration["MapBox:ApiKey"];
        ViewData["MapDataJson"] = System.Text.Json.JsonSerializer.Serialize(model);
        return View(model);
    }

    public async Task<IActionResult> DebugData() {
        var model = await CreateMapDataViewModel();
        return View(model);
    }

    private async Task<MapDataViewModel> CreateMapDataViewModel() {
        
        var times = await _db.TimeBetweenStations
            .AsNoTracking()
            .Select(t => new TimeBetweenViewModel
            {
                FromStationID = t.FromStationID,
                ToStationID   = t.ToStationID,
                DistanceM     = t.DistanceM,
                TimeSeconds   = t.TimeSeconds
            })
            .ToListAsync();

        var lines = await _db.Lines
            .AsNoTracking()
            .Select(l => new LineWithStationsViewModel
            {
                LineID = l.LineID,
                Name   = l.Name,
                Color  = l.Color,
                Stations = l.LineStations
                    .OrderBy(ls => ls.StationOrder)
                    .Select(ls => new StationViewModel
                    {
                        StationID = ls.StationID,
                        Name      = ls.Station.Name,
                        Latitude  = ls.Station.Latitude,
                        Longitude = ls.Station.Longitude,
                        Order     = ls.StationOrder
                    })
                    .ToList()
            })
            .ToListAsync();

        return new MapDataViewModel {
            Lines        = lines,
            TimeBetween  = times
        };
    }
    
    public IActionResult History() {
        return View();
    }
    
}