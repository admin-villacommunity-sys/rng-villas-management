using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Services;

namespace VillaCommunityManagement.Controllers
{
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.TotalIncome = _context.Incomes.Sum(x => x.Amount);

            ViewBag.TotalExpense = _context.Expenditures.Sum(x => x.Amount);

            ViewBag.Balance =
                ViewBag.TotalIncome - ViewBag.TotalExpense;

            ViewBag.TotalOwners =
                _context.Owners.Count();

            ViewBag.TotalVillas =
                _context.Owners.Count();

            ViewBag.Pending =
                _context.Maintenances.Count(x => !x.Payment_details);

            return View();
        }

        public async Task<IActionResult> TestEmail(
    [FromServices] EmailService emailService)
        {
            try
            {
                await emailService.SendEmailAsync(
                    "admin.villacommunity@gmail.com",
                    "Test Email from RNG Supra Villas",
                    @"
            <h2>Email Service Working ✅</h2>
            <p>This email confirms that the SMTP configuration is correct.</p>
            <p><strong>RNG Supra Villas Management System</strong></p>
            ");

                return Content("Email sent successfully!");
            }
            catch (Exception ex)
            {
                return Content(ex.Message);
            }
        }
    }
}