using Microsoft.AspNetCore.Mvc;
using UniALPRMain.Models;

namespace UniALPRMain.Controllers
{
    public class LoginController : Controller
    {
        private static Random random = new Random();


        private readonly VysitorDbContext _db;

        public LoginController(VysitorDbContext db)
        {
            _db = db;
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(string login, string password)
        {
            if(login == "root" && password == "root")
            {
                string cookie = RandomString(40);

                _db.Cookies.Add(new Cookie() 
                {
                    Value = cookie
                });
                _db.SaveChanges();

                Response.Cookies.Append("auth", cookie);

                return Redirect("/");
            }

            return Redirect("/Login");
        }

        static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
