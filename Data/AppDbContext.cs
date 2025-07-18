using Microsoft.EntityFrameworkCore;
using test_project.Models;

namespace test_project.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Demo_user> DemoUsers { get; set; }
        public DbSet<Expense> Expenses { get; set; }
        public DbSet<Income> Incomes { get; set; }
        public DbSet<Category> Categories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ✅ Table name mapping (optional for clarity)
            modelBuilder.Entity<Expense>().ToTable("Expenses");
            modelBuilder.Entity<Income>().ToTable("Incomes");
            modelBuilder.Entity<Demo_user>().ToTable("Demo_user");
            modelBuilder.Entity<Category>().ToTable("Category");

            // ✅ Decimal precision to avoid truncation issues
            modelBuilder.Entity<Expense>()
                .Property(e => e.Amount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Income>()
                .Property(i => i.Amount)
                .HasPrecision(18, 2);

            // ✅ Seed default categories
            modelBuilder.Entity<Category>().HasData(
                new Category { Id = 1, Name = "Food" },
                new Category { Id = 2, Name = "Rent" },
                new Category { Id = 3, Name = "Utilities" },
                new Category { Id = 4, Name = "Travel" },
                new Category { Id = 5, Name = "Health" }
            );
        }
    }
}
