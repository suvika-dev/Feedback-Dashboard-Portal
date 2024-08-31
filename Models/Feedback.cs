using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FDP.Models
{
    public class Feedback
    {
        [Key]
        public int FeedbackID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [ForeignKey("EvaluationType")]
        public int EvalTypeID { get; set; }

        [Range(1, 100, ErrorMessage = "Score must be between 1 and 100.")]
        public int Score { get; set; }

        [MaxLength(500)]
        public string Comments { get; set; }

        public DateTime Date { get; set; }

        [ForeignKey("EnteredByUser")]
        public int EnteredByUserID { get; set; }

        // Additional fields specific to evaluation types
       // public string TestType { get; set; } // Specific to Tests

       // public string CompetencyAreas { get; set; } // Specific to Assessments

        public string InterviewerFeedback { get; set; } // Specific to Interviews

        public string CandidateStrengths { get; set; } // Specific to Interviews

        public string Milestones { get; set; } // Specific to Project Progress

        [Range(0, 100, ErrorMessage = "Completion percentage must be between 0 and 100.")]
        public int CompletionPercentage { get; set; } // Specific to Project Progress

        // Navigation properties
        public virtual User User { get; set; }
        public virtual EvaluationType EvaluationType { get; set; }
        public virtual User EnteredByUser { get; set; }
       // public string EvaluatorComments { get; internal set; }
       // public string EmployeeName { get; internal set; }
    }
}
