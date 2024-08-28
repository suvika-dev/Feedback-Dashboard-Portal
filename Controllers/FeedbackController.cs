using FDP.Data;
using FDP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;

namespace FDP.Controllers
{
    public class FeedbackController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedbackController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            var model = new FeedbackFormViewModel
            {
                Users = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = u.Username // Assume EmployeeName is the name field in the Users table
                }).ToList(),
                EvalTypes = _context.EvaluationType.Select(e => new SelectListItem
                {
                    Value = e.EvalTypeID.ToString(),
                    Text = e.EvalName
                }).ToList()
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult Create(FeedbackFormViewModel model)
        {
            // Adjust ModelState based on EvalTypeID
            switch (model.EvalTypeID)
            {
                case 1: // Test
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("CompetencyAreas");
                    ModelState.Remove("EvaluatorComments");
                    ModelState.Remove("InterviewerFeedback");
                    ModelState.Remove("CandidateStrengths");
                    ModelState.Remove("Milestones");
                    break;
                case 2: // Assessment
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("TestType");
                    ModelState.Remove("InterviewerFeedback");
                    ModelState.Remove("CandidateStrengths");
                    ModelState.Remove("Milestones");
                    break;
                case 3: // Interview
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("TestType");
                    ModelState.Remove("CompetencyAreas");
                    ModelState.Remove("EvaluatorComments");
                    ModelState.Remove("Milestones");
                    break;
                case 4: // Project Progress
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("TestType");
                    ModelState.Remove("CompetencyAreas");
                    ModelState.Remove("EvaluatorComments");
                    ModelState.Remove("InterviewerFeedback");
                    ModelState.Remove("CandidateStrengths");
                    break;
                default:
                    break;
            }

            if (!ModelState.IsValid)
            {
                // Re-populate dropdowns in case of validation failure
                model.Users = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = u.Username
                }).ToList();

                // Re-populate the EvalTypes dropdown in case of validation failure
                model.EvalTypes = _context.EvaluationType.Select(e => new SelectListItem
                {
                    Value = e.EvalTypeID.ToString(),
                    Text = e.EvalName
                }).ToList();

                return View(model);
            }

            // Check for duplicates
            bool isDuplicate = _context.Feedback.Any(f => f.UserID == model.UserID &&
                                                            f.Date == model.Date &&
                                                            f.EvalTypeID == model.EvalTypeID);
            if (isDuplicate)
            {
                ModelState.AddModelError("", "Duplicate entry found. Please revise your input.");
                return View(model);
            }

            var feedback = new Feedback
            {
                UserID = model.UserID,
                EvalTypeID = model.EvalTypeID,
                Score = model.Score,
                Comments = model.Comments,
                TestType = model.TestType,
                CompetencyAreas = model.CompetencyAreas,
                EvaluatorComments = model.EvaluatorComments,
                InterviewerFeedback = model.InterviewerFeedback,
                CandidateStrengths = model.CandidateStrengths,
                Milestones = model.Milestones,
                CompletionPercentage = (int)model.CompletionPercentage,
                Date = model.Date
            };

            _context.Feedback.Add(feedback);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }



    }

}