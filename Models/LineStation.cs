namespace RigaMetro.Models;

public class LineStation {
    public int LineID { get; set; }
    public int StationID { get; set; }
    public int StationOrder { get; set; }

    public Line? Line { get; set; }
    public Station? Station { get; set; }
}