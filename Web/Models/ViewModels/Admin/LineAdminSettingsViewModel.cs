namespace RigaMetro.Web.Models.ViewModels.Admin;

public class LineAdminSettingsViewModel {
    public string LineID { get; set; } = "";
    public string Name { get; set; } = "";
    public DateTime StartWorkTime { get; set; }
    public DateTime EndWorkTime { get; set; }
    public string Color { get; set; } = "";
}