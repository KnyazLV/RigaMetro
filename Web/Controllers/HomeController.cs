using System.Diagnostics;
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
        _logger.LogInformation("Loading main map page");

        try {
            var model = await CreateMapDataViewModel();
            ViewData["MapboxToken"] = _configuration["MapBox:ApiKey"];

            _logger.LogInformation("Successfully loaded");
            return View(model);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to load map data");
            throw;
        }
    }

    #endregion

    #region Language Management

    /// <summary>
    /// Changes the application language and redirects back to the previous page
    /// </summary>
    /// <param name="culture">Target culture code</param>
    /// <returns>Redirect to previous page</returns>
    public IActionResult ChangeLanguage(string culture) {
        if (string.IsNullOrEmpty(culture)) {
            _logger.LogWarning("Attempted to change language with empty culture");
            return BadRequest("Culture parameter is required");
        }

        try {
            var cookieValue = CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture));
            var cookieOptions = new CookieOptions {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                HttpOnly = true,
                Secure = Request.IsHttps,
                SameSite = SameSiteMode.Lax
            };

            Response.Cookies.Append(CookieRequestCultureProvider.DefaultCookieName, cookieValue, cookieOptions);

            _logger.LogInformation("Language changed to {Culture}", culture);

            var referer = Request.Headers["Referer"].ToString();
            return !string.IsNullOrEmpty(referer) ? Redirect(referer) : RedirectToAction(nameof(Index));
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to change language to {Culture}", culture);
            return RedirectToAction(nameof(Index));
        }
    }

    #endregion

    #region ViewModelCreation

    private async Task<MapDataViewModel> CreateMapDataViewModel() {
        _logger.LogDebug("Creating map data view model");

        var stopwatch = Stopwatch.StartNew();

        try {
            var trains = await GetActiveTrainsAsync();
            var scheduleStops = await GetScheduleStopsAsync();
            var lines = await GetLinesWithStationsAsync();

            var stationSchedules = BuildStationSchedules(scheduleStops);
            var lineViewModels = BuildLineViewModels(lines, stationSchedules);

            var model = new MapDataViewModel {
                Lines = lineViewModels,
                Trains = trains
            };

            stopwatch.Stop();
            _logger.LogInformation("Map data view model created in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);

            return model;
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to create map data view model");
            throw;
        }
    }

    private async Task<List<TrainViewModel>> GetActiveTrainsAsync() {
        return await _db.Trains
            .AsNoTracking()
            .Where(t => t.IsActive)
            .Select(t => new TrainViewModel {
                TrainID = t.TrainID,
                LineID = t.LineID,
                TrainName = t.TrainName,
                IsActive = t.IsActive,
                StartWorkTime = t.StartWorkTime,
                EndWorkTime = t.EndWorkTime
            })
            .ToListAsync();
    }

    private async Task<List<ScheduleStop>> GetScheduleStopsAsync() {
        return await _db.ScheduleStops
            .AsNoTracking()
            .Include(s => s.Schedule)
            .ToListAsync();
    }

    private async Task<List<Line>> GetLinesWithStationsAsync() {
        return await _db.Lines
            .AsNoTracking()
            .Include(l => l.LineStations)
            .ThenInclude(ls => ls.Station)
            .ToListAsync();
    }

    private List<LineWithStationsViewModel> BuildLineViewModels(
        List<Line> lines,
        Dictionary<string, Dictionary<string, ScheduleViewModel>> stationSchedules) {
        var lineViewModels = new List<LineWithStationsViewModel>();

        foreach (var line in lines) {
            var stations = BuildStationViewModels(line, stationSchedules);

            lineViewModels.Add(new LineWithStationsViewModel {
                LineID = line.LineID,
                Name = line.Name,
                Color = line.Color,
                Stations = stations,
                ClockwiseTerminal = stations.LastOrDefault()?.Name ?? string.Empty,
                CounterclockwiseTerminal = stations.FirstOrDefault()?.Name ?? string.Empty
            });
        }

        return lineViewModels;
    }

    private List<StationViewModel> BuildStationViewModels(
        Line line,
        Dictionary<string, Dictionary<string, ScheduleViewModel>> stationSchedules) {
        return line.LineStations
            .OrderBy(lineStation => lineStation.StationOrder)
            .Select(lineStation => new StationViewModel {
                StationID = lineStation.StationID,
                Name = lineStation.Station?.Name ?? string.Empty,
                Latitude = lineStation.Station?.Latitude ?? 0,
                Longitude = lineStation.Station?.Longitude ?? 0,
                Order = lineStation.StationOrder,
                Schedule = stationSchedules.TryGetValue(lineStation.StationID, out var schedule)
                    ? schedule
                    : new Dictionary<string, ScheduleViewModel>()
            })
            .ToList();
    }

    private Dictionary<string, Dictionary<string, ScheduleViewModel>> BuildStationSchedules(List<ScheduleStop> stops) {
        var result = new Dictionary<string, Dictionary<string, ScheduleViewModel>>();

        var groupedByStation = stops.GroupBy(scheduleStop => scheduleStop.StationID);

        foreach (var stationGroup in groupedByStation) {
            var lineSchedules = new Dictionary<string, ScheduleViewModel>();
            var groupedByLine = stationGroup.GroupBy(ss => ss.Schedule.LineID);

            foreach (var lineGroup in groupedByLine) {
                var viewModel = new ScheduleViewModel();

                foreach (var stop in lineGroup)
                    if (stop.Schedule.IsClockwise)
                        viewModel.Clockwise.Add(stop.ArrivalTime);
                    else
                        viewModel.Counterclockwise.Add(stop.ArrivalTime);

                lineSchedules[lineGroup.Key] = viewModel;
            }

            result[stationGroup.Key] = lineSchedules;
        }

        return result;
    }

    #endregion
}