using Microsoft.EntityFrameworkCore;
using RigaMetro.Models;

namespace RigaMetro.Data;

public class MetroDbContext : DbContext {
    public MetroDbContext(DbContextOptions<MetroDbContext> options) : base(options) {}

    public DbSet<Line> Lines { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<LineStation> LineStations { get; set; }
    public DbSet<TimeBetweenStations> TimeBetweenStations { get; set; }
    public DbSet<Train> Trains { get; set; }
    public DbSet<LineSchedule> LineSchedules { get; set; }
    public DbSet<ScheduleStop> ScheduleStops { get; set; }
    public DbSet<TrainAssignment> TrainAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // Настройка всех DateTime полей на timestamp without time zone
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
        {
            property.SetColumnType("timestamp without time zone");
        }

        // Составные ключи 
        modelBuilder.Entity<LineStation>().HasKey(ls => new { ls.LineID, ls.StationID });
        modelBuilder.Entity<TimeBetweenStations>().HasKey(t => new { t.FromStationID, t.ToStationID });
        modelBuilder.Entity<ScheduleStop>().HasKey(ss => new { ss.ScheduleID, ss.StationOrder });
        modelBuilder.Entity<TrainAssignment>().HasKey(ta => new { ta.TrainID, ta.ScheduleID, ta.AssignmentDate });

        // Настройка связей для TimeBetweenStations
        modelBuilder.Entity<TimeBetweenStations>()
            .HasOne(t => t.FromStation)
            .WithMany(s => s.TimeFrom)
            .HasForeignKey(t => t.FromStationID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TimeBetweenStations>()
            .HasOne(t => t.ToStation)
            .WithMany(s => s.TimeTo)
            .HasForeignKey(t => t.ToStationID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Line>().HasData(
            new Line 
            { 
                LineID = 1, 
                Name = "Sarkandaugava–Ziepniekkalns", 
                Color = "#FF0000", 
                IsClockwiseDirection = true, 
                StartWorkTime = new DateTime(2000, 1, 1, 6, 0, 0), // произвольная дата + время
                EndWorkTime = new DateTime(2000, 1, 1, 23, 0, 0) 
            }
        );

        modelBuilder.Entity<Station>().HasData(
            new Station { StationID = 1, Name = "Sarkandaugava", Latitude = 57.003384, Longitude = 24.118737 },
            new Station { StationID = 2, Name = "Rupnica RER", Latitude = 56.991002, Longitude = 24.122138 },
            new Station { StationID = 3, Name = "Ramulu iela", Latitude = 56.974579, Longitude = 24.111671 },
            new Station { StationID = 4, Name = "Petersala", Latitude = 56.964006, Longitude = 24.106183 },
            new Station { StationID = 5, Name = "Kronvalda parks", Latitude = 56.956935, Longitude = 24.101257 },
            new Station { StationID = 6, Name = "Stacijas laukums", Latitude = 56.947561, Longitude = 24.119752 },
            new Station { StationID = 7, Name = "Zaķusala", Latitude = 56.933003, Longitude = 24.121713 },
            new Station { StationID = 8, Name = "Straume", Latitude = 56.919697, Longitude = 24.098176 },
            new Station { StationID = 9, Name = "Dzintars", Latitude = 56.912930, Longitude = 24.069526 },
            new Station { StationID = 10, Name = "Ziepniekkalns", Latitude = 56.898448, Longitude = 24.092073 }
        );

        modelBuilder.Entity<LineStation>().HasData(
            new LineStation { LineID = 1, StationID = 1, StationOrder = 1 },
            new LineStation { LineID = 1, StationID = 2, StationOrder = 2 },
            new LineStation { LineID = 1, StationID = 3, StationOrder = 3 },
            new LineStation { LineID = 1, StationID = 4, StationOrder = 4 },
            new LineStation { LineID = 1, StationID = 5, StationOrder = 5 },
            new LineStation { LineID = 1, StationID = 6, StationOrder = 6 },
            new LineStation { LineID = 1, StationID = 7, StationOrder = 7 },
            new LineStation { LineID = 1, StationID = 8, StationOrder = 8 },
            new LineStation { LineID = 1, StationID = 9, StationOrder = 9 },
            new LineStation { LineID = 1, StationID = 10, StationOrder = 10 }
        );
    }
}
