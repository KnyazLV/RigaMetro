using Microsoft.AspNetCore.Mvc;

namespace RigaMetro.Web.Controllers;

public class AdminController : Controller {
    public IActionResult Index() {
        return View();
    }
}