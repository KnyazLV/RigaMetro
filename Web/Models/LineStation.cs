namespace RigaMetro.Web.Models;

public class LineStation {
    public string LineID { get; set; } = "";
    public string StationID { get; set; } = "";
    public int StationOrder { get; set; }

    public Line? Line { get; set; }
    public Station? Station { get; set; }
}