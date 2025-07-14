using System.ComponentModel.DataAnnotations;

namespace RigaMetro.Web.Models;

public class Line {
    [Key]
    [MaxLength(8)]
    public string LineID { get; set; } = "";
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public bool IsClockwiseDirection { get; set; }
    public TimeSpan StartWorkTime { get; set; }
    public TimeSpan EndWorkTime { get; set; }

    // Навигационные свойства
    public ICollection<LineStation>? LineStations { get; set; }
    public ICollection<Train>? Trains { get; set; }
    public ICollection<LineSchedule>? LineSchedules { get; set; }
}