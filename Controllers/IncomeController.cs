using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using test_project.Data;
using test_project.Models;
using System.Collections.Generic;

namespace test_project.Controllers
{
    [Authorize]
    public class IncomeController : Controller
    {
        private readonly AppDbContext _context;

        public IncomeController(AppDbContext context)
        {
            _context = context;
        }

        private int? GetCurrentUserId()
        {
            var username = User.Identity?.Name;
            return _context.DemoUsers.SingleOrDefault(u => u.Username == username)?.Id;
        }

        public IActionResult Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "User");

            var incomes = _context.Incomes
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.Date)
                .ToList();

            // Group for chart
            var grouped = incomes
                .GroupBy(i => i.Source)
                .Select(g => new
                {
                    Source = g.Key,
                    Total = g.Sum(x => x.Amount)
                }).ToList();

            ViewBag.ChartLabels = grouped.Select(g => g.Source).ToList();
            ViewBag.ChartData = grouped.Select(g => g.Total).ToList();

            return View(incomes);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Income income)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return RedirectToAction("Login", "User");

            // Debug log
            Console.WriteLine($"Income received: {income.Source}, {income.Amount}, {income.Date}");

            income.UserId = userId.Value;
            _context.Incomes.Add(income);
            _context.SaveChanges();

            TempData["Success"] = "Income added successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
