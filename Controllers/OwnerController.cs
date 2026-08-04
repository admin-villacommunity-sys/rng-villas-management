using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Filters;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Controllers
{
    public class OwnerController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OwnerController(ApplicationDbContext context)
        {
            _context = context;
        }

        // LIST
        public IActionResult Index()
        {
            ViewData["Title"] = "Residents";

            var owners = _context.Owners
                                 .OrderBy(o => o.Villa_No)
                                 .ToList();

            return View(owners);
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var owner = _context.Owners.Find(id);

            if (owner == null)
                return NotFound();

            return View(owner);
        }

        // CREATE
        [AdminOnly]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Create(Owner owner)
        {
            if (!ModelState.IsValid)
                return View(owner);

            _context.Owners.Add(owner);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var owner = _context.Owners.Find(id);

            if (owner == null)
                return NotFound();

            return View(owner);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Edit(Owner owner)
        {
            if (!ModelState.IsValid)
                return View(owner);

            _context.Owners.Update(owner);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var owner = _context.Owners.Find(id);

            if (owner == null)
                return NotFound();

            _context.Owners.Remove(owner);

            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }
    }
}