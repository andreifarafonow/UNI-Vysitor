using Microsoft.AspNetCore.Mvc;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class UnRequestController : Controller
    {
        private readonly VysitorDbContext _db;

        public UnRequestController(VysitorDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View(_db.UnauthorizedRequests.OrderByDescending(x => x.Id).ToArray());
        }
    }
}
