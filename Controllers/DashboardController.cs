
using FDP.Data;
using FDP.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace FDP.Controllers
{
    public class DashboardController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Dashboard/Index
        public ActionResult Index()
        {
           

            return View();
        }

        // GET: Dashboard/GetKPIData
       
        // GET: Dashboard/Filter
        public ActionResult Filter(DateTime? startDate, DateTime? endDate)
        {
            // Logic to filter dashboard data
            return View();
        }
    }
}
