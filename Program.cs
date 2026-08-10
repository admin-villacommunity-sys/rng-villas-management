using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Models;
using VillaCommunityManagement.Services;
using AspNetCoreRateLimit;
using Npgsql;

// ==========================================
// FORCE POLLING FILE WATCHER (Fixes inotify limit)
// ==========================================
Environment.SetEnvironmentVariable("DOTNET_USE_POLLING_FILE_WATCHER", "true");

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Read and parse the connection string
// ==========================================
string? rawConnectionString = builder.Configuration.GetConnectionString("DefaultConnection")?.Trim();

if (string.IsNullOrEmpty(rawConnectionString))
{
    Console.WriteLine("--- Connection String: NULL or EMPTY after trim ---");
    throw new Exception("Connection string is empty");
}

Console.WriteLine($"--- Raw connection string length: {rawConnectionString.Length} ---");

string connectionString;

// Try to parse as URI
if (Uri.TryCreate(rawConnectionString, UriKind.Absolute, out Uri? uri))
{
    // Extract components
    var userInfo = uri.UserInfo.Split(':');
    var username = userInfo.Length > 0 ? userInfo[0] : "";
    var password = userInfo.Length > 1 ? userInfo[1] : "";
    var host = uri.Host;
    var port = uri.Port; // could be -1
    var database = uri.LocalPath.TrimStart('/');

    // If port is -1, default to 5432
    if (port == -1) port = 5432;

    // Build Npgsql connection string
    var csBuilder = new NpgsqlConnectionStringBuilder
    {
        Host = host,
        Port = port,
        Username = username,
        Password = password,
        Database = database,
        SslMode = SslMode.Require
        // TrustServerCertificate is obsolete and not needed
    };
    connectionString = csBuilder.ConnectionString;

    // Log without exposing password
    var masked = connectionString.Replace(password, "***", StringComparison.OrdinalIgnoreCase);
    Console.WriteLine($"--- Converted connection string: {masked} ---");
}
else
{
    // Fallback: use raw string as-is
    connectionString = rawConnectionString;
    Console.WriteLine("--- Using raw connection string (not URI) ---");
    var masked = System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]*", "Password=***", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    Console.WriteLine($"--- Connection string: {masked} ---");
}

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
    options.UseNpgsql(connectionString));

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

// ==========================================
// Migrate database with detailed logging
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("--- Starting database migration ---");
        dbContext.Database.Migrate();
        Console.WriteLine("--- Migration completed successfully ---");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"--- Migration failed: {ex.Message} ---");
        Console.WriteLine($"--- Inner exception: {ex.InnerException?.Message} ---");
        throw;
    }
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