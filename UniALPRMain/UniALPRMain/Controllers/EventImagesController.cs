using Microsoft.AspNetCore.Mvc;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class EventImagesController : Controller
    {
        private readonly VysitorDbContext _db;

        public EventImagesController(VysitorDbContext db)
        {
            _db = db;
        }

        public IActionResult Index(int id)
        {
            return File(Convert.FromBase64String(_db.Events.FirstOrDefault(x => x.Id == id).PictureUrl), "image/jpeg");
        }
    }
}
