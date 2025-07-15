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

    #region Main Action

    public async Task<IActionResult> Index() {
        try {
            _logger.LogInformation("Loading admin dashboard");
            var model = await CreateAdminDataAsync();
            return View(model);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to load admin dashboard");
            throw;
        }
    }

    #endregion

    #region Train CRUD

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateTrain([FromBody] TrainViewModel model) {
        try {
            _logger.LogInformation("Updating train {TrainId}", model.TrainID);

            var validationResult = ValidateTimes(model.StartWorkTime, model.EndWorkTime);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var train = await _db.Trains.FindAsync(model.TrainID);
            if (train == null) {
                _logger.LogWarning("Train {TrainId} not found for update", model.TrainID);
                return NotFound();
            }

            await UpdateTrainEntity(train, model);
            await _db.SaveChangesAsync();

            if (train.IsActive)
                await _scheduleService.GenerateDailyScheduleAsync(train.TrainID);

            _logger.LogInformation("Train {TrainId} updated successfully", model.TrainID);
            return Ok();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to update train {TrainId}", model.TrainID);
            return StatusCode(500, "Failed to update train");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteTrain([FromBody] string id) {
        try {
            _logger.LogInformation("Deleting train {TrainId}", id);

            if (string.IsNullOrEmpty(id))
                return BadRequest("Train ID is required");

            var train = await _db.Trains.FindAsync(id);
            if (train == null) {
                _logger.LogWarning("Train {TrainId} not found for deletion", id);
                return NotFound("Train not found");
            }

            await DeleteTrainWithRelatedData(id);
            await _db.SaveChangesAsync();

            _logger.LogInformation("Train {TrainId} deleted successfully", id);
            return Ok();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to delete train {TrainId}", id);
            return StatusCode(500, "Failed to delete train");
        }
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateTrain([FromBody] TrainViewModel model) {
        try {
            _logger.LogInformation("Creating new train for line {LineId}", model.LineID);

            var validationResult = ValidateTimes(model.StartWorkTime, model.EndWorkTime);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var newId = await GenerateTrainId(model.LineID);
            if (string.IsNullOrEmpty(newId))
                return BadRequest("Failed to generate train ID");

            var train = CreateTrainEntity(newId, model);
            _db.Trains.Add(train);
            await _db.SaveChangesAsync();

            if (train.IsActive)
                await _scheduleService.GenerateDailyScheduleAsync(newId);

            _logger.LogInformation("Train {TrainId} created successfully", newId);
            return Ok(new { newId });
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to create train for line {LineId}", model.LineID);
            return StatusCode(500, "Failed to create train");
        }
    }

    #endregion

    #region Line CRUD

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateLine([FromBody] LineAdminSettingsViewModel model) {
        try {
            _logger.LogInformation("Updating line {LineId}", model.LineID);

            if (model == null)
                return BadRequest("Line model is required");

            var validationResult = ValidateTimes(model.StartWorkTime, model.EndWorkTime);
            if (!validationResult.IsValid)
                return BadRequest(validationResult.Message);

            var line = await _db.Lines.FindAsync(model.LineID);
            if (line == null) {
                _logger.LogWarning("Line {LineId} not found for update", model.LineID);
                return NotFound();
            }

            await UpdateLineEntity(line, model);
            await RegenerateLineSchedules(line.LineID);

            _logger.LogInformation("Line {LineId} updated successfully", model.LineID);
            return Ok();
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to update line {LineId}", model.LineID);
            return StatusCode(500, "Failed to update line");
        }
    }

    #endregion

    #region Data for ViewModels

    private async Task<AdminDataViewModel> CreateAdminDataAsync() {
        var totalLines = await _db.Lines.CountAsync();
        var totalStations = await _db.Stations.CountAsync();
        var totalTrains = await _db.Trains.CountAsync();
        var activeTrains = await _db.Trains.CountAsync(t => t.IsActive);

        var activeTrainIds = await _db.Trains
            .Where(t => t.IsActive)
            .Select(t => t.TrainID)
            .ToListAsync();

        var lineStats = await BuildLineStatistics(activeTrainIds);
        var hourlyTrips = await BuildHourlyTripsData(activeTrainIds);
        var chartData = CreateChartData(lineStats, hourlyTrips);

        var statsVm = new AdminStatisticsViewModel {
            TotalLines = totalLines,
            TotalStations = totalStations,
            TotalTrains = totalTrains,
            ActiveTrains = activeTrains,
            TotalNetworkDistanceKm = lineStats.Sum(s => s.TotalDistanceKm),
            TotalDailyTrips = lineStats.Sum(s => s.DailyTripsCount),
            LineStatistics = lineStats,
            TripsPerLineHourlyJson = JsonSerializer.Serialize(chartData)
        };

        var lines = await _db.Lines
            .AsNoTracking()
            .Select(l => new LineAdminSettingsViewModel {
                LineID = l.LineID,
                StartWorkTime = l.StartWorkTime,
                EndWorkTime = l.EndWorkTime,
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

    private async Task<List<LineStatisticsViewModel>> BuildLineStatistics(List<string> activeTrainIds) {
        return await _db.Lines
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
    }

    private async Task<List<dynamic>> BuildHourlyTripsData(List<string> activeTrainIds) {
        return await _db.LineSchedules
            .Where(ls => activeTrainIds.Any(trainId => ls.ScheduleID.StartsWith(trainId + "_")))
            .GroupBy(ls => new { ls.LineID, Hour = ls.StartTime.Hours })
            .Select(g => new { g.Key.LineID, g.Key.Hour, Count = g.Count() })
            .ToListAsync<dynamic>();
    }

    private object CreateChartData(List<LineStatisticsViewModel> lineStats, List<dynamic> hourlyTrips) {
        var hourlyDict = lineStats.ToDictionary(
            ls => ls.LineID,
            _ => Enumerable.Repeat(0, 24).ToArray());

        foreach (var h in hourlyTrips)
            hourlyDict[h.LineID][h.Hour] = h.Count;

        return new {
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
    }

    #endregion

    #region Helpers

    private (bool IsValid, string Message) ValidateTimes(TimeSpan startTime, TimeSpan endTime) {
        if (startTime >= endTime)
            return (false, "Start time must be earlier than end time");

        return (true, string.Empty);
    }

    private async Task UpdateTrainEntity(Train train, TrainViewModel model) {
        train.TrainName = model.TrainName;
        train.LineID = model.LineID;
        train.StartWorkTime = model.StartWorkTime;
        train.EndWorkTime = model.EndWorkTime;
        train.IsActive = model.IsActive;
    }

    private async Task DeleteTrainWithRelatedData(string trainId) {
        var assignments = await _db.TrainAssignments.Where(a => a.TrainID == trainId).ToListAsync();
        var scheduleIds = assignments.Select(a => a.ScheduleID).ToList();

        if (scheduleIds.Any()) {
            var stops = await _db.ScheduleStops.Where(s => scheduleIds.Contains(s.ScheduleID)).ToListAsync();
            var schedules = await _db.LineSchedules.Where(s => scheduleIds.Contains(s.ScheduleID)).ToListAsync();

            _db.ScheduleStops.RemoveRange(stops);
            _db.LineSchedules.RemoveRange(schedules);
        }

        _db.TrainAssignments.RemoveRange(assignments);

        var train = await _db.Trains.FindAsync(trainId);
        if (train != null)
            _db.Trains.Remove(train);
    }

    private async Task<string> GenerateTrainId(string lineId) {
        try {
            if (string.IsNullOrEmpty(lineId) || lineId.Length < 4)
                return null;

            var lineNumber = int.Parse(lineId.Substring(lineId.Length - 2));
            var existingTrains = await _db.Trains
                .Where(t => t.TrainID.StartsWith($"TR{lineNumber}"))
                .ToListAsync();

            var maxNum = existingTrains
                .Select(t => int.TryParse(t.TrainID.Substring(3), out var n) ? n : 0)
                .DefaultIfEmpty(0)
                .Max();

            var newNum = maxNum + 1;
            return $"TR{lineNumber}{newNum:00}";
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Failed to generate train ID for line {LineId}", lineId);
            return null;
        }
    }

    private Train CreateTrainEntity(string trainId, TrainViewModel model) {
        return new Train {
            TrainID = trainId,
            TrainName = model.TrainName,
            LineID = model.LineID,
            StartWorkTime = model.StartWorkTime,
            EndWorkTime = model.EndWorkTime,
            IsActive = model.IsActive
        };
    }

    private async Task UpdateLineEntity(Line line, LineAdminSettingsViewModel model) {
        line.StartWorkTime = model.StartWorkTime;
        line.EndWorkTime = model.EndWorkTime;
        line.Color = model.Color;
        await _db.SaveChangesAsync();
    }

    private async Task RegenerateLineSchedules(string lineId) {
        var schedules = await _db.LineSchedules
            .Where(s => s.LineID == lineId)
            .ToListAsync();

        _db.LineSchedules.RemoveRange(schedules);
        await _db.SaveChangesAsync();

        var activeTrains = await _db.Trains
            .Where(t => t.LineID == lineId && t.IsActive)
            .Select(t => t.TrainID)
            .ToListAsync();

        foreach (var trainId in activeTrains) await _scheduleService.GenerateDailyScheduleAsync(trainId);
    }

    #endregion
}