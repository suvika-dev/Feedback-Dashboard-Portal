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

    
        private readonly ApplicationDbContext _context;
        

        public FeedbackController( ApplicationDbContext context)
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
                    Username = f.User.Username??"Unknown" , // Provide a default value if User is null
                    UserID = f.UserID,
                    RoleID = f.User.UserRoles.Select(ur => ur.RoleID).FirstOrDefault() , // Handle null UserRoles
                    EvalType = f.EvaluationType.EvalName ?? "No Evaluation Type", // Handle null EvaluationType
                    Score = f.Score,
                    Date = f.Date,
                    InterviewerFeedback = f.InterviewerFeedback ?? "No Feedback", // Provide a default value if null
                    CandidateStrengths = f.CandidateStrengths ?? "No Strengths", // Provide a default value if null
                    Milestones = f.Milestones ?? "No Milestones", // Provide a default value if null
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

                //var feedback = await _context.Feedback
                //    .Include(f => f.User.UserID) // Make sure to include related data if necessary
                //    .Include(f => f.EvaluationType.EvalTypeID) // Same here
                //    .FirstOrDefaultAsync(f => f.FeedbackID == id);

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
                    Comments = feedback.Comments ?? string.Empty, // Handle null values
                    TestType = feedback.TestType ?? string.Empty, // Handle null values
                    CompetencyAreas = feedback.CompetencyAreas ?? string.Empty, // Handle null values
                   // EvaluatorComments = feedback.EvaluatorComments ?? string.Empty, // Handle null values
                    InterviewerFeedback = feedback.InterviewerFeedback ?? string.Empty, // Handle null values
                    CandidateStrengths = feedback.CandidateStrengths ?? string.Empty, // Handle null values
                    Milestones = feedback.Milestones ?? string.Empty, // Handle null values
                   // CompletionPercentage = feedback.CompletionPercentage ?? 0, // Handle null values if it's nullable
                    Users = await GetUsersSelectListAsync(), // Ensure this returns a Task<IEnumerable<SelectListItem>>
                    EvalTypes = await GetEvalTypesSelectListAsync() // Ensure this returns a Task<IEnumerable<SelectListItem>>
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
            model.Users =  await GetUsersSelectListAsync   ();
            model.EvalTypes = await  GetEvalTypesSelectListAsync();
            return View("Create",model);
        }

        private async Task<IEnumerable<SelectListItem>> GetUsersSelectListAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(u => new SelectListItem
            {
                Value = u.UserID.ToString(),
                Text = u.Username
            });
        }

        private async Task<IEnumerable<SelectListItem>> GetEvalTypesSelectListAsync()
        {
            var evalTypes = await _context.EvaluationType.ToListAsync();
            return evalTypes.Select(e => new SelectListItem
            {
                Value = e.EvalTypeID.ToString(),
                Text = e.EvalName
            });
        }





    }

}