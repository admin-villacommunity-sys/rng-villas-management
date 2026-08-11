using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VillaCommunityManagement.Data;

namespace VillaCommunityManagement.Controllers
{
    public class VillaController : Controller
    {
        private readonly ApplicationDbContext _context;

        public VillaController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var villas = _context.Owners
                                 .OrderBy(x => x.Villa_No)
                                 .ToList();

            return View(villas);
        }

        public IActionResult Details(int id)
        {
            var owner = _context.Owners
                .FirstOrDefault(x => x.Villa_No == id);

            if (owner == null)
            {
                return View("VillaNotFound");
            }

            // Get all maintenance records for this villa
            var maintenanceRecords = _context.Maintenances
                .Where(x => x.Villa_No == id)
                .OrderByDescending(x => x.Due)
                .ToList();

            // Calculate summary — FIXED: use ?? 0 to handle null
            var totalPaid = maintenanceRecords
                .Where(x => x.Payment_details)
                .Sum(x => x.paid) ?? 0;

            var pendingCount = maintenanceRecords
                .Count(x => !x.Payment_details);

            var lastPayment = maintenanceRecords
                .Where(x => x.Payment_details)
                .OrderByDescending(x => x.payment_date)
                .FirstOrDefault();

            // Pass data to view
            ViewBag.MaintenanceRecords = maintenanceRecords;
            ViewBag.TotalPaid = totalPaid;
            ViewBag.PendingCount = pendingCount;
            ViewBag.LastPaymentDate = lastPayment?.payment_date;

            return View(owner);
        }
    }
}