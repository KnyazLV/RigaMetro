using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Web.Models.ViewModels;
using RigaMetro.Web.Models.ViewModels.Admin;

namespace RigaMetro.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller {
    private readonly MetroDbContext _db;
    private readonly ILogger<AdminController> _logger;

    public AdminController(MetroDbContext db, ILogger<AdminController> logger) {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index() {
        var model = await CreateAdminDataAsync();
        return View(model);
    }


    private async Task<AdminDataViewModel> CreateAdminDataAsync() {
        var totalLines = await _db.Lines.CountAsync();
        var totalStations = await _db.Stations.CountAsync();
        var totalTrains = await _db.Trains.CountAsync();
        var activeTrains = await _db.Trains.CountAsync(t => t.IsActive);

        var lineStats = await _db.Lines
            .Include(l => l.LineStations)
            .Include(l => l.LineSchedules)
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
                DailyTripsCount = l.LineSchedules.Count,
                AssignedTrainsCount = l.Trains.Count()
            })
            .ToListAsync();

        // Считаем количество поездок по часам для каждой линии
        var hourlyTrips = await _db.LineSchedules
            .GroupBy(ls => new { ls.LineID, Hour = ls.StartTime.Hour })
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

}