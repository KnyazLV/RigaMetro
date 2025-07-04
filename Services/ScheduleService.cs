using Microsoft.EntityFrameworkCore;
using RigaMetro.Data;
using RigaMetro.Models;

namespace RigaMetro.Services;

public class ScheduleService : IScheduleService {
    private const int StationDwellSeconds = 20; // station dwell time
    private const int TurnaroundPauseSeconds = 500; // pause between trips

    private readonly MetroDbContext _db;

    public ScheduleService(MetroDbContext db) {
        _db = db;
    }

    public async Task GenerateDailyScheduleAsync(string trainId, DateTime workDate) {
        var train = await LoadTrainAsync(trainId);
        var line = await LoadLineWithStationsAsync(train.LineID);
        var (startTime, endTime) = CalculateWorkWindow(train, line, workDate);

        await RemoveExistingSchedulesAsync(trainId, workDate);

        var isClockwise = true;
        var tripNumber = 1;
        var current = startTime;

        while (current < endTime) {
            var schedule = CreateLineSchedule(trainId, line.LineID, workDate, tripNumber, isClockwise, current);
            _db.LineSchedules.Add(schedule);

            var lastDeparture = await GenerateStopsAndGetLastDepartureAsync(schedule, line, isClockwise, current);
            CreateAssignment(schedule.ScheduleID, trainId, workDate);

            current = lastDeparture.AddSeconds(TurnaroundPauseSeconds);
            isClockwise = !isClockwise;
            tripNumber++;
        }

        await _db.SaveChangesAsync();
    }

    #region Helpers

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
        var datePart = workDate.Date;
        var lineStart = line.StartWorkTime.TimeOfDay;
        var lineEnd = line.EndWorkTime.TimeOfDay;
        var trainStart = train.StartWorkTime;
        var trainEnd = train.EndWorkTime;

        var startOffset = lineStart > trainStart ? lineStart : trainStart;
        var endOffset = lineEnd < trainEnd ? lineEnd : trainEnd;

        return (datePart + startOffset, datePart + endOffset);
    }

    private async Task RemoveExistingSchedulesAsync(string trainId, DateTime workDate) {
        var assignments = await _db.TrainAssignments
            .Where(ta => ta.TrainID == trainId && ta.AssignmentDate == workDate.Date)
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

    private LineSchedule CreateLineSchedule(
        string trainId,
        string lineId,
        DateTime date,
        int tripNumber,
        bool isClockwise,
        DateTime startTime) {
        return new LineSchedule {
            ScheduleID = $"{trainId}_{date:yyyyMMdd}_{tripNumber}",
            LineID = lineId,
            TripNumber = tripNumber,
            IsClockwise = isClockwise,
            StartTime = startTime
        };
    }

    private async Task<DateTime> GenerateStopsAndGetLastDepartureAsync(LineSchedule schedule,
                                                                       Line line,
                                                                       bool isClockwise,
                                                                       DateTime departureBase) {
        var travelTimes = await _db.TimeBetweenStations
            .Where(t => line.LineStations.Select(ls => ls.StationID).Contains(t.FromStationID)
                        && line.LineStations.Select(ls => ls.StationID).Contains(t.ToStationID))
            .ToListAsync();

        var stationsOrdered = line.LineStations
            .OrderBy(ls => ls.StationOrder)
            .Select(ls => ls.StationID)
            .ToList();
        if (!isClockwise) stationsOrdered.Reverse();

        var stops = new List<ScheduleStop>();
        var timeCursor = departureBase;

        for (var i = 0; i < stationsOrdered.Count; i++) {
            var stationId = stationsOrdered[i];
            var arrivalTime = timeCursor;
            var departureTime = arrivalTime.AddSeconds(StationDwellSeconds);

            stops.Add(new ScheduleStop {
                ScheduleID = schedule.ScheduleID,
                StationOrder = i + 1,
                StationID = stationId,
                ArrivalTime = arrivalTime,
                DepartureTime = departureTime
            });

            if (i + 1 < stationsOrdered.Count) {
                var nextStationId = stationsOrdered[i + 1];
                var travel = travelTimes
                    .FirstOrDefault(t => (t.FromStationID == stationId && t.ToStationID == nextStationId)
                                         || (t.FromStationID == nextStationId && t.ToStationID == stationId));
                if (travel == null)
                    throw new InvalidOperationException(
                        $"Missing travel time between {stationId} and {nextStationId}");
                timeCursor = departureTime.AddSeconds(travel.TimeSeconds);
            }
        }

        _db.ScheduleStops.AddRange(stops);
        return stops.Last().DepartureTime;
    }


    private void CreateAssignment(string scheduleId, string trainId, DateTime date) {
        _db.TrainAssignments.Add(new TrainAssignment {
            TrainID = trainId,
            ScheduleID = scheduleId,
            AssignmentDate = date.Date
        });
    }

    #endregion
}