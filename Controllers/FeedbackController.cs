using FDP.Data;
using FDP.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using Newtonsoft.Json;

namespace FDP.Controllers
{
    public class FeedbackController : Controller

    {

       // private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        //UserManager<ApplicationUser> userManager,

        public FeedbackController( ApplicationDbContext context)
        {
           // _userManager = userManager;
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
        public async Task<IActionResult> Create(FeedbackFormViewModel model)
        {
           
            // Adjust ModelState based on EvalTypeID
            switch (model.EvalTypeID)
            {
                case 1: // Test
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("CompetencyAreas");                   
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
                    ModelState.Remove("Milestones");
                    break;
                case 4: // Project Progress
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("TestType");
                    ModelState.Remove("CompetencyAreas");                   
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
           
            // Retrieve the UserID claim
            var userIdClaim =  User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return BadRequest("User ID claim is missing or invalid.");
            }
            try
            {
                var feedback = new Feedback
                {
                    UserID = model?.UserID ?? default(int),
                    EvalTypeID = model?.EvalTypeID ?? default(int),
                    Score = model?.Score ?? default(int),
                    Comments = model?.Comments?.ToString() ?? "N/A",
                    TestType = model?.TestType?.ToString() ?? "N/A",
                    CompetencyAreas = model?.CompetencyAreas?.ToString() ?? "N/A",
                    InterviewerFeedback = model?.InterviewerFeedback?.ToString() ?? "N/A",
                    CandidateStrengths = model?.CandidateStrengths?.ToString() ?? "N/A",
                    Milestones = model?.Milestones?.ToString() ?? "N/A",
                    CompletionPercentage = model?.CompletionPercentage ?? 0,
                    Date = DateTime.Now,
                    EnteredByUserID = int.Parse(userIdClaim)
                };
                _context.Feedback.Add(feedback);
                // _context.SaveChanges();
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                // Log the exception details
                Console.WriteLine($"Error initializing Feedback: {ex.Message}");
                Console.WriteLine(ex.StackTrace); // Optionally log the stack trace
            }


           

            return RedirectToAction("FeedbackList");
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
                    EvalType = f.EvaluationType.EvalName.ToString(),
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


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid ID");
            }
            try
            {
                var feedback = await _context.Feedback.FindAsync(id);

                if (feedback == null)
                {
                    return NotFound();
                }

                var model = new FeedbackFormViewModel
                {
                    FeedbackID = feedback.FeedbackID,
                    UserID = feedback.UserID,
                    EvalTypeID = feedback.EvalTypeID,
                    Score = feedback.Score,
                    Comments = feedback.Comments,
                    TestType = feedback.TestType,
                    CompetencyAreas = feedback.CompetencyAreas,
                    // EvaluatorComments = feedback.EvaluatorComments,
                    InterviewerFeedback = feedback.InterviewerFeedback,
                    CandidateStrengths = feedback.CandidateStrengths,
                    Milestones = feedback.Milestones,
                    CompletionPercentage = feedback.CompletionPercentage,
                    // Populate the select lists for drop-downs
                    Users = GetUsersSelectList(),
                    EvalTypes = GetEvalTypesSelectList()
                };

                return View("Create", model); // Load the same form view
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(FeedbackFormViewModel model)
        {
            switch (model.EvalTypeID)
            {
                case 1: // Test
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("CompetencyAreas");
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
                    ModelState.Remove("Milestones");
                    break;
                case 4: // Project Progress
                    ModelState.Remove("Users");
                    ModelState.Remove("EvalTypes");
                    ModelState.Remove("TestType");
                    ModelState.Remove("CompetencyAreas");
                    ModelState.Remove("InterviewerFeedback");
                    ModelState.Remove("CandidateStrengths");
                    break;
                default:
                    break;
            }
            if (ModelState.IsValid)
            {
                var feedback = await _context.Feedback.FindAsync(model.FeedbackID);
                if (feedback == null)
                {
                    return NotFound();
                }

                // Update only the fields that have changed
                feedback.UserID = model.UserID;
                feedback.EvalTypeID = model.EvalTypeID;
                feedback.Score = model.Score;
                feedback.Comments = model.Comments;
                feedback.TestType = model.TestType;
                feedback.CompetencyAreas = model.CompetencyAreas;
               // feedback.EvaluatorComments = model;
                feedback.InterviewerFeedback = model.InterviewerFeedback;
                feedback.CandidateStrengths = model.CandidateStrengths;
                feedback.Milestones = model.Milestones;
                feedback.CompletionPercentage = model.CompletionPercentage.HasValue ? (int)model.CompletionPercentage.Value : 0;

                // Save changes to the database
                _context.Feedback.Update(feedback);
                await _context.SaveChangesAsync();

                return RedirectToAction("FeedbackList");
            }

            // If model state is not valid, reload the form with the model data
            model.Users = GetUsersSelectList();
            model.EvalTypes = GetEvalTypesSelectList();
            return View("Create",model);
        }

        public IEnumerable<SelectListItem> GetUsersSelectList()
        {
            return _context.Users
                .Select(u => new SelectListItem
                {
                    Value = u.UserID.ToString(), // Assuming UserID is the ID property
                    Text = u.Username // Assuming Username is the display text
                })
                .ToList();
        }
        public IEnumerable<SelectListItem> GetEvalTypesSelectList()
        {
            return _context.EvaluationType  
                .Select(e => new SelectListItem
                {
                    Value = e.EvalTypeID.ToString(), // Assuming EvalTypeID is the ID property
                    Text = e.EvalName // Assuming EvalName is the display text
                })
                .ToList();
        }





    }

}