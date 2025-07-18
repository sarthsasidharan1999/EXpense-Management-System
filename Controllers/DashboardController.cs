using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using test_project.Data;
using test_project.Models;
using System.Collections.Generic;

namespace test_project.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly AppDbContext _context;

        public DashboardController(AppDbContext context)
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
            if (userId == null)
                return RedirectToAction("Login", "User");

            // ✅ Fetch Income Data
            var incomes = _context.Incomes
                .Where(i => i.UserId == userId)
                .ToList();

            var incomeGroups = incomes
                .GroupBy(i => i.Source)
                .Select(g => new
                {
                    Label = g.Key,
                    Total = g.Sum(x => x.Amount)
                }).ToList();

            var totalIncome = incomes.Sum(i => i.Amount);

            // ✅ Fetch Expense Data (include Category to prevent nulls)
            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .Include(e => e.Category)
                .Where(e => e.Category != null)
                .ToList();

            var expenseGroups = expenses
                .GroupBy(e => e.Category.Name)
                .Select(g => new
                {
                    Label = g.Key,
                    Total = g.Sum(x => x.Amount)
                }).ToList();

            var totalExpense = expenses.Sum(e => e.Amount);

            // ✅ Fill ViewModel
            var model = new DashboardViewModel
            {
                IncomeLabels = incomeGroups.Select(g => g.Label).ToList(),
                IncomeData = incomeGroups.Select(g => g.Total).ToList(),
                ExpenseLabels = expenseGroups.Select(g => g.Label).ToList(),
                ExpenseData = expenseGroups.Select(g => g.Total).ToList(),
                TotalIncome = totalIncome,
                TotalExpense = totalExpense
            };

            return View(model);
        }
    }
}
