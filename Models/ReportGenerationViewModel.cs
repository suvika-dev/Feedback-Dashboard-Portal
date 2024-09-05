using Microsoft.AspNetCore.Mvc.Rendering;

namespace FDP.Models
{
    public class ReportGenerationViewModel
    {
        public int? UserID { get; set; }  // Nullable to allow filtering based on selection
        public int? EvalTypeID { get; set; }  // Nullable to allow filtering based on selection
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; }
        public IEnumerable<SelectListItem> EvalTypes { get; set; }

        public IEnumerable<Feedback> FilteredFeedbacks { get; set; }
    }
}
