using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test_project.Data;
using test_project.Models;
using iText.Layout;


namespace test_project.Controllers
{
    public class ExpenseController : Controller
    {
        private readonly AppDbContext _context;

        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ Get user ID from ClaimsPrincipal (not session)
        private int? GetCurrentUserId()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdClaim?.Value, out int userId))
                {
                    return userId;
                }
            }
            return null;
        }

        // GET: Expense/Create
        public IActionResult Create()
        {
            ViewBag.Categories = _context.Categories.ToList(); // dropdown
            return View();
        }

        // POST: Expense/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Expense expense)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "User");

            // Removed ModelState validation
            expense.UserId = userId.Value;
            _context.Expenses.Add(expense);
            _context.SaveChanges();

            TempData["Message"] = "Expense added successfully.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Expense/Index
        public IActionResult Index(int? month)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "User");

            var expensesQuery = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId);

            if (month.HasValue)
            {
                expensesQuery = expensesQuery.Where(e => e.Date.Month == month);
            }

            var expenses = expensesQuery
                .OrderByDescending(e => e.Date)
                .ToList();

            // ✅ Use null-check to avoid NullReferenceException
            ViewBag.ChartLabels = expenses
                .Where(e => e.Category != null)
                .GroupBy(e => e.Category.Name)
                .Select(g => g.Key)
                .ToList();

            ViewBag.ChartData = expenses
                .Where(e => e.Category != null)
                .GroupBy(e => e.Category.Name)
                .Select(g => g.Sum(e => e.Amount))
                .ToList();

            return View(expenses);
        }


        // Optional: Export to PDF
        public IActionResult ExportToPdf()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
                return RedirectToAction("Login", "User");

            var expenses = _context.Expenses
                .Include(e => e.Category)
                .Where(e => e.UserId == userId)
                .OrderByDescending(e => e.Date)
                .ToList();

            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
                document.Add(new Paragraph("Expense Report")
                    .SetFont(font)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetMarginBottom(20));

                var table = new Table(3, false);
                table.AddHeaderCell("Date");
                table.AddHeaderCell("Category");
                table.AddHeaderCell("Amount");

                foreach (var expense in expenses)
                {
                    table.AddCell(expense.Date.ToString("dd/MM/yyyy"));
                    table.AddCell(expense.Category?.Name ?? "N/A");
                    table.AddCell(expense.Amount.ToString("C"));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "ExpenseReport.pdf");
            }
        }

    }
}
