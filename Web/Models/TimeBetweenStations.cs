using System.ComponentModel.DataAnnotations.Schema;

namespace RigaMetro.Web.Models;

public class TimeBetweenStations {
    public string FromStationID { get; set; }
    public string ToStationID { get; set; }
    public int TimeSeconds { get; set; }
    public int DistanceM { get; set; }

    [ForeignKey(nameof(FromStationID))]
    [InverseProperty(nameof(Station.TimeFrom))]
    public Station FromStation { get; set; }

    [ForeignKey(nameof(ToStationID))]
    [InverseProperty(nameof(Station.TimeTo))]
    public Station ToStation   { get; set; }
}