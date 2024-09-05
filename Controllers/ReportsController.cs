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
    if (model.EvalTypeID is null)
        ModelState.Remove("EvalTypeID");
    if (model.UserID is null)
        ModelState.Remove("UserID");

    ModelState.Remove("Users");
    ModelState.Remove("EvalTypes");
    ModelState.Remove("FilteredFeedbacks");

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
                        // Scale logo to a smaller size
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

            // Create a table with 5 columns for Employee, EvalType, Score, Comments, Date
            iText.Layout.Element.Table table = new iText.Layout.Element.Table(new float[] { 2, 2, 1, 3, 2 });
            table.SetWidth(UnitValue.CreatePercentValue(100)); // Set table to full width

            // Add table headers
            table.AddHeaderCell(new Cell().Add(new Paragraph("Employee").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Evaluation Type").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Score").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Comments").SetBold()));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Date").SetBold()));

            // Add rows to the table
            foreach (var feedback in feedbacks)
            {
                table.AddCell(new Cell().Add(new Paragraph(feedback.User?.Username ?? "N/A")));
                table.AddCell(new Cell().Add(new Paragraph(feedback.EvaluationType?.EvalName ?? "N/A")));
                table.AddCell(new Cell().Add(new Paragraph(feedback.Score.ToString())));
                table.AddCell(new Cell().Add(new Paragraph(feedback.Comments ?? "N/A")));
                table.AddCell(new Cell().Add(new Paragraph(feedback.Date.ToShortDateString())));
            }

            // Add the table to the document
            document.Add(table);

            document.Close();

            return File(stream.ToArray(), "application/pdf", "GeneratedReport.pdf");
        }
    }

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
