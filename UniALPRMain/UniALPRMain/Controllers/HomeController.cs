using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class HomeController : Controller
    {
        private readonly VysitorDbContext _db;

        public HomeController(VysitorDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.Cameras = _db.Cameras.ToArray();

            return View();
        }
    }
}