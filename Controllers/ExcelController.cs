using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using VillaCommunityManagement.Data;
using VillaCommunityManagement.Filters;
using VillaCommunityManagement.Models;

namespace VillaCommunityManagement.Controllers
{
    [AdminOnly]
    public class ExcelController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExcelController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        // EXPORT
        public IActionResult ExportOwners()
        {
            using var workbook = new XLWorkbook();

            var ws = workbook.Worksheets.Add("Owners");

            ws.Cell(1, 1).Value = "Villa No";
            ws.Cell(1, 2).Value = "Owner";
            ws.Cell(1, 3).Value = "Tenant";
            ws.Cell(1, 4).Value = "Phone";
            ws.Cell(1, 5).Value = "Email";
            ws.Cell(1, 6).Value = "Status";

            int row = 2;

            foreach (var owner in _context.Owners)
            {
                ws.Cell(row, 1).Value = owner.Villa_No;
                ws.Cell(row, 2).Value = owner.Owner_name;
                ws.Cell(row, 3).Value = owner.Tenant_name;
                ws.Cell(row, 4).Value = owner.Phone;
                ws.Cell(row, 5).Value = owner.Email;
                ws.Cell(row, 6).Value = owner.Status;

                row++;
            }

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Owners.xlsx");
        }

        // IMPORT
        [HttpPost]
        public IActionResult ImportOwners(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Message"] = "Please select an Excel file.";
                return RedirectToAction("Index");
            }

            using var stream = file.OpenReadStream();
            using var workbook = new XLWorkbook(stream);
            var ws = workbook.Worksheet(1);
            int lastRow = ws.LastRowUsed().RowNumber();

            for (int row = 2; row <= lastRow; row++)
            {
                int villaNo = ws.Cell(row, 1).GetValue<int>();
                var owner = _context.Owners.Find(villaNo);

                if (owner != null)
                {
                    owner.Owner_name = ws.Cell(row, 2).GetString() ?? string.Empty;
                    owner.Tenant_name = ws.Cell(row, 3).GetString() ?? string.Empty;
                    owner.Phone = ws.Cell(row, 4).GetString() ?? string.Empty;
                    owner.Email = ws.Cell(row, 5).GetString() ?? string.Empty;
                    owner.Status = ws.Cell(row, 6).GetString() ?? string.Empty;
                }
                else
                {
                    owner = new Owner
                    {
                        Villa_No = villaNo,
                        Owner_name = ws.Cell(row, 2).GetString() ?? string.Empty,
                        Tenant_name = ws.Cell(row, 3).GetString() ?? string.Empty,
                        Phone = ws.Cell(row, 4).GetString() ?? string.Empty,
                        Email = ws.Cell(row, 5).GetString() ?? string.Empty,
                        Status = ws.Cell(row, 6).GetString() ?? string.Empty
                    };
                    _context.Owners.Add(owner);
                }
            }

            _context.SaveChanges();
            TempData["Message"] = "Excel Imported Successfully!";
            return RedirectToAction("Index");
        }
    }
}