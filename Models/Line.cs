namespace RigaMetro.Models;

public class Line {
    public int LineID { get; set; }
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
    public bool IsClockwiseDirection { get; set; }
    public DateTime StartWorkTime { get; set; }
    public DateTime EndWorkTime { get; set; }

    // Навигационные свойства
    public ICollection<LineStation>? LineStations { get; set; }
    public ICollection<Train>? Trains { get; set; }
    public ICollection<LineSchedule>? LineSchedules { get; set; }
}