using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            ViewBag.Username = HttpContext.Session.GetString("Username");

            ViewBag.TotalVillas = 59;

            ViewBag.TotalOwners = _context.Owners.Count();

            ViewBag.Pending =
                _context.Maintenances.Count(x => !x.Payment_details);

            ViewBag.TotalIncome =
                _context.Incomes.Any()
                    ? _context.Incomes.Sum(x => x.Amount)
                    : 0;

            ViewBag.TotalExpense =
                _context.Expenditures.Any()
                    ? _context.Expenditures.Sum(x => x.Amount)
                    : 0;

            ViewBag.Balance =
                ViewBag.TotalIncome - ViewBag.TotalExpense;

            ViewBag.RecentIncome =
                _context.Incomes
                    .OrderByDescending(x => x.month)
                    .Take(5)
                    .ToList();

            ViewBag.RecentExpenses =
                _context.Expenditures
                    .OrderByDescending(x => x.Payment_date)
                    .Take(5)
                    .ToList();

            ViewBag.RecentMaintenance =
                _context.Maintenances
                    .OrderByDescending(x => x.payment_date)
                    .Take(5)
                    .ToList();

            var owners = _context.Owners
                                 .OrderBy(x => x.Villa_No)
                                 .ToList();

            return View(owners);
        }
    }
}