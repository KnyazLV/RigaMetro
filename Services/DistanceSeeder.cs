using Microsoft.EntityFrameworkCore;
using RigaMetro.Infrastructure.Data;
using RigaMetro.Web.Models;

namespace RigaMetro.Services;

public class DistanceSeeder {
    private readonly MetroDbContext _db;

    public DistanceSeeder(MetroDbContext db) {
        _db = db;
    }

    public async Task SeedAsync() {
        var lines = await _db.Lines
            .Include(l => l.LineStations.OrderBy(ls => ls.StationOrder))
            .ThenInclude(ls => ls.Station)
            .ToListAsync();

        var pairs = await _db.TimeBetweenStations
            .AsNoTracking()
            .Select(t => new { t.FromStationID, t.ToStationID })
            .ToListAsync();

        var existing = pairs
            .Select(x => (x.FromStationID, x.ToStationID))
            .ToHashSet();

        var toInsert = new List<TimeBetweenStations>();

        foreach (var line in lines) {
            var stations = line.LineStations.Select(ls => ls.Station!).ToList();
            for (var i = 0; i + 1 < stations.Count; i++) {
                var from = stations[i];
                var to = stations[i + 1];

                if (existing.Contains((from.StationID, to.StationID))
                    || existing.Contains((to.StationID, from.StationID)))
                    continue;

                var distance = CalculateDistanceMeters(
                    from.Latitude, from.Longitude,
                    to.Latitude, to.Longitude);

                var timeSec = (int)Math.Round(distance / 11.11);

                toInsert.Add(new TimeBetweenStations {
                    FromStationID = from.StationID,
                    ToStationID = to.StationID,
                    DistanceM = (int)Math.Round(distance),
                    TimeSeconds = timeSec
                });

                toInsert.Add(new TimeBetweenStations {
                    FromStationID = to.StationID,
                    ToStationID = from.StationID,
                    DistanceM = (int)Math.Round(distance),
                    TimeSeconds = timeSec
                });
            }
        }

        if (toInsert.Any()) {
            await _db.TimeBetweenStations.AddRangeAsync(toInsert);
            await _db.SaveChangesAsync();
        }
    }


    private static double CalculateDistanceMeters(double latitude1, double longitude1, double latitude2, double longitude2) {
        const double earthRadius = 6371000; // meters
        var latitudeRad1 = latitude1 * Math.PI / 180.0;
        var latitudeRad2 = latitude2 * Math.PI / 180.0;
        var deltaLatitude = (latitude2 - latitude1) * Math.PI / 180.0;
        var deltaLongitude = (longitude2 - longitude1) * Math.PI / 180.0;

        var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                Math.Cos(latitudeRad1) * Math.Cos(latitudeRad2) *
                Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

        return earthRadius * c;
    }
}