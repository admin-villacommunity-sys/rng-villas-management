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
builder.Configuration.AddEnvironmentVariables();

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
    // Use the raw connection string as-is (URI format)
    connectionString = rawConnectionString;
    // Mask password for logging
    var masked = System.Text.RegularExpressions.Regex.Replace(connectionString, @"password=[^&]*", "password=***");
    Console.WriteLine($"--- Using raw connection string (URI): {masked} ---");
}
else
{
    // Not a URI, use as-is (likely key-value)
    connectionString = rawConnectionString;
    var masked = System.Text.RegularExpressions.Regex.Replace(connectionString, @"Password=[^;]*", "Password=***", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    Console.WriteLine($"--- Using raw connection string (key-value): {masked} ---");
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

// Configure Email Settings
builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

// Register Brevo email service
builder.Services.AddScoped<BrevoEmailService>();

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
// Migrate database and create admin if missing
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    try
    {
        Console.WriteLine("--- Starting database migration ---");
        dbContext.Database.Migrate();
        Console.WriteLine("--- Migration completed successfully ---");

        // Check if any admin exists
        if (!dbContext.AdminLogins.Any())
        {
            Console.WriteLine("--- No admin found. Creating default admin... ---");
            var admin = new AdminLogin
            {
                Username = "admin",
                Password = BCrypt.Net.BCrypt.HashPassword("admin123"),
                Email = "admin.villacommunity@gmail.com",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.AdminLogins.Add(admin);
            dbContext.SaveChanges();
            Console.WriteLine("--- Default admin created (username: admin, password: admin123) ---");
        }
        else
        {
            Console.WriteLine($"--- Admin(s) already exist: {dbContext.AdminLogins.Count()} found ---");
        }
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