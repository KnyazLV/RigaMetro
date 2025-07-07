using Microsoft.AspNetCore.Mvc;
using RigaMetro.Web.Models.ViewModels;

namespace RigaMetro.Web.ViewComponents;

public class StationScheduleViewComponent : ViewComponent {
    public IViewComponentResult Invoke(StationViewModel station, Dictionary<string, LineInfo> lineInfo) {
        var model = new StationScheduleComponentViewModel {
            Station = station,
            LineInfo = lineInfo
        };
        return View(model);
    }
}

public class StationScheduleComponentViewModel {
    public StationViewModel Station { get; set; } = new();
    public Dictionary<string, LineInfo> LineInfo { get; set; } = new();
}

public class LineInfo {
    public string Name { get; set; } = "";
    public string Color { get; set; } = "";
}