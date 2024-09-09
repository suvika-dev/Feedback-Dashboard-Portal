using Microsoft.AspNetCore.Mvc;
using FDP.Models;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FDP.Data;
using DinkToPdf;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using iText.Commons.Actions.Contexts;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Security.Claims;

public class ReportsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ICompositeViewEngine _viewEngine;



    public ReportsController(ApplicationDbContext context, ICompositeViewEngine viewEngine)
    {
        _viewEngine = viewEngine;
        _context = context;
    }
    public IActionResult Generate()
    {
        var model = new ReportGenerationViewModel
        {
            Users = GetUsersSelectList(),
            EvalTypes = GetEvalTypesSelectList(),
            StartDate = DateTime.Now.AddMonths(-1),  // Default to the past month
            EndDate = DateTime.Now
        };

        return View(model);
    }

    [HttpPost]

    public async Task<IActionResult> GenerateReport(ReportGenerationViewModel model, IFormFile companyLogo)

    {
      
            ModelState.Clear();
        
       
        var roleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
        var roleName = roleClaim?.Value;

        // Check if the role is HR Manager or Admin
        if (roleName != "HR Manager" && roleName != "Admin")
        {
            return Unauthorized("Access denied. You do not have permission to generate reports.");
        }

        // Remove unnecessary validation for optional fields
        if (model.EvalTypeID is null)
            ModelState.Remove("EvalTypeID");
        if (model.UserID is null)
            ModelState.Remove("UserID");

        ModelState.Remove("Users");
        ModelState.Remove("EvalTypes");
        ModelState.Remove("FilteredFeedbacks");
        ModelState.Remove("companyLogo");

        if (ModelState.IsValid)
        {
            var feedbackQuery = _context.Feedback
                .Include(f => f.User)
                .Include(f => f.EvaluationType)
                .AsQueryable();

            if (model.UserID.HasValue && model.UserID.Value != 0)
            {
                feedbackQuery = feedbackQuery.Where(f => f.UserID == model.UserID.Value);
            }

            if (model.EvalTypeID.HasValue && model.EvalTypeID.Value != 0)
            {
                feedbackQuery = feedbackQuery.Where(f => f.EvalTypeID == model.EvalTypeID.Value);
            }

            feedbackQuery = feedbackQuery.Where(f => f.Date >= model.StartDate && f.Date <= model.EndDate);

            var feedbacks = await feedbackQuery.ToListAsync();

            // Generate PDF with table using iText7
            using (var stream = new MemoryStream())
            {
                var writer = new PdfWriter(stream);
                var pdfDocument = new PdfDocument(writer);
                var document = new Document(pdfDocument);

                // Add company logo if uploaded
                if (companyLogo != null && companyLogo.Length > 0)
                {
                    using (var ms = new MemoryStream())
                    {
                        await companyLogo.CopyToAsync(ms);
                        var imageData = ImageDataFactory.Create(ms.ToArray());
                        var logo = new Image(imageData);
                        logo.ScaleToFit(100, 50); // Adjust width and height as needed
                        logo.SetHorizontalAlignment(HorizontalAlignment.CENTER); // Align the logo
                        document.Add(logo);
                    }
                }

                // Add title
                document.Add(new Paragraph("Feedback Report")
                    .SetTextAlignment(TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetBold());

                var columnWidths = new List<float> { 2, 2, 2, 2 }; // Default widths for common columns

                // Adjust column widths based on conditional columns
                if (model.EvalTypeID.HasValue && model.EvalTypeID.Value != 0)
                {
                    switch (model.EvalTypeID)
                    {
                        case 1: // Tests
                            columnWidths.Add(2); // Add column for Test Type
                            break;
                        case 2: // Assessments
                            columnWidths.Add(2); // Add column for Competency Areas
                            break;
                        case 3: // Interviews
                            columnWidths.Add(2); // Add column for Interviewer Feedback
                            columnWidths.Add(2); // Add column for Candidate Strengths
                            break;
                        case 4: // Project Progress
                            columnWidths.Add(2); // Add column for Milestones
                            columnWidths.Add(2); // Add column for Completion Percentage
                            break;
                    }
                }

                // Add an additional column width for comments if needed
                if (model.IncludeComments)
                {
                    columnWidths.Add(2); // Add column for Comments
                }

                // Create the table using the calculated column widths
                var table = new iText.Layout.Element.Table(columnWidths.ToArray());  // Directly pass the array
               // var table = new iText.Layout.Element.Table(UnitValue.CreatePercentArray(columnWidths.ToArray()))
                  //  .SetWidth(UnitValue.CreatePercentValue(100)); // Set table to full width

                // Add common table headers
                table.AddHeaderCell(new Cell().Add(new Paragraph("Employee").SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Evaluation Type").SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetBold()));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Score").SetBold()));

                // Track additional columns and ensure alignment
                if (model.EvalTypeID.HasValue && model.EvalTypeID.Value != 0)
                {
                    switch (model.EvalTypeID)
                    {
                        case 1: // Tests
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Test Type").SetBold()));
                            break;
                        case 2: // Assessments
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Competency Areas").SetBold()));                           
                            break;
                        case 3: // Interviews
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Interviewer Feedback").SetBold()));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Candidate Strengths").SetBold()));                          
                            break;
                        case 4: // Project Progress
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Milestones").SetBold()));
                            table.AddHeaderCell(new Cell().Add(new Paragraph("Completion Percentage").SetBold()));                           
                            break;
                    }
                }

                // Add extra column for comments if needed
                if (model.IncludeComments)
                {
                    table.AddHeaderCell(new Cell().Add(new Paragraph("Comments").SetBold()));
                   // columnWidths.Add(2); // Add column for Comments
                }

                if (feedbacks != null && feedbacks.Any())
                {

                    // Add rows to the table
                    foreach (var feedback in feedbacks)
                    {
                        // Add common columns
                        table.AddCell(new Cell().Add(new Paragraph(feedback.User?.Username ?? "N/A")));
                        table.AddCell(new Cell().Add(new Paragraph(feedback.EvaluationType?.EvalName ?? "N/A")));
                        table.AddCell(new Cell().Add(new Paragraph(feedback.Date.ToShortDateString())));
                        table.AddCell(new Cell().Add(new Paragraph(feedback.Score.ToString())));

                        // Add conditional columns based on EvalTypeID
                        if (model.EvalTypeID.HasValue && model.EvalTypeID.Value != 0)
                        {
                            switch (feedback.EvalTypeID)
                            {
                                case 1: // Tests
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.TestType ?? "N/A")));
                                    break;
                                case 2: // Assessments
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.CompetencyAreas ?? "N/A")));
                                    break;
                                case 3: // Interviews
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.InterviewerFeedback ?? "N/A")));
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.CandidateStrengths ?? "N/A")));
                                    break;
                                case 4: // Project Progress
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.Milestones ?? "N/A")));
                                    table.AddCell(new Cell().Add(new Paragraph(feedback.CompletionPercentage.ToString("P2"))));
                                    break;
                            }
                        }

                        // Add an empty cell for conditional columns that are not present
                        else
                        {
                            switch (model.EvalTypeID)
                            {
                                case 1:
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    break;
                                case 2:
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    break;
                                case 3:
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    break;
                                case 4:
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    table.AddCell(new Cell().Add(new Paragraph("")));
                                    break;
                            }
                        }

                        // Add comments column if enabled
                        if (model.IncludeComments)
                        {
                            table.AddCell(new Cell().Add(new Paragraph(feedback.Comments ?? "N/A")));
                        }
                        //else
                        //{
                        //    table.AddCell(new Cell().Add(new Paragraph("")));
                        //}
                    }

                    // Add the table to the document
                    document.Add(table);

                    document.Close();

                    return File(stream.ToArray(), "application/pdf", "GeneratedReport.pdf");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "No feedback available for the selected options."); 
                    model.Users = GetUsersSelectList();
                    model.EvalTypes = GetEvalTypesSelectList();
                    return View("Generate",model);

                }
            }
        }

        // If model state is invalid, reload the view with dropdowns
        model.Users = GetUsersSelectList();
        model.EvalTypes = GetEvalTypesSelectList();
        return View("Generate", model);
        
    }







    private IEnumerable<SelectListItem> GetUsersSelectList()
    {
        return _context.Users.Select(u => new SelectListItem
        {
            Value = u.UserID.ToString(),
            Text = u.Username
        }).ToList();
    }

    private IEnumerable<SelectListItem> GetEvalTypesSelectList()
    {
        return _context.EvaluationType.Select(e => new SelectListItem
        {
            Value = e.EvalTypeID.ToString(),
            Text = e.EvalName
        }).ToList();
    }
}
