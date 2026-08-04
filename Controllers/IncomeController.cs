using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Filters;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Controllers
{
    public class IncomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public IncomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index(int? villaNo)
        {
            ViewData["Title"] = "Income";
            ViewBag.VillaNo = villaNo;

            var list = _context.Incomes
                               .OrderBy(i => i.month)
                               .ToList();

            return View(list);
        }

        public IActionResult Details(int id)
        {
            var income = _context.Incomes.Find(id);
            if (income == null)
                return NotFound();
            return View(income);
        }

        [AdminOnly]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Create(Income income)
        {
            if (!ModelState.IsValid)
                return View(income);

            _context.Incomes.Add(income);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var income = _context.Incomes.Find(id);
            if (income == null)
                return NotFound();
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Edit(Income income)
        {
            if (!ModelState.IsValid)
                return View(income);

            _context.Incomes.Update(income);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var income = _context.Incomes.Find(id);
            if (income == null)
                return NotFound();

            _context.Incomes.Remove(income);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}