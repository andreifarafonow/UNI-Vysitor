using UniALPRMain.Models;

namespace UniALPRMain.Middlewares
{
    public class AuthMiddleware : IMiddleware
    {
        private readonly VysitorDbContext _db;

        public AuthMiddleware(VysitorDbContext db)
        {
            _db = db;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var path = context.Request.Path;

            if ((context.Request.Cookies["auth"] == null || !_db.Cookies.Any(x => x.Value == context.Request.Cookies["auth"])) && path != "/Login")
            {
                var remoteIpAddress = context.Request.HttpContext.Connection.RemoteIpAddress;

                _db.UnauthorizedRequests.Add(new UnauthorizedRequest() 
                {
                     Browser = context.Request.Headers.UserAgent,
                     Ip = remoteIpAddress.ToString()
                });

                _db.SaveChanges();

                context.Response.Redirect("/Login");
            }

            await next(context);
        }
    }
}
