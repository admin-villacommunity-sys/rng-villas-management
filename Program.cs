using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Models;
using VillaCommunityManagement.Services;
using AspNetCoreRateLimit;

// ==========================================
// FORCE POLLING FILE WATCHER (Fixes inotify limit)
// ==========================================
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Load User Secrets in Development
// ==========================================
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ==========================================
// DISABLE FILE WATCHING (additional safety)
// ==========================================
builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false);

// ==========================================
// Add services
// ==========================================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSession();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddScoped<EmailService>();

// ==========================================
// Rate Limiting
// ==========================================
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(
    builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// ==========================================
// Build and configure the app
// ==========================================
var app = builder.Build();

// Error handling (dev vs production)
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Rate Limiting
app.UseIpRateLimiting();

app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Login}/{action=Index}/{id?}");

app.Run();