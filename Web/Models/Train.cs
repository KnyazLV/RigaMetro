namespace RigaMetro.Web.Models;

public class Train {
    public string TrainID { get; set; }
    public string LineID { get; set; }
    public string? TrainName { get; set; }
    public TimeSpan StartWorkTime { get; set; }
    public TimeSpan EndWorkTime { get; set; }
    public bool IsActive { get; set; }

    public Line? Line { get; set; }
    public ICollection<TrainAssignment>? Assignments { get; set; }
}