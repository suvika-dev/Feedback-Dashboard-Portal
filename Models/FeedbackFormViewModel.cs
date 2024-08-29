using FDP.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FDP.Models
{
    public class FeedbackFormViewModel
    {
        [Required]
        public int FeedbackID { get; set; }
        [Required]
        public int UserID { get; set; }

        public IEnumerable<SelectListItem> Users { get; set; }

        //[Required]
        //public string EmployeeName { get; set; }

        [Required]
        public int EvalTypeID { get; set; }

        public IEnumerable<SelectListItem> EvalTypes { get; set; }

        [Required]
        [Range(1, 100, ErrorMessage = "Score must be between 1 and 100.")]
        public int Score { get; set; }

        public string Comments { get; set; }

        // Specific to Tests
        [RequiredIfEvaluationType("EvalTypeID", "1")]
        public string TestType { get; set; }

        // Specific to Assessments
        [RequiredIfEvaluationType("EvalTypeID", "2")]
        public string CompetencyAreas { get; set; }

        [RequiredIfEvaluationType("EvalTypeID", "2")]
        public string EvaluatorComments { get; set; }

        // Specific to Interviews
        [RequiredIfEvaluationType("EvalTypeID", "3")]
        public string InterviewerFeedback { get; set; }

        [RequiredIfEvaluationType("EvalTypeID", "3")]
        public string CandidateStrengths { get; set; }

        // Specific to Project Progress
        [RequiredIfEvaluationType("EvalTypeID", "4")]
        public string Milestones { get; set; }

        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100.")]
        public int? CompletionPercentage { get; set; }

        public DateTime Date { get; set; }
    }

}
