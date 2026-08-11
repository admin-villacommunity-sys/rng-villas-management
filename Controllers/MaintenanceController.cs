using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Filters;
using VillaCommunityManagement.Models;
using VillaCommunityManagement.Services;

namespace VillaCommunityManagement.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly BrevoEmailService _emailService;

        public MaintenanceController(
            ApplicationDbContext context,
            BrevoEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // LIST
        public IActionResult Index()
        {
            ViewData["Title"] = "Maintenance";

            var list = _context.Maintenances
                               .OrderBy(x => x.Villa_No)
                               .ThenBy(x => x.Due)
                               .ToList();

            return View(list);
        }

        // DETAILS
        public IActionResult Details(int id)
        {
            var maintenance = _context.Maintenances
                                      .FirstOrDefault(x => x.MaintenanceId == id);

            if (maintenance == null)
                return NotFound();

            return View(maintenance);
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
        public IActionResult Create(Maintenance maintenance)
        {
            if (!ModelState.IsValid)
                return View(maintenance);

            // Convert to UTC before saving
            maintenance.Due = DateTime.SpecifyKind(maintenance.Due, DateTimeKind.Utc);
            if (maintenance.payment_date.HasValue)
                maintenance.payment_date = DateTime.SpecifyKind(maintenance.payment_date.Value, DateTimeKind.Utc);

            _context.Maintenances.Add(maintenance);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // EDIT
        [AdminOnly]
        public IActionResult Edit(int id)
        {
            var maintenance = _context.Maintenances
                                      .FirstOrDefault(x => x.MaintenanceId == id);

            if (maintenance == null)
                return NotFound();

            return View(maintenance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOnly]
        public IActionResult Edit(Maintenance maintenance)
        {
            if (!ModelState.IsValid)
                return View(maintenance);

            // Convert to UTC before saving
            maintenance.Due = DateTime.SpecifyKind(maintenance.Due, DateTimeKind.Utc);
            if (maintenance.payment_date.HasValue)
                maintenance.payment_date = DateTime.SpecifyKind(maintenance.payment_date.Value, DateTimeKind.Utc);

            // If Payment_details is false, clear paid and payment_date
            if (!maintenance.Payment_details)
            {
                maintenance.paid = null;
                maintenance.payment_date = null;
            }

            _context.Maintenances.Update(maintenance);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // DELETE
        [AdminOnly]
        public IActionResult Delete(int id)
        {
            var maintenance = _context.Maintenances
                                      .FirstOrDefault(x => x.MaintenanceId == id);

            if (maintenance == null)
                return NotFound();

            _context.Maintenances.Remove(maintenance);
            _context.SaveChanges();

            return RedirectToAction(nameof(Index));
        }

        // SEND REMINDER (for a specific maintenance record)
        [AdminOnly]
        public async Task<IActionResult> SendReminder(int id)
        {
            var maintenance = _context.Maintenances
                                      .FirstOrDefault(x => x.MaintenanceId == id);

            if (maintenance == null)
                return NotFound();

            // Get ALL pending records for this villa
            var pendingRecords = _context.Maintenances
                .Where(x => x.Villa_No == maintenance.Villa_No && !x.Payment_details)
                .OrderBy(x => x.Due)
                .ToList();

            if (!pendingRecords.Any())
            {
                TempData["Error"] = "No pending maintenance records found for this villa.";
                return RedirectToAction(nameof(Index));
            }

            var owner = _context.Owners
                                .FirstOrDefault(x => x.Villa_No == maintenance.Villa_No);

            if (owner == null || string.IsNullOrWhiteSpace(owner.Email))
            {
                TempData["Error"] = "Owner email not found.";
                return RedirectToAction(nameof(Index));
            }

            string subject = "Maintenance Payment Reminder - RNG Supra Villas";

            var tableRows = string.Empty;
            foreach (var record in pendingRecords)
            {
                string dueDisplay = $"₹ {record.DueAmount:N2}";
                string paidDisplay = record.paid.HasValue ? $"₹ {record.paid.Value:N2}" : "₹ 0.00";
                tableRows += $@"
<tr>
    <td style='padding:10px;border:1px solid #ddd;'>{record.Month}</td>
    <td style='padding:10px;border:1px solid #ddd;'>{record.Due:dd-MMM-yyyy}</td>
    <td style='padding:10px;border:1px solid #ddd;'>{dueDisplay}</td>
    <td style='padding:10px;border:1px solid #ddd;'>{paidDisplay}</td>
    <td style='padding:10px;border:1px solid #ddd;color:red;'>Pending</td>
</tr>";
            }

            string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f5;padding:20px;'>

<div style='max-width:650px;background:white;padding:30px;border-radius:8px;border:1px solid #ddd;'>

<h2 style='color:#1f4e79;'>RNG Supra Villas Management</h2>

<hr/>

<p>Dear <strong>{owner.Owner_name}</strong>,</p>

<p>This is a friendly reminder that the following maintenance payments are still pending for <strong>Villa {maintenance.Villa_No}</strong>.</p>

<table style='border-collapse:collapse;width:100%;margin-top:20px;'>

<tr style='background:#f0f0f0;'>
    <th style='padding:10px;border:1px solid #ddd;'>Month</th>
    <th style='padding:10px;border:1px solid #ddd;'>Due Date</th>
    <th style='padding:10px;border:1px solid #ddd;'>Total Due</th>
    <th style='padding:10px;border:1px solid #ddd;'>Paid</th>
    <th style='padding:10px;border:1px solid #ddd;'>Status</th>
</tr>
{tableRows}
</table>

<br/>

<p>
Please make the pending payments at your earliest convenience.
</p>

<p>
If you have already paid, kindly ignore this email.
</p>

<br/>

Regards,<br/>
<strong>RNG Supra Villas Management Team</strong>

</div>

</body>
</html>";

            await _emailService.SendEmailAsync(
                owner.Email,
                subject,
                body);

            TempData["Success"] = $"Reminder sent to {owner.Email} for {pendingRecords.Count} pending month(s).";
            return RedirectToAction(nameof(Index));
        }

        // SEND ALL REMINDERS (for all villas)
        [AdminOnly]
        public async Task<IActionResult> SendAllReminders()
        {
            var pendingRecords = _context.Maintenances
                .Where(x => !x.Payment_details)
                .OrderBy(x => x.Villa_No)
                .ThenBy(x => x.Due)
                .ToList();

            if (!pendingRecords.Any())
            {
                TempData["Error"] = "No pending maintenance records found.";
                return RedirectToAction(nameof(Index));
            }

            var grouped = pendingRecords.GroupBy(x => x.Villa_No);

            int totalSent = 0;
            int villasNotified = 0;

            foreach (var group in grouped)
            {
                var villaNo = group.Key;
                var records = group.ToList();

                var owner = _context.Owners
                    .FirstOrDefault(x => x.Villa_No == villaNo);

                if (owner == null || string.IsNullOrWhiteSpace(owner.Email))
                    continue;

                var tableRows = string.Empty;
                foreach (var record in records)
                {
                    string dueDisplay = $"₹ {record.DueAmount:N2}";
                    string paidDisplay = record.paid.HasValue ? $"₹ {record.paid.Value:N2}" : "₹ 0.00";
                    tableRows += $@"
<tr>
    <td style='padding:8px;border:1px solid #ddd;'>{record.Month}</td>
    <td style='padding:8px;border:1px solid #ddd;'>{record.Due:dd-MMM-yyyy}</td>
    <td style='padding:8px;border:1px solid #ddd;'>{dueDisplay}</td>
    <td style='padding:8px;border:1px solid #ddd;'>{paidDisplay}</td>
</tr>";
                }

                string body = $@"
<html>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f5;padding:20px;'>

<div style='max-width:650px;background:white;padding:30px;border-radius:8px;border:1px solid #ddd;'>

<h2 style='color:#1f4e79;'>RNG Supra Villas Management</h2>

<hr/>

<p>Dear <strong>{owner.Owner_name}</strong>,</p>

<p>This is a friendly reminder that you have <strong>{records.Count}</strong> pending maintenance payment(s) for <strong>Villa {villaNo}</strong>.</p>

<table style='border-collapse:collapse;width:100%;margin-top:20px;'>

<tr style='background:#f0f0f0;'>
    <th style='padding:10px;border:1px solid #ddd;'>Month</th>
    <th style='padding:10px;border:1px solid #ddd;'>Due Date</th>
    <th style='padding:10px;border:1px solid #ddd;'>Total Due</th>
    <th style='padding:10px;border:1px solid #ddd;'>Paid</th>
</tr>
{tableRows}
</table>

<br/>

<p>
Please make the pending payments at your earliest convenience.
</p>

<p>
If you have already paid, kindly ignore this email.
</p>

<br/>

Regards,<br/>
<strong>RNG Supra Villas Management Team</strong>

</div>

</body>
</html>";

                await _emailService.SendEmailAsync(owner.Email, "Maintenance Payment Reminder", body);
                totalSent += records.Count;
                villasNotified++;
            }

            TempData["Success"] = $"Reminders sent to {villasNotified} villa(s) covering {totalSent} pending record(s).";
            return RedirectToAction(nameof(Index));
        }
    }
}