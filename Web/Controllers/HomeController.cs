using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
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
        const string trainId = "TR001";
        var workDate = DateTime.Today;

        await _scheduleService.GenerateDailyScheduleAsync(trainId, workDate);
        await _scheduleService.GenerateDailyScheduleAsync("TR002", workDate);

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
        /* ―–– 1. базовые справочники ―–– */
        var times = await _db.TimeBetweenStations
            .AsNoTracking()
            .Select(t => new TimeBetweenViewModel {
                FromStationID = t.FromStationID,
                ToStationID = t.ToStationID,
                DistanceM = t.DistanceM,
                TimeSeconds = t.TimeSeconds
            })
            .ToListAsync();

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

        /* ―–– 2. все остановки для построения расписания ―–– */
        var scheduleStops = await _db.ScheduleStops
            .AsNoTracking()
            .Include(s => s.Schedule)
            .ToListAsync();

        var stationSchedules = BuildStationSchedules(scheduleStops);

        /* ―–– 3. линии + терминальные станции ―–– */
        var lines = await _db.Lines
            .AsNoTracking()
            .Include(l => l.LineStations)
            .ThenInclude(ls => ls.Station)
            .ToListAsync();

        var lineViewModels = new List<LineWithStationsViewModel>();

        foreach (var line in lines) {
            /* список станций строго по порядку */
            var stations = line.LineStations
                .Select(ls => new StationViewModel {
                    StationID = ls.StationID,
                    Name = ls.Station!.Name,
                    Latitude = ls.Station.Latitude,
                    Longitude = ls.Station.Longitude,
                    Order = ls.StationOrder,
                    Schedule = stationSchedules.GetValueOrDefault(ls.StationID,
                        new Dictionary<string, ScheduleViewModel>())
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

        return new MapDataViewModel {
            Lines = lineViewModels,
            TimeBetween = times,
            Trains = trains
        };
    }


    private Dictionary<string, Dictionary<string, ScheduleViewModel>> BuildStationSchedules(List<ScheduleStop> scheduleStops) {
        var result = new Dictionary<string, Dictionary<string, ScheduleViewModel>>();

        var stopsByStation = scheduleStops.GroupBy(ss => ss.StationID);

        foreach (var stationGroup in stopsByStation) {
            var stationId = stationGroup.Key;
            var lineSchedules = new Dictionary<string, ScheduleViewModel>();
            var stopsByLine = stationGroup.GroupBy(ss => ss.Schedule.LineID);

            foreach (var lineGroup in stopsByLine) {
                var lineId = lineGroup.Key;
                var scheduleViewModel = new ScheduleViewModel();
  
                foreach (var stop in lineGroup)
                    if (stop.Schedule.IsClockwise)
                        scheduleViewModel.Clockwise.Add(stop.ArrivalTime);
                    else
                        scheduleViewModel.Counterclockwise.Add(stop.ArrivalTime);

                lineSchedules[lineId] = scheduleViewModel;
            }


            result[stationId] = lineSchedules;
        }

        return result;
    }

    #endregion
}