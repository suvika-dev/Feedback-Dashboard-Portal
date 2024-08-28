namespace FDP.Models
{
    public class Report
    {
        public int ReportID { get; set; }
        public string ReportName { get; set; }
        public DateTime GeneratedDate { get; set; }


        public int GeneratedByUserID { get; set; }
        public string ReportData { get; set; }
    }
}
