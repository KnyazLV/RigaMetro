using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Resources;
using RigaMetro.Services;
using RigaMetro.Web.Models;
using RigaMetro.Web.Models.ViewModels;
using RigaMetro.Web.Models.ViewModels.Schedule;

namespace RigaMetro.Web.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IScheduleService _scheduleService;
    private readonly MetroDbContext _db;

    public HomeController(ILogger<HomeController> logger,
                          IConfiguration configuration,
                          MetroDbContext db,
                          IScheduleService scheduleService) {
        _logger = logger;
        _configuration = configuration;
        _db = db;
        _scheduleService = scheduleService;
    }

    #region Actions

    public async Task<IActionResult> Index() {

        var model = await CreateMapDataViewModel();
        ViewData["MapboxToken"] = _configuration["MapBox:ApiKey"];
        return View(model);
    }
    
    public IActionResult ChangeLanguage(string culture) {
        Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)), new CookieOptions() {
            Expires = DateTimeOffset.UtcNow.AddYears(1)
        });
        return Redirect(Request.Headers["Referer"].ToString());
    }

    public async Task<IActionResult> DebugData() {
        var model = await CreateMapDataViewModel();
        return View(model);
    }

    #endregion

    #region ViewModelCreation

    private async Task<MapDataViewModel> CreateMapDataViewModel() {

        var trains = await _db.Trains
            .AsNoTracking()
            .Select(t => new TrainViewModel {
                TrainID = t.TrainID,
                LineID = t.LineID,
                TrainName = t.TrainName,
                IsActive = t.IsActive,
                StartWorkTime = t.StartWorkTime,
                EndWorkTime = t.EndWorkTime
            })
            .ToListAsync();

        var scheduleStops = await _db.ScheduleStops
            .AsNoTracking()
            .Include(s => s.Schedule)
            .ToListAsync();

        var stationSchedules = BuildStationSchedules(scheduleStops);

        var lines = await _db.Lines
            .AsNoTracking()
            .Include(l => l.LineStations)
            .ThenInclude(ls => ls.Station)
            .ToListAsync();

        var lineViewModels = new List<LineWithStationsViewModel>();

        foreach (var line in lines) {
            var stations = line.LineStations
                .OrderBy(ls => ls.StationOrder)
                .Select(ls => new StationViewModel
                {
                    StationID = ls.StationID,
                    Name      = ls.Station!.Name,
                    Latitude  = ls.Station.Latitude,
                    Longitude = ls.Station.Longitude,
                    Order     = ls.StationOrder,
                    Schedule  = stationSchedules.ContainsKey(ls.StationID)
                        ? stationSchedules[ls.StationID]
                        : new Dictionary<string, ScheduleViewModel>()
                })
                .ToList();


            lineViewModels.Add(new LineWithStationsViewModel {
                LineID = line.LineID,
                Name = line.Name,
                Color = line.Color,
                Stations = stations,
                ClockwiseTerminal = stations.Last().Name,
                CounterclockwiseTerminal = stations.First().Name
            });
        }

        var model = new MapDataViewModel() {
            Lines = lineViewModels,
            // TimeBetween = times,
            Trains = trains,
        };
        
        return model;
    }


    private Dictionary<string, Dictionary<string, ScheduleViewModel>> BuildStationSchedules(List<ScheduleStop> stops)
    {
        var result = new Dictionary<string, Dictionary<string, ScheduleViewModel>>();

        foreach (var stationGroup in stops.GroupBy(ss => ss.StationID))
        {
            var lineSchedules = new Dictionary<string, ScheduleViewModel>();

            foreach (var lineGroup in stationGroup.GroupBy(ss => ss.Schedule.LineID))
            {
                // создаём новый ScheduleViewModel для каждой линии
                var vm = new ScheduleViewModel();
                foreach (var stop in lineGroup)
                    if (stop.Schedule.IsClockwise) vm.Clockwise.Add(stop.ArrivalTime);
                    else vm.Counterclockwise.Add(stop.ArrivalTime);
                lineSchedules[lineGroup.Key] = vm;
            }

            // НИКОГДА не reuse lineSchedules из предыдущей итерации
            result[stationGroup.Key] = lineSchedules;
        }

        return result;
    }


    #endregion
}