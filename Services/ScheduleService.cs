using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Web.Models;

namespace RigaMetro.Services;

public class ScheduleService : IScheduleService {
    private const int StationDwellSeconds = 25; // station dwell time
    private const int TurnaroundPauseSeconds = 550; // pause between trips

    private readonly MetroDbContext _db;
    private readonly ILogger<ScheduleService> _logger;

    public ScheduleService(MetroDbContext db, ILogger<ScheduleService> logger) {
        _db = db;
        _logger = logger;
    }

    public async Task GenerateDailyScheduleAsync(string trainId) {
        var train = await LoadTrainAsync(trainId);
        var line = await LoadLineWithStationsAsync(train.LineID);
        var (startTime, endTime) = CalculateWorkWindow(train, line);

        await RemoveExistingSchedulesAsync(trainId);

        var isClockwise = true;
        var tripNumber = 1;
        var current = startTime;

        while (current < endTime) {
            var schedule = new LineSchedule {
                ScheduleID = $"{trainId}_{tripNumber}",
                LineID = line.LineID,
                TripNumber = tripNumber,
                IsClockwise = isClockwise,
                StartTime = current
            };
            _db.LineSchedules.Add(schedule);

            var lastDeparture = await GenerateStopsAndGetLastDepartureAsync(schedule, line, isClockwise, current);
            CreateAssignment(schedule.ScheduleID, trainId);

            current = lastDeparture + TimeSpan.FromSeconds(TurnaroundPauseSeconds);
            isClockwise = !isClockwise;
            tripNumber++;
        }

        await _db.SaveChangesAsync();
        _logger.LogInformation("Schedule generation for train {TrainID} complete.", trainId);
    }

    #region Helpers
    
    private async Task RemoveExistingSchedulesAsync(string trainId) {
        _logger.LogInformation("Removing previous schedules for train {TrainID}", trainId);
        var assignments = await _db.TrainAssignments
            .Where(ta => ta.TrainID == trainId)
            .ToListAsync();
        var scheduleIds = assignments.Select(a => a.ScheduleID).ToList();

        var stops = await _db.ScheduleStops
            .Where(ss => scheduleIds.Contains(ss.ScheduleID))
            .ToListAsync();
        var schedules = await _db.LineSchedules
            .Where(ls => scheduleIds.Contains(ls.ScheduleID))
            .ToListAsync();

        _db.ScheduleStops.RemoveRange(stops);
        _db.TrainAssignments.RemoveRange(assignments);
        _db.LineSchedules.RemoveRange(schedules);

        await _db.SaveChangesAsync();
    }

    private async Task<Train> LoadTrainAsync(string trainId) {
        var train = await _db.Trains.FindAsync(trainId);
        if (train != null) return train;
        _logger.LogError("Train '{TrainID}' not found", trainId);
        throw new InvalidOperationException($"Train '{trainId}' not found");
    }

    private async Task<Line> LoadLineWithStationsAsync(string lineId) {
        return await _db.Lines
            .Include(l => l.LineStations)
            .ThenInclude(ls => ls.Station)
            .FirstAsync(l => l.LineID == lineId);
    }
    
    private (TimeSpan start, TimeSpan end) CalculateWorkWindow(Train train, Line line) {
        var lineStart = NormalizeWorkTime(line.StartWorkTime, true);
        var lineEnd = NormalizeWorkTime(line.EndWorkTime, false);
        var trainStart = NormalizeWorkTime(train.StartWorkTime, true);
        var trainEnd = NormalizeWorkTime(train.EndWorkTime, false);

        var startOffset = lineStart > trainStart ? lineStart : trainStart;
        var endOffset = lineEnd < trainEnd ? lineEnd : trainEnd;

        if (endOffset < startOffset)
            endOffset = endOffset.Add(TimeSpan.FromDays(1));

        return (startOffset, endOffset);
    }

    private async Task<TimeSpan> GenerateStopsAndGetLastDepartureAsync(
        LineSchedule schedule,
        Line line,
        bool isClockwise,
        TimeSpan departureBase) {
        var travelTimes = await _db.TimeBetweenStations
            .ToListAsync();

        var ordered = line.LineStations
            .OrderBy(ls => ls.StationOrder)
            .Select(ls => ls.StationID)
            .ToList();
        if (!isClockwise) ordered.Reverse();

        var stops = new List<ScheduleStop>();
        var cursor = departureBase;

        for (var i = 0; i < ordered.Count; i++) {
            var stationId = ordered[i];
            var arrival = cursor;
            var departure = arrival.Add(TimeSpan.FromSeconds(StationDwellSeconds));

            stops.Add(new ScheduleStop {
                ScheduleID = schedule.ScheduleID,
                StationOrder = i + 1,
                StationID = stationId,
                ArrivalTime = arrival,
                DepartureTime = departure
            });

            if (i + 1 < ordered.Count) {
                var next = ordered[i + 1];
                var travel = travelTimes.FirstOrDefault(t => (t.FromStationID == stationId && t.ToStationID == next) ||
                                                             (t.FromStationID == next && t.ToStationID == stationId));
                if (travel == null)
                    throw new InvalidOperationException($"Missing travel time between {stationId} and {next}");
                cursor = departure.Add(TimeSpan.FromSeconds(travel.TimeSeconds));
            }
        }

        _db.ScheduleStops.AddRange(stops);
        return stops.Last().DepartureTime;
    }

    private void CreateAssignment(string scheduleId, string trainId) {
        _db.TrainAssignments.Add(new TrainAssignment {
            TrainID = trainId,
            ScheduleID = scheduleId,
        });
    }
    
    private static TimeSpan NormalizeWorkTime(TimeSpan time, bool isStart) {
        if (time == TimeSpan.Zero)
            return TimeSpan.FromMinutes(1); // 00:00 → 00:01
        if (time == TimeSpan.FromHours(24))
            return TimeSpan.FromHours(23).Add(TimeSpan.FromMinutes(59)); // 24:00 → 23:59
        return time;
    }
    
    #endregion
}