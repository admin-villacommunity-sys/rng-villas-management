using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Filters;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Controllers
{
    public class ExpenditureController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpenditureController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? villaNo)
        {
            ViewData["Title"] = "Expenditure";
            ViewBag.VillaNo = villaNo;

            var list = _context.Expenditures
                               .OrderByDescending(x => x.Payment_date)
                               .ToList();

            return View(list);
        }

        public IActionResult Details(int id)
        {
            var expenditure = _context.Expenditures.Find(id);
            if (expenditure == null)
                return NotFound();
            return View(expenditure);
        }

        [AdminOnly]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Create(Expenditure expenditure)
        {
            if (!ModelState.IsValid)
                return View(expenditure);

            _context.Expenditures.Add(expenditure);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var expenditure = _context.Expenditures.Find(id);
            if (expenditure == null)
                return NotFound();
            return View(expenditure);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Edit(Expenditure expenditure)
        {
            if (!ModelState.IsValid)
                return View(expenditure);

            _context.Expenditures.Update(expenditure);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var expenditure = _context.Expenditures.Find(id);
            if (expenditure == null)
                return NotFound();

            _context.Expenditures.Remove(expenditure);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}