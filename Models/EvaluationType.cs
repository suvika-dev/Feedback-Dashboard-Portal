using System.ComponentModel.DataAnnotations;

namespace FDP.Models
{
    public class EvaluationType
    {
        [Key]
        public int EvalTypeID { get; set; } // Primary Key
        public string EvalName { get; set; }
        public string Description { get; set; }
    }
}
