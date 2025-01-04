using Microsoft.AspNetCore.Mvc;

namespace WanviBE.API.Controllers
{
    public class UserController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
