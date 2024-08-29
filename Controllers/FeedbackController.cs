using FDP.Data;
using FDP.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
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
                Score = model.Score, // Provide a default value if Score is null
                Comments = model.Comments ?? string.Empty, // Provide a default value if Comments is null
                TestType = model.TestType ?? string.Empty,
                CompetencyAreas = model.CompetencyAreas ?? string.Empty,
                EvaluatorComments = model.EvaluatorComments ?? string.Empty,
                InterviewerFeedback = model.InterviewerFeedback ?? string.Empty,
                CandidateStrengths = model.CandidateStrengths ?? string.Empty,
                Milestones = model.Milestones ?? string.Empty,
                CompletionPercentage = model.CompletionPercentage.HasValue ? (int)model.CompletionPercentage.Value : 0, // Ensure CompletionPercentage has a value
                Date = model.Date  // Provide a default value if Date is null
            };

            _context.Feedback.Add(feedback);
            _context.SaveChanges();

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> FeedbackList()
        {
            var feedbacks = await _context.Feedback
                .Include(f => f.User)
                .Include(f => f.EvaluationType)
                .Select(f => new FeedbackListViewModel
                {
                    FeedbackID = f.FeedbackID,
                    Username = f.User.Username,
                    UserID = f.UserID,
                    RoleID = f.User.UserRoles.Select(ur => ur.RoleID).FirstOrDefault(),
                    EvalType = f.EvaluationType.ToString(),
                    Score = f.Score,
                    Date = f.Date,
                    InterviewerFeedback = f.InterviewerFeedback,
                    CandidateStrengths = f.CandidateStrengths,
                    Milestones = f.Milestones,
                    CompletionPercentage = f.CompletionPercentage
                })
                .ToListAsync();

            return View(feedbacks);
        }
        public async Task<IActionResult> Edit(int id)
        {
            var feedback = await _context.Feedback
                .Include(f => f.User)
                .Include(f => f.EvaluationType)
                .FirstOrDefaultAsync(f => f.FeedbackID == id);

            if (feedback == null)
            {
                return NotFound();
            }

            var model = new FeedbackFormViewModel
            {
                UserID = feedback.UserID,
                EvalTypeID = feedback.EvalTypeID,
                Score = feedback.Score,
                Comments = feedback.Comments,
                InterviewerFeedback = feedback.InterviewerFeedback,
                CandidateStrengths = feedback.CandidateStrengths,
                Milestones = feedback.Milestones,
                CompletionPercentage = feedback.CompletionPercentage,
                Date = feedback.Date,
                Users = _context.Users.Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(),
                    Text = u.Username
                }).ToList(),
                EvalTypes = _context.EvaluationType.Select(et => new SelectListItem
                {
                    Value = et.EvalTypeID.ToString(),
                    Text = et.ToString()  // Ensure you have a proper ToString() override or use a different property
                }).ToList()
            };

            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(FeedbackFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                var feedback = await _context.Feedback.FindAsync(model.FeedbackID);

                if (feedback == null)
                {
                    return NotFound();
                }

                feedback.UserID = model.UserID;
                feedback.EvalTypeID = model.EvalTypeID;
                feedback.Score = model.Score;
                feedback.Comments = model.Comments;
                feedback.InterviewerFeedback = model.InterviewerFeedback;
                feedback.CandidateStrengths = model.CandidateStrengths;
                feedback.Milestones = model.Milestones;
                feedback.CompletionPercentage = (int)model.CompletionPercentage;
                feedback.Date = model.Date;

                _context.Update(feedback);
                await _context.SaveChangesAsync();

                return RedirectToAction("FeedbackList");
            }

            // Re-populate the dropdown lists in case of a validation error
            model.Users = _context.Users.Select(u => new SelectListItem
            {
                Value = u.UserID.ToString(),
                Text = u.Username
            }).ToList();
            model.EvalTypes = _context.EvaluationType.Select(et => new SelectListItem
            {
                Value = et.EvalTypeID.ToString(),
                Text = et.ToString()  // Ensure you have a proper ToString() override or use a different property
            }).ToList();

            return View(model);
        }





    }

}