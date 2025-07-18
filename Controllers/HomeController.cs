using Microsoft.AspNetCore.Mvc;

namespace test_project.Controllers
{
    public class HomeController : Controller
    {
        // GET: /Home/Index or just /
        public IActionResult Index()
        {
            return View();
        }

        // Optional: Privacy page example
        public IActionResult Privacy()
        {
            return View();
        }

        // Optional: Error page example
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(); // You can create Error.cshtml to show error details
        }
    }
}
