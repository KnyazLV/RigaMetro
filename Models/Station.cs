using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RigaMetro.Models;

public class Station {
    [Key] [MaxLength(8)] 
    public string StationID { get; set; } = "";
    public string Name { get; set; } = "";
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    [InverseProperty(nameof(TimeBetweenStations.FromStation))]
    public ICollection<TimeBetweenStations> TimeFrom { get; set; } = new List<TimeBetweenStations>();

    [InverseProperty(nameof(TimeBetweenStations.ToStation))]
    public ICollection<TimeBetweenStations> TimeTo   { get; set; } = new List<TimeBetweenStations>();
}