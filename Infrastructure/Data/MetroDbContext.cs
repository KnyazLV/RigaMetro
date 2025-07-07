using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using RigaMetro.Web.Models;

namespace RigaMetro.Infrastructure.Data;

public class MetroDbContext : DbContext {
    public MetroDbContext(DbContextOptions<MetroDbContext> options) : base(options) { }

    public DbSet<Line> Lines { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<LineStation> LineStations { get; set; }
    public DbSet<TimeBetweenStations> TimeBetweenStations { get; set; }
    public DbSet<Train> Trains { get; set; }
    public DbSet<LineSchedule> LineSchedules { get; set; }
    public DbSet<ScheduleStop> ScheduleStops { get; set; }
    public DbSet<TrainAssignment> TrainAssignments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        // all DateTime mapped to timestamp without time zone
        foreach (var property in modelBuilder.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            property.SetColumnType("timestamp without time zone");

        modelBuilder.Entity<LineStation>().HasKey(ls => new { ls.LineID, ls.StationID });
        modelBuilder.Entity<TimeBetweenStations>().HasKey(t => new { t.FromStationID, t.ToStationID });
        modelBuilder.Entity<ScheduleStop>().HasKey(ss => new { ss.ScheduleID, ss.StationOrder });
        modelBuilder.Entity<TrainAssignment>().HasKey(ta => new { ta.TrainID, ta.ScheduleID, ta.AssignmentDate });

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

        // --- Seed Lines ---
        modelBuilder.Entity<Line>().HasData(
            new Line {
                LineID = "LN01", Name = "Sarkandaugava–Ziepniekkalns", Color = "#FF0000",
                IsClockwiseDirection = true,
                StartWorkTime = new DateTime(2000, 1, 1, 6, 0, 0), EndWorkTime = new DateTime(2000, 1, 1, 23, 0, 0)
            },
            new Line {
                LineID = "LN02", Name = "Imanta–Jugla", Color = "#00B050",
                IsClockwiseDirection = true,
                StartWorkTime = new DateTime(2000, 1, 1, 6, 0, 0), EndWorkTime = new DateTime(2000, 1, 1, 23, 0, 0)
            },
            new Line {
                LineID = "LN03", Name = "Dreilini–Buļļu kāpa", Color = "#0000FF",
                IsClockwiseDirection = true,
                StartWorkTime = new DateTime(2000, 1, 1, 6, 0, 0), EndWorkTime = new DateTime(2000, 1, 1, 23, 0, 0)
            }
        );

        // --- Seed Stations (each StationID only once) ---
        modelBuilder.Entity<Station>().HasData(
            // LN01
            new Station { StationID = "ST101", Name = "Sarkandaugava", Latitude = 57.003384, Longitude = 24.118737 },
            new Station { StationID = "ST102", Name = "Rupnica RER", Latitude = 56.991002, Longitude = 24.122138 },
            new Station { StationID = "ST103", Name = "Ramulu iela", Latitude = 56.974579, Longitude = 24.111671 },
            new Station { StationID = "ST104", Name = "Petersala", Latitude = 56.964006, Longitude = 24.106183 },
            new Station { StationID = "ST105", Name = "Kronvalda parks", Latitude = 56.956935, Longitude = 24.101257 },
            new Station { StationID = "ST106", Name = "Stacijas laukums", Latitude = 56.947561, Longitude = 24.119752 },
            new Station { StationID = "ST107", Name = "Zaķusala", Latitude = 56.933003, Longitude = 24.121713 },
            new Station { StationID = "ST108", Name = "Straume", Latitude = 56.919697, Longitude = 24.098176 },
            new Station { StationID = "ST109", Name = "Dzintars", Latitude = 56.912930, Longitude = 24.069526 },
            new Station { StationID = "ST110", Name = "Ziepniekkalns", Latitude = 56.898448, Longitude = 24.092073 },

            // LN02
            new Station { StationID = "ST201", Name = "Imanta", Latitude = 56.960271, Longitude = 24.014750 },
            new Station { StationID = "ST202", Name = "Zolitūde", Latitude = 56.945293, Longitude = 24.002022 },
            new Station { StationID = "ST203", Name = "Pleskodāle", Latitude = 56.930816, Longitude = 24.036110 },
            new Station { StationID = "ST204", Name = "Zasulauks", Latitude = 56.946689, Longitude = 24.048094 },
            new Station { StationID = "ST205", Name = "Āgenskalns", Latitude = 56.935529, Longitude = 24.072192 },
            new Station { StationID = "ST206", Name = "Uzvaras parks", Latitude = 56.937130, Longitude = 24.086747 },
            new Station { StationID = "ST208", Name = "Esplanāde", Latitude = 56.954588, Longitude = 24.118322 },
            new Station { StationID = "ST209", Name = "Vidzemes tirgus", Latitude = 56.959962, Longitude = 24.130017 },
            new Station { StationID = "ST210", Name = "Brasa", Latitude = 56.972436, Longitude = 24.141018 },
            new Station { StationID = "ST211", Name = "VEF", Latitude = 56.973676, Longitude = 24.166967 },
            new Station { StationID = "ST212", Name = "Teika", Latitude = 56.983054, Longitude = 24.200993 },
            new Station { StationID = "ST213", Name = "Alfa", Latitude = 56.987684, Longitude = 24.229665 },
            new Station { StationID = "ST214", Name = "Jugla", Latitude = 56.979464, Longitude = 24.253134 },

            // LN03
            new Station { StationID = "ST301", Name = "Dreilini", Latitude = 56.943385, Longitude = 24.254300 },
            new Station { StationID = "ST302", Name = "Plavnieki", Latitude = 56.938481, Longitude = 24.210546 },
            new Station { StationID = "ST303", Name = "Purvciems", Latitude = 56.957674, Longitude = 24.184348 },
            new Station { StationID = "ST304", Name = "Daugavas stadions", Latitude = 56.953286, Longitude = 24.156230 },
            new Station { StationID = "ST305", Name = "Matīsa Iela", Latitude = 56.955895, Longitude = 24.135721 },
            new Station { StationID = "ST306", Name = "Ķīpsala", Latitude = 56.949916, Longitude = 24.087104 },
            new Station { StationID = "ST307", Name = "Iļģuciems", Latitude = 56.966904, Longitude = 24.058006 },
            new Station { StationID = "ST308", Name = "Lačupe", Latitude = 56.975793, Longitude = 24.032716 },
            new Station { StationID = "ST309", Name = "Kleisti", Latitude = 56.984632, Longitude = 24.026594 },
            new Station { StationID = "ST310", Name = "Buļļu kāpa", Latitude = 57.001216, Longitude = 23.987065 }
        );

        // --- Seed LineStations for each line ---
// --- Seed LineStations for each line ---
        modelBuilder.Entity<LineStation>().HasData(
            // LN01
            new LineStation { LineID = "LN01", StationID = "ST101", StationOrder = 1 },
            new LineStation { LineID = "LN01", StationID = "ST102", StationOrder = 2 },
            new LineStation { LineID = "LN01", StationID = "ST103", StationOrder = 3 },
            new LineStation { LineID = "LN01", StationID = "ST104", StationOrder = 4 },
            new LineStation { LineID = "LN01", StationID = "ST105", StationOrder = 5 },
            new LineStation { LineID = "LN01", StationID = "ST106", StationOrder = 6 },
            new LineStation { LineID = "LN01", StationID = "ST107", StationOrder = 7 },
            new LineStation { LineID = "LN01", StationID = "ST108", StationOrder = 8 },
            new LineStation { LineID = "LN01", StationID = "ST109", StationOrder = 9 },
            new LineStation { LineID = "LN01", StationID = "ST110", StationOrder = 10 },

            // LN02
            new LineStation { LineID = "LN02", StationID = "ST201", StationOrder = 1 },
            new LineStation { LineID = "LN02", StationID = "ST202", StationOrder = 2 },
            new LineStation { LineID = "LN02", StationID = "ST203", StationOrder = 3 },
            new LineStation { LineID = "LN02", StationID = "ST204", StationOrder = 4 },
            new LineStation { LineID = "LN02", StationID = "ST205", StationOrder = 5 },
            new LineStation { LineID = "LN02", StationID = "ST206", StationOrder = 6 },
            new LineStation { LineID = "LN02", StationID = "ST106", StationOrder = 7 }, // Stacijas laukums
            new LineStation { LineID = "LN02", StationID = "ST208", StationOrder = 8 }, // Esplanāde
            new LineStation { LineID = "LN02", StationID = "ST209", StationOrder = 9 },
            new LineStation { LineID = "LN02", StationID = "ST210", StationOrder = 10 },
            new LineStation { LineID = "LN02", StationID = "ST211", StationOrder = 11 },
            new LineStation { LineID = "LN02", StationID = "ST212", StationOrder = 12 },
            new LineStation { LineID = "LN02", StationID = "ST213", StationOrder = 13 },
            new LineStation { LineID = "LN02", StationID = "ST214", StationOrder = 14 },

            // LN03
            new LineStation { LineID = "LN03", StationID = "ST301", StationOrder = 1 },
            new LineStation { LineID = "LN03", StationID = "ST302", StationOrder = 2 },
            new LineStation { LineID = "LN03", StationID = "ST303", StationOrder = 3 },
            new LineStation { LineID = "LN03", StationID = "ST304", StationOrder = 4 },
            new LineStation { LineID = "LN03", StationID = "ST305", StationOrder = 5 },
            new LineStation { LineID = "LN03", StationID = "ST208", StationOrder = 6 }, // Esplanāde
            new LineStation { LineID = "LN03", StationID = "ST105", StationOrder = 7 }, // Kronvalda parks
            new LineStation { LineID = "LN03", StationID = "ST306", StationOrder = 8 },
            new LineStation { LineID = "LN03", StationID = "ST307", StationOrder = 9 },
            new LineStation { LineID = "LN03", StationID = "ST308", StationOrder = 10 },
            new LineStation { LineID = "LN03", StationID = "ST309", StationOrder = 11 },
            new LineStation { LineID = "LN03", StationID = "ST310", StationOrder = 12 }
        );


        // --- Seed Trains for all lines ---
        modelBuilder.Entity<Train>().HasData(
            new Train {
                TrainID = "TR001", LineID = "LN01", TrainName = "TR–1", IsActive = true,
                StartWorkTime = new TimeSpan(8, 0, 0), EndWorkTime = new TimeSpan(20, 0, 0)
            },
            new Train {
                TrainID = "TR002", LineID = "LN01", TrainName = "TR–2", IsActive = true,
                StartWorkTime = new TimeSpan(7, 30, 0), EndWorkTime = new TimeSpan(19, 30, 0)
            },
            new Train {
                TrainID = "TR201", LineID = "LN02", TrainName = "TR–Green–1", IsActive = true,
                StartWorkTime = new TimeSpan(7, 0, 0), EndWorkTime = new TimeSpan(21, 0, 0)
            },
            new Train {
                TrainID = "TR202", LineID = "LN02", TrainName = "TR–Green–2", IsActive = true,
                StartWorkTime = new TimeSpan(7, 30, 0), EndWorkTime = new TimeSpan(21, 30, 0)
            },
            new Train {
                TrainID = "TR301", LineID = "LN03", TrainName = "TR–Blue–1", IsActive = true,
                StartWorkTime = new TimeSpan(6, 0, 0), EndWorkTime = new TimeSpan(22, 0, 0)
            },
            new Train {
                TrainID = "TR302", LineID = "LN03", TrainName = "TR–Blue–2", IsActive = true,
                StartWorkTime = new TimeSpan(6, 30, 0), EndWorkTime = new TimeSpan(22, 30, 0)
            }
        );
    }
}