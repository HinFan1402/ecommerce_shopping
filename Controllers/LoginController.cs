using Microsoft.AspNetCore.Mvc;

namespace ecommerce_shopping.Controllers
{
    public class LoginController: Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
