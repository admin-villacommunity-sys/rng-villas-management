using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Services;

namespace VillaCommunityManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BrevoEmailService _emailService;

        public ReportsController(ApplicationDbContext context, BrevoEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public IActionResult Index()
        {
            ViewBag.TotalIncome = _context.Incomes.Sum(x => x.Amount);
            ViewBag.TotalExpense = _context.Expenditures.Sum(x => x.Amount);
            ViewBag.Balance = ViewBag.TotalIncome - ViewBag.TotalExpense;
            ViewBag.TotalOwners = _context.Owners.Count();
            ViewBag.TotalVillas = _context.Owners.Count();
            ViewBag.Pending = _context.Maintenances.Count(x => !x.Payment_details);

            return View();
        }

        public async Task<IActionResult> TestEmail()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "admin.villacommunity@gmail.com",
                    "Test Email from RNG Supra Villas",
                    @"
            <h2>Email Service Working ✅</h2>
            <p>This email confirms that the Brevo API configuration is correct.</p>
            <p><strong>RNG Supra Villas Management System</strong></p>
            ");

                return Content("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }
    }
}