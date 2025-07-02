using System.ComponentModel.DataAnnotations.Schema;

namespace RigaMetro.Models;

public class TimeBetweenStations {
    public int FromStationID { get; set; }
    public int ToStationID { get; set; }
    public int TimeSeconds { get; set; }
    public int DistanceM { get; set; }

    [ForeignKey(nameof(FromStationID))]
    [InverseProperty(nameof(Station.TimeFrom))]
    public Station FromStation { get; set; }

    [ForeignKey(nameof(ToStationID))]
    [InverseProperty(nameof(Station.TimeTo))]
    public Station ToStation   { get; set; }
}