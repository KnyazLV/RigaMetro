namespace RigaMetro.Web.Models.ViewModels.Admin;

public class AdminDataViewModel {
    public AdminStatisticsViewModel AdminStatistics { get; set; }
    public List<LineAdminSettingsViewModel> Lines { get; set; }
    public List<TrainViewModel> Trains { get; set; }
}
