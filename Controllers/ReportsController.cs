using Microsoft.AspNetCore.Mvc;
using FDP.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

public class ReportsController : Controller
{
    [HttpGet]
    public IActionResult Generate()
    {
        var model = new ReportGenerationViewModel
        {
            Departments = GetDepartments() // Initialize with a list of departments
        };

        return View(model);
    }

    [HttpPost]
    public IActionResult Generate(ReportGenerationViewModel model)
    {
        if (ModelState.IsValid)
        {
            // Handle report generation logic here
        }

        // Reinitialize departments in case of errors
        model.Departments = GetDepartments();
        return View(model);
    }

    private IEnumerable<SelectListItem> GetDepartments()
    {
        // Example implementation, replace with actual data retrieval logic
        return new List<SelectListItem>
        {
            new SelectListItem { Value = "1", Text = "Department 1" },
            new SelectListItem { Value = "2", Text = "Department 2" }
        };
    }
}
