namespace RigaMetro.Web.Models.ViewModels.Admin;

public class LineAdminSettingsViewModel {
    public string LineID { get; set; } = "";
    public string Name { get; set; } = "";
    public TimeSpan StartWorkTime { get; set; }
    public TimeSpan EndWorkTime { get; set; }
    public string Color { get; set; } = "";
}