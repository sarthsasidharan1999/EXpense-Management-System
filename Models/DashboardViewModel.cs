using System.Collections.Generic;

namespace test_project.Models
{
    public class DashboardViewModel
    {
        public List<string> IncomeLabels { get; set; }
        public List<decimal> IncomeData { get; set; }

        public List<string> ExpenseLabels { get; set; }
        public List<decimal> ExpenseData { get; set; }

        // For pie chart and total summary
        public decimal TotalIncome { get; set; }
        public decimal TotalExpense { get; set; }
    }
}
