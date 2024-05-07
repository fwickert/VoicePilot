using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VoicePilot_Web.Models;

namespace VoicePilot_Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult GetActionReco(string input)
        {
            input = "Please go to the Zone5";
            this._logger.LogDebug("Action reco receive request.");
            return ViewComponent("ActionRecoComponent", new { input = input });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
