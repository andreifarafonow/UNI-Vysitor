using Microsoft.AspNetCore.Mvc;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class EventsController : Controller
    {
        private readonly VysitorDbContext _db;

        public EventsController(VysitorDbContext db)
        {
            _db = db;
        }

        public IActionResult Index()
        {
            ViewBag.Events = _db.Events.OrderByDescending(x => x.Time).ToArray();

            return View();
        }
    }
}
