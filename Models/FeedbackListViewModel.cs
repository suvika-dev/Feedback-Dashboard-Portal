namespace FDP.Models
{
    public class FeedbackListViewModel
    {
        public int FeedbackID { get; set; }
        public string Username { get; set; }
        public int UserID { get; set; }    // Add UserID if needed
        public int RoleID { get; set; }    // Add RoleID if needed
        public string EvalType { get; set; }
        public int Score { get; set; }
        public DateTime Date { get; set; }
        public string InterviewerFeedback { get; set; }
        public string CandidateStrengths { get; set; }
        public string Milestones { get; set; }
        public int? CompletionPercentage { get; set; }
    }
}
