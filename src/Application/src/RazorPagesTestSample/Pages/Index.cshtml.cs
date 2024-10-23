using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RazorPagesTestSample.Data;
using System.Threading;
using System.IO;
using System.IO.Compression;

namespace RazorPagesTestSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly AppDbContext _db;

        public IndexModel(AppDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Message Message { get; set; }

        public IList<Message> Messages { get; private set; }

        [TempData]
        public string MessageAnalysisResult { get; set; }

        #region snippet1
        public async Task OnGetAsync()
        {
            Messages = await _db.GetMessagesAsync();
        }
        #endregion

        public async Task<IActionResult> OnPostAddMessageAsync()
        {
            if (!ModelState.IsValid)
            {
                Messages = await _db.GetMessagesAsync();

                return Page();
            }

            await _db.AddMessageAsync(Message);

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAllMessagesAsync()
        {
            await _db.DeleteAllMessagesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteMessageAsync(int id)
        {
            await _db.DeleteMessageAsync(id);

            return RedirectToPage();
        }

        /// <summary>
        /// Analyzes the messages stored in the database and calculates the average word count per message.
        /// If there are no messages, sets the analysis result to indicate that there are no messages to analyze.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains an <see cref="IActionResult"/>
        /// that redirects to the current page.
        /// </returns>
        /// Integrate REsolve #8
    
        public async Task<IActionResult> OnPostAnalyzeMessagesAsync()
        {
            Messages = await _db.GetMessagesAsync();

            if (Messages.Count == 0)
            {
                MessageAnalysisResult = "There are no messages to analyze.";
            }
            else
            {
                var wordCount = 0;

                foreach (var message in Messages)
                {
                    wordCount += CountWords(message.Text);
                }

                var avgWordCount = Decimal.Divide(wordCount, Messages.Count);
                MessageAnalysisResult = $"The average message length is {avgWordCount:0.##} words.";
            }

            return RedirectToPage();
        }

        private int CountWords(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return 0;
            }

            var wordArray = text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            return wordArray.Length;
        }

        public static void WriteToDirectory(ZipArchiveEntry entry, string destDirectory)
        {
            string destFileName = Path.Combine(destDirectory, entry.FullName);
        
            // Ensure the destination path is within the intended directory
            string fullPath = Path.GetFullPath(destFileName);
            
            if (!fullPath.StartsWith(Path.GetFullPath(destDirectory), StringComparison.OrdinalIgnoreCase))
            {
                throw new UnauthorizedAccessException("Attempt to write outside of the destination directory.");
            }
        
            entry.ExtractToFile(fullPath);
        }
    }
}
