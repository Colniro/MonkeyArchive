using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;

namespace MonkeyArchive.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        // Geheime Seite, verlinkt nirgends im Menü. Nur zum Spass, kein
        // richtiger Tower.
        [Route("/elprimo")]
        public IActionResult ElPrimo()
        {
            return View();
        }

        // Wird von UseStatusCodePagesWithReExecute in Program.cs aufgerufen.
        // Heisst absichtlich nicht NotFound: ControllerBase hat schon eine
        // Methode mit dem Namen, die wuerde sonst verdeckt.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult PageNotFound()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}