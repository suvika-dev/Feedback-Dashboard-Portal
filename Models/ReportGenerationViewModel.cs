using Microsoft.AspNetCore.Mvc.Rendering;

namespace FDP.Models
{
    public class ReportGenerationViewModel
    {
        public string ReportType { get; set; } // Ensure this property exists
        public int DepartmentID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }
    }
}
