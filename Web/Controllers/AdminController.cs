using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Resources;
using RigaMetro.Services;
using RigaMetro.Web.Models;
using RigaMetro.Web.Models.ViewModels;
using RigaMetro.Web.Models.ViewModels.Admin;

namespace RigaMetro.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller {
    private readonly MetroDbContext _db;
    private readonly IScheduleService _scheduleService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(MetroDbContext db, IScheduleService scheduleService, ILogger<AdminController> logger) {
        _db = db;
        _scheduleService = scheduleService;
        _logger = logger;
    }


    public async Task<IActionResult> Index() {
        var model = await CreateAdminDataAsync();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTrain([FromBody] TrainViewModel model) {
        if (model.StartWorkTime >= model.EndWorkTime)
            return BadRequest("StartWorkTime must be earlier than EndWorkTime");

        var train = await _db.Trains.FindAsync(model.TrainID);
        if (train == null) return NotFound();

        train.TrainName = model.TrainName;
        train.LineID = model.LineID;
        train.StartWorkTime = model.StartWorkTime;
        train.EndWorkTime = model.EndWorkTime;
        train.IsActive = model.IsActive;
        await _db.SaveChangesAsync();

        if (train.IsActive) await _scheduleService.GenerateDailyScheduleAsync(train.TrainID);

        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrain([FromBody] string id) {
        // Удалить все связанные расписания, остановки и назначения
        var assignments = await _db.TrainAssignments.Where(a => a.TrainID == id).ToListAsync();
        var scheduleIds = assignments.Select(a => a.ScheduleID).ToList();
        var stops = await _db.ScheduleStops.Where(s => scheduleIds.Contains(s.ScheduleID)).ToListAsync();
        var schedules = await _db.LineSchedules.Where(s => scheduleIds.Contains(s.ScheduleID)).ToListAsync();

        _db.ScheduleStops.RemoveRange(stops);
        _db.TrainAssignments.RemoveRange(assignments);
        _db.LineSchedules.RemoveRange(schedules);

        var train = await _db.Trains.FindAsync(id);
        if (train == null)
            return NotFound("Поезд не найден");
        _db.Trains.Remove(train);

        await _db.SaveChangesAsync();
        return Ok();
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrain([FromBody] TrainViewModel model) {
        if (model.StartWorkTime >= model.EndWorkTime)
            return BadRequest("StartWorkTime must be earlier than EndWorkTime");

        var lineNumber = int.Parse(model.LineID.Substring(model.LineID.Length - 2)); // LN02 -> 2

        var existingTrains = await _db.Trains
            .Where(t => t.TrainID.StartsWith($"TR{lineNumber}"))
            .ToListAsync();

        var maxNum = existingTrains
            .Select(t => int.TryParse(t.TrainID.Substring(3), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();
        var newNum = maxNum + 1;
        var newId = $"TR{lineNumber}{newNum:00}";


        var train = new Train {
            TrainID = newId,
            TrainName = model.TrainName,
            LineID = model.LineID,
            StartWorkTime = model.StartWorkTime,
            EndWorkTime = model.EndWorkTime,
            IsActive = model.IsActive
        };
        _db.Trains.Add(train);
        await _db.SaveChangesAsync();

        if (train.IsActive)
            await _scheduleService.GenerateDailyScheduleAsync(newId);

        return Ok(new { newId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLine([FromBody] LineAdminSettingsViewModel model) {
        if (model == null)
            return BadRequest("Модель обновления не передана");

        if (model.StartWorkTime >= model.EndWorkTime)
            return BadRequest("StartWorkTime must be earlier than EndWorkTime");

        var line = await _db.Lines.FindAsync(model.LineID);
        if (line == null) return NotFound();
        
        line.StartWorkTime = model.StartWorkTime;
        line.EndWorkTime   = model.EndWorkTime;
        line.Color = model.Color;
        await _db.SaveChangesAsync();

        var schedules = await _db.LineSchedules
            .Where(s => s.LineID == line.LineID)
            .ToListAsync();
        
        _db.LineSchedules.RemoveRange(schedules);
        await _db.SaveChangesAsync();

        var activeTrains = await _db.Trains
            .Where(t => t.LineID == line.LineID && t.IsActive)
            .Select(t => t.TrainID)
            .ToListAsync();

        foreach (var trainId in activeTrains) {
            await _scheduleService.GenerateDailyScheduleAsync(trainId);
        }

        return Ok();
    }
    
    private async Task<AdminDataViewModel> CreateAdminDataAsync() {
        var totalLines = await _db.Lines.CountAsync();
        var totalStations = await _db.Stations.CountAsync();
        var totalTrains = await _db.Trains.CountAsync();
        var activeTrains = await _db.Trains.CountAsync(t => t.IsActive);
        
        var activeTrainIds = await _db.Trains
            .Where(t => t.IsActive)
            .Select(t => t.TrainID)
            .ToListAsync();

        var lineStats = await _db.Lines
            .Include(l => l.LineStations)
            .Include(l => l.Trains)
            .Select(l => new LineStatisticsViewModel {
                LineID = l.LineID,
                LineName = l.Name,
                LineColor = l.Color,
                StationCount = l.LineStations.Count,
                TotalDistanceKm = l.LineStations
                    .Join(_db.TimeBetweenStations,
                        ls => ls.StationID,
                        t => t.FromStationID,
                        (ls, t) => t.DistanceM)
                    .Sum() / 1000.0,
                AssignedTrainsCount = l.Trains.Count(t => t.IsActive),
                DailyTripsCount = _db.LineSchedules.Count(s => s.LineID == l.LineID && 
                                                               activeTrainIds.Any(trainId => s.ScheduleID.StartsWith(trainId + "_")))
            })
            .ToListAsync();

        // Считаем количество поездок по часам для каждой линии
        var hourlyTrips = await _db.LineSchedules
            .Where(ls => activeTrainIds.Any(trainId => ls.ScheduleID.StartsWith(trainId + "_")))
            .GroupBy(ls => new { ls.LineID, Hour = ls.StartTime.Hours })
            .Select(g => new { g.Key.LineID, g.Key.Hour, Count = g.Count() })
            .ToListAsync();

        // Заполняем словарь <LineID, int[24]>
        var hourlyDict = lineStats.ToDictionary(
            ls => ls.LineID,
            _ => Enumerable.Repeat(0, 24).ToArray());

        foreach (var h in hourlyTrips)
            hourlyDict[h.LineID][h.Hour] = h.Count;


        // Формируем объект для Chart.js
        var chartDto = new {
            labels = Enumerable.Range(0, 24).Select(i => i.ToString("00")),
            datasets = lineStats.Select(ls => new {
                label = ls.LineName,
                data = hourlyDict[ls.LineID],
                borderColor = ls.LineColor,
                backgroundColor = ls.LineColor,
                tension = 0.3,
                fill = false
            })
        };

        var statsVm = new AdminStatisticsViewModel {
            TotalLines = totalLines,
            TotalStations = totalStations,
            TotalTrains = totalTrains,
            ActiveTrains = activeTrains,
            TotalNetworkDistanceKm = lineStats.Sum(s => s.TotalDistanceKm),
            TotalDailyTrips = lineStats.Sum(s => s.DailyTripsCount),
            LineStatistics = lineStats,
            TripsPerLineHourlyJson = JsonSerializer.Serialize(chartDto)
        };

        var lines = await _db.Lines
            .AsNoTracking()
            .Select(l => new LineAdminSettingsViewModel {
                LineID = l.LineID,
                StartWorkTime = l.StartWorkTime,
                EndWorkTime   = l.EndWorkTime,
                Color = l.Color,
                Name = l.Name
            })
            .ToListAsync();

        var trains = await _db.Trains
            .AsNoTracking()
            .Select(t => new TrainViewModel {
                TrainID = t.TrainID,
                LineID = t.LineID,
                TrainName = t.TrainName ?? "",
                IsActive = t.IsActive,
                StartWorkTime = t.StartWorkTime,
                EndWorkTime = t.EndWorkTime
            })
            .ToListAsync();

        return new AdminDataViewModel {
            AdminStatistics = statsVm,
            Lines = lines,
            Trains = trains
        };
    }
}