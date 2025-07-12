using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Web.Models;

namespace RigaMetro.Services;

public class ScheduleService : IScheduleService {
    private const int StationDwellSeconds = 25; // station dwell time
    private const int TurnaroundPauseSeconds = 550; // pause between trips

    private readonly MetroDbContext _db;

    public ScheduleService(MetroDbContext db) {
        _db = db;
    }

    public async Task GenerateDailyScheduleAsync(string trainId, DateTime workDate) {
        var train = await LoadTrainAsync(trainId);
        var line = await LoadLineWithStationsAsync(train.LineID);
        var (startTime, endTime) = CalculateWorkWindow(train, line, workDate);

        await RemoveExistingSchedulesAsync(trainId);

        var isClockwise = true;
        var tripNumber = 1;
        var current = startTime;

        while (current < endTime) {
            // ScheduleID без даты
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

            current = lastDeparture.AddSeconds(TurnaroundPauseSeconds);
            isClockwise = !isClockwise;
            tripNumber++;
        }

        await _db.SaveChangesAsync();
    }

    // Убираем учёт даты при удалении — удаляем все старые рейсы по TrainID
    private async Task RemoveExistingSchedulesAsync(string trainId) {
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
        return await _db.Trains.FindAsync(trainId)
               ?? throw new InvalidOperationException($"Train '{trainId}' not found");
    }

    private async Task<Line> LoadLineWithStationsAsync(string lineId) {
        return await _db.Lines
            .Include(l => l.LineStations)
            .ThenInclude(ls => ls.Station)
            .FirstAsync(l => l.LineID == lineId);
    }
    
    private (DateTime start, DateTime end) CalculateWorkWindow(Train train, Line line, DateTime workDate) {
        var date = workDate.Date;
        var lineStart = line.StartWorkTime.TimeOfDay;
        var lineEnd = line.EndWorkTime.TimeOfDay;
        var trainStart = train.StartWorkTime;
        var trainEnd = train.EndWorkTime;
    
        // Обработка случая когда время переходит через полночь
        var startOffset = lineStart > trainStart ? lineStart : trainStart;
        var endOffset = lineEnd < trainEnd ? lineEnd : trainEnd;
    
        // Если конечное время меньше начального, значит работа через полночь
        if (endOffset < startOffset) {
            endOffset = endOffset.Add(TimeSpan.FromDays(1));
        }
    
        return (date + startOffset, date + endOffset);
    }

    private async Task<DateTime> GenerateStopsAndGetLastDepartureAsync(
        LineSchedule schedule,
        Line line,
        bool isClockwise,
        DateTime departureBase) {
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
            var departure = arrival.AddSeconds(StationDwellSeconds);

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
                cursor = departure.AddSeconds(travel.TimeSeconds);
            }
        }

        _db.ScheduleStops.AddRange(stops);
        return stops.Last().DepartureTime;
    }

    private void CreateAssignment(string scheduleId, string trainId) {
        _db.TrainAssignments.Add(new TrainAssignment {
            TrainID = trainId,
            ScheduleID = scheduleId,
            AssignmentDate = DateTime.Today // можно убрать дату, но оставляем для совместимости
        });
    }
}