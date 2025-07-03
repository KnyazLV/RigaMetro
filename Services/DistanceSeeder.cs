using Microsoft.EntityFrameworkCore;
using RigaMetro.Models;
using RigaMetro.Data;

namespace RigaMetro.Services;

public class DistanceSeeder {
    private readonly MetroDbContext _db;

    public DistanceSeeder(MetroDbContext db) {
        _db = db;
    }

    /// <summary>
    /// Заполняет таблицу TimeBetweenStations расстояниями и временем между соседями по каждой линии.
    /// Вызывается единожды при инициализации.
    /// </summary>
    public async Task SeedAsync() {
        var lines = await _db.Lines
            .Include(l => l.LineStations.OrderBy(ls => ls.StationOrder))
            .ThenInclude(ls => ls.Station)
            .ToListAsync();

        var entities = new List<TimeBetweenStations>();

        foreach (var stations in lines.Select(line => line.LineStations.Select(ls => ls.Station).ToList())) {
            for (var i = 0; i < stations.Count - 1; i++)
            {
                var from = stations[i];
                var to   = stations[i + 1];

                var distance = CalculateDistanceMeters(
                    from.Latitude, from.Longitude,
                    to.Latitude,   to.Longitude);

                // Предположим средняя скорость метро 40 км/ч → 11.11 м/с
                var timeSec = (int)Math.Round(distance / 11.11);

                entities.Add(new TimeBetweenStations 
                {
                    FromStationID = from.StationID,
                    ToStationID   = to.StationID,
                    DistanceM     = (int)Math.Round(distance),
                    TimeSeconds   = timeSec
                });
            }
        }

        // Сохраняем в БД, если ещё не заполнено
        if (!await _db.TimeBetweenStations.AnyAsync())
        {
            await _db.TimeBetweenStations.AddRangeAsync(entities);
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
