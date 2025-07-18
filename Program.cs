using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using test_project.Data;
using System.Globalization;

var cultureInfo = new CultureInfo("en-GB"); // For dd/MM/yyyy
cultureInfo.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
CultureInfo.DefaultThreadCurrentCulture = cultureInfo;
CultureInfo.DefaultThreadCurrentUICulture = cultureInfo;


var builder = WebApplication.CreateBuilder(args);

// Add MVC and Razor
builder.Services.AddControllersWithViews();

// Add EF Core with SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Session support
builder.Services.AddHttpContextAccessor();
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// ✅ Authentication - Cookie-based
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/User/Login"; // Redirect here if not authenticated
        options.AccessDeniedPath = "/User/Login";
        options.Cookie.Name = "ExpenseTrackerAuth";
        options.ExpireTimeSpan = TimeSpan.FromDays(7); // Optional
        options.SlidingExpiration = true;
    });

var app = builder.Build();

// Exception handler
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Middleware order matters
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// ✅ Authentication & Session
app.UseAuthentication();
app.UseSession();
app.UseAuthorization();

// Optional: fake login for dev
if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        if (!context.User.Identity.IsAuthenticated)
        {
            var claims = new[] {
                new Claim(ClaimTypes.Name, "testuser"),
                new Claim(ClaimTypes.NameIdentifier, "1")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
        }

        await next();
    });
}

// Routes
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Login}/{id?}");

app.Run();
