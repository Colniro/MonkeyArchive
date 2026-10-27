using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MonkeyArchive.Models;
using MySqlConnector;

namespace MonkeyArchive.Controllers
{
    public class HomeController : Controller
    {
        private readonly string _connString;

        public HomeController(IConfiguration config)
        {
            _connString = config.GetConnectionString("Default")!;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Towers()
        {
            return View();
        }

        // Testseite für die Datenbankverbindung
        public IActionResult DbTest()
        {
            var namen = new List<string>();

            using var conn = new MySqlConnection(_connString);
            conn.Open();

            using var cmd = new MySqlCommand("SELECT Name FROM Tower", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                namen.Add(reader.GetString(0));
            }

            return View(namen);
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