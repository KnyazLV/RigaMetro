using Microsoft.AspNetCore.Mvc;

namespace RigaMetro.Controllers;

public class HomeController : Controller {
    private readonly ILogger<HomeController> _logger;
    private readonly IConfiguration _configuration;
    
    public HomeController(ILogger<HomeController> logger,  IConfiguration configuration) {
        _logger = logger;
        _configuration = configuration;
    }

    public IActionResult Index() {
        var token = _configuration["MapBox:ApiKey"];
        ViewData["MapboxToken"] = token;
        return View();
    }
    
    public IActionResult History() {
        return View();
    }
}