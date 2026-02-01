using ecommerce_shopping.Areas.Admin.Repository;
using ecommerce_shopping.Models;
using ecommerce_shopping.Repository;
using ecommerce_shopping.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// ... code khác ...
//Conection db
builder.Services.AddRazorPages();
builder.Services.AddDbContext<DataContext>(options =>
{
    var conn = builder.Configuration.GetConnectionString("ConnectedDb") ?? builder.Configuration["ConnectionStrings:ConnectedDb"];
    options.UseSqlServer(conn, sqlOptions =>
    {
        // increase command timeout to 60 seconds and enable simple retry policy for transient faults
        sqlOptions.CommandTimeout(60);
        sqlOptions.EnableRetryOnFailure();
    });

    // enable detailed EF logging only in Development to help diagnose long-running queries
    if (builder.Environment.IsDevelopment())
    {
        options.EnableSensitiveDataLogging();
        options.LogTo(Console.WriteLine);
    }
});
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(option =>
{
    option.IdleTimeout = TimeSpan.FromMinutes(30);
    option.Cookie.IsEssential = true;
});
builder.Services.AddIdentity<AppUserModel,IdentityRole>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings.
    options.User.AllowedUserNameCharacters =
    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
});
builder.Services.ConfigureApplicationCookie(options =>
{
    options.AccessDeniedPath = "/Home/AccessDenied";
});
builder.Services.AddScoped<IStatisticalService, StatisticalService>();
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    // Cho phép cookie được gửi qua redirect OAuth
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // production: yêu cầu HTTPS
})
.AddGoogle(googleOptions =>
{
    googleOptions.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    googleOptions.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;

    // Cấu hình cookie tương quan của handler
    googleOptions.CorrelationCookie.SameSite = SameSiteMode.None;
    googleOptions.CorrelationCookie.SecurePolicy = CookieSecurePolicy.Always;

    // Tùy chọn: nếu cần chỉ định callback path rõ ràng:
    // googleOptions.CallbackPath = "/Account/GoogleResponse";
});

// Tránh framework tự rewrite SameSite
builder.Services.Configure<CookiePolicyOptions>(options =>
{
    options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
    options.Secure = CookieSecurePolicy.Always;
    options.HttpOnly = HttpOnlyPolicy.Always;
});
builder.Services.AddHttpClient();

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statuscode={0}");
app.UseStaticFiles();
app.UseCookiePolicy();
app.UseRouting();

app.UseAuthentication();// dang nhap
app.UseAuthorization();// chec
app.UseSession(); //

app.MapControllerRoute(
    name: "Areas",
    pattern: "{area:exists}/{controller=Product}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "category",
    pattern: "/category/{Slug?}",
    defaults: new { controller = "Category", action = "Index" }
    );
app.MapControllerRoute(
    name: "brand",
    pattern: "/brand/{Slug?}",
    defaults: new { controller = "brand", action = "Index" }
    );
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Only run migrations/seeding in Development by default.
// To enable in production set "RunMigrationsOnStartup": true in appsettings (not recommended for general production).
var runMigrations = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("RunMigrationsOnStartup");
if (runMigrations)
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var conn = builder.Configuration.GetConnectionString("ConnectedDb") ?? builder.Configuration["ConnectionStrings:ConnectedDb"];

    try
    {
        // Quick connectivity test with a short connect timeout so failures are clearer than a long migration exception
        var csb = new SqlConnectionStringBuilder(conn) { ConnectTimeout = 15 };
        using (var testConn = new SqlConnection(csb.ConnectionString))
        {
            testConn.Open(); // throws quickly if network/credentials are wrong
            logger.LogInformation("Database connectivity test succeeded.");
        }

        // Run migrations and seed (keep this out of normal production deploys)
        context.Database.Migrate();
        SeedData.SeedingData(context);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration/seeding failed on startup. Inner: {Inner}", ex.InnerException?.Message ?? "(none)");
        if (app.Environment.IsDevelopment())
            throw;
        // In production, surface friendly message or fail fast via monitoring.
    }
}

app.Run();