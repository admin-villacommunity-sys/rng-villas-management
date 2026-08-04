using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Models;
using VillaCommunityManagement.Services;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);
// At the top of Program.cs, after builder is created
builder.Configuration.AddEnvironmentVariables();

// ==========================================
// Load User Secrets in Development
// ==========================================
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
}

// ==========================================
// Add services
// ==========================================
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
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

var app = builder.Build();

// ==========================================
// Middleware Pipeline
// ==========================================

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