using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;
using System.Diagnostics;

namespace MonkeyArchive.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Towers()
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


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
