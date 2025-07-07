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
        await _scheduleService.GenerateDailyScheduleAsync("TR201", workDate);
        await _scheduleService.GenerateDailyScheduleAsync("TR202", workDate);
        await _scheduleService.GenerateDailyScheduleAsync("TR301", workDate);
        await _scheduleService.GenerateDailyScheduleAsync("TR302", workDate);

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
                .OrderBy(ls => ls.StationOrder)          // ← Сортируем здесь
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
            TimeBetween = times,
            Trains = trains,
        };
        
        foreach (var line in model.Lines)
            foreach (var st in line.Stations)
                _logger.LogInformation("Station {0} has {1} lines in Schedule",
                    st.StationID, st.Schedule.Count);

        return model;

        // return new MapDataViewModel {
        //     Lines = lineViewModels,
        //     TimeBetween = times,
        //     Trains = trains
        // };
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