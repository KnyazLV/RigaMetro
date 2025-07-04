using Microsoft.AspNetCore.Mvc;
using RigaMetro.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Data;
using RigaMetro.Services;


namespace RigaMetro.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IScheduleService _scheduleService;
    private readonly MetroDbContext _db;
    
    public HomeController(ILogger<HomeController> logger,  IConfiguration configuration, MetroDbContext db,
                          IScheduleService scheduleService ) {
        _logger = logger;
        _configuration = configuration;
        _db = db;
        _scheduleService = scheduleService;
    }

    #region Actions
    
    public async Task<IActionResult> Index() {
        
        const string trainId = "TR001";
        var workDate = DateTime.Today;
        
        await _scheduleService.GenerateDailyScheduleAsync(trainId, workDate);
        
        var model = await CreateMapDataViewModel();
        ViewData["MapboxToken"] = _configuration["MapBox:ApiKey"];
        return View(model);
    }

    public async Task<IActionResult> DebugData() {
        var model = await CreateMapDataViewModel();
        return View(model);
    }
    
    #endregion

    #region ViewModelCreation
    
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

        var trains = await _db.Trains
            .AsNoTracking()
            .Select(t => new TrainViewModel
            {
                TrainID       = t.TrainID,
                LineID        = t.LineID,
                TrainName     = t.TrainName,
                IsActive      = t.IsActive,
                StartWorkTime = t.StartWorkTime,
                EndWorkTime   = t.EndWorkTime
            })
            .ToListAsync();
        
        var schedules = await _db.LineSchedules.AsNoTracking().OrderBy(ls => ls.StartTime).ToListAsync();
        var stops = await _db.ScheduleStops.AsNoTracking().OrderBy(ss => ss.DepartureTime).ToListAsync();
        var assigns = await _db.TrainAssignments.AsNoTracking().ToListAsync();

        return new MapDataViewModel {
            Lines       = lines,
            TimeBetween = times,
            Trains      = trains,
            Schedules   = schedules,
            Stops       = stops,
            Assignments = assigns
        };
    }
    #endregion
}