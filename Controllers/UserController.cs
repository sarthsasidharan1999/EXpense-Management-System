using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test_project.Data;
using test_project.Models;
using System.Linq;
using System.Threading.Tasks;

namespace test_project.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register([Bind("Username,Email,Password")] Demo_user user)
        {
            if (!ModelState.IsValid) return View(user);

            if (_context.DemoUsers.Any(u => u.Username == user.Username || u.Email == user.Email))
            {
                ModelState.AddModelError("", "Username or Email already exists.");
                return View(user);
            }

            _context.DemoUsers.Add(user);
            await _context.SaveChangesAsync();

            TempData["Message"] = "Registration successful. Please log in.";
            return RedirectToAction("Login");
        }

        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var user = _context.DemoUsers
                .FirstOrDefault(u => u.Username == username && u.Password == password); // ❗ Hash comparison in production

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid credentials.");
                return View();
            }

            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            TempData["Message"] = $"Welcome, {username}!";
            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }

}
