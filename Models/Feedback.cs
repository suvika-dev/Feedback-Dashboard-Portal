namespace FDP.Models
{
    public class Feedback
    {
        public int FeedbackID { get; set; }
        public int UserID { get; set; }
        public int EvalTypeID { get; set; }
        public decimal Score { get; set; }
        public string Comments { get; set; }
        public DateTime Date { get; set; }
        public int EnteredByUserID { get; set; }
    }
}
