using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Tesseract;

namespace InfoCheckWeb.Pages
{
    public class IndexModel : PageModel
    {
        [BindProperty]
        public IFormFile? UploadedFile { get; set; } // Made nullable
        public string OCRResult { get; set; } = string.Empty; // Initialized as empty
        public string[] VisibleDetails { get; set; } = Array.Empty<string>(); // Initialized as an empty array
        public string[] RestrictedDetails { get; set; } = Array.Empty<string>(); // Initialized as an empty array
        public string PromoCodeMessage { get; set; } = string.Empty; // Initialized as empty

        public enum SubscriptionLevel
        {
            Free,
            Premium,
            Pro
        }

        public SubscriptionLevel CurrentSubscription { get; set; } = SubscriptionLevel.Free; // Default to Free tier

        private readonly List<string> ValidPromoCodes = new() { "PARTNER2024", "VIPACCESS", "UNLOCKPRO" };

        public void OnGet()
        {
            // Default subscription level setup if needed
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (UploadedFile == null || UploadedFile.Length == 0)
            {
                OCRResult = "Please upload a valid image file.";
                return Page();
            }

            // Save the uploaded file temporarily
            var filePath = Path.GetTempFileName();
            using (var stream = System.IO.File.Create(filePath))
            {
                await UploadedFile.CopyToAsync(stream);
            }

            // Path to Tesseract data
            string tesseractDataPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "tessdata");

            try
            {
                using (var engine = new TesseractEngine(tesseractDataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(filePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            // Extract text from image
                            OCRResult = page.GetText();

                            // Process extracted text based on subscription level
                            ProcessOCRResult(OCRResult);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                OCRResult = $"Error during OCR processing: {ex.Message}\n{ex.StackTrace}";
            }

            return Page();
        }

        public IActionResult OnPostPromoCode(string promoCode)
        {
            if (ValidPromoCodes.Contains(promoCode))
            {
                CurrentSubscription = SubscriptionLevel.Pro;
                PromoCodeMessage = "Promo code applied successfully! You now have access to Pro features.";
            }
            else
            {
                PromoCodeMessage = "Invalid promo code. Please try again.";
            }

            return Page();
        }

        private void ProcessOCRResult(string ocrText)
        {
            // Split OCR text into lines for processing
            var details = ocrText.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            // Filter details based on subscription level
            if (CurrentSubscription == SubscriptionLevel.Free)
            {
                VisibleDetails = details.Take(3).ToArray(); // Free: Show first 3 lines
                RestrictedDetails = details.Skip(3).ToArray(); // Remaining lines are restricted
            }
            else if (CurrentSubscription == SubscriptionLevel.Premium)
            {
                VisibleDetails = details.Take(6).ToArray(); // Premium: Show first 6 lines
                RestrictedDetails = details.Skip(6).ToArray();
            }
            else if (CurrentSubscription == SubscriptionLevel.Pro)
            {
                VisibleDetails = details; // Pro: Show all lines
                RestrictedDetails = Array.Empty<string>(); // No restrictions for Pro users
            }
        }
    }
}





