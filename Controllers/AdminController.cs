using Microsoft.AspNetCore.Mvc;

namespace RigaMetro.Controllers;

public class AdminController : Controller {
    public IActionResult Index() {
        return View();
    }
}