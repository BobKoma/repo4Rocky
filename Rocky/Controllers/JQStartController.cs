using Microsoft.AspNetCore.Mvc;

namespace Rocky.Controllers
{
    public class JQStartController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
