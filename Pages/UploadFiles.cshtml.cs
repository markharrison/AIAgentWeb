using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{

    public class UploadFilesModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;

        public UploadFilesModel(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public string strFilenames = "";

        [BindProperty]
        public IFormFileCollection? Files { get; set; }

        [BindProperty]
        public bool DeleteExistingFiles { get; set; }

        public void OnGet()
        {
            var appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data\\files");
            if (Directory.Exists(appDataPath))
            {
                var files = Directory.GetFiles(appDataPath);
                if (files.Length > 0)
                {
                    strFilenames = "<div>Interim Storage:<br/>";
                    foreach (var file in files)
                    {
                        strFilenames += "&bull;&nbsp;" + Path.GetFileName(file) + "<br/>";
                    }
                    strFilenames += "</div><br/>";
                    return;
                }
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                if (Files == null || Files.Count == 0)
                {
                    return Page();
                }

                var appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data\\files");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }
                else if (DeleteExistingFiles)  
                {
                    var allFiles = Directory.GetFiles(appDataPath);
                    foreach (var file in allFiles)
                    {
                        System.IO.File.Delete(file);
                    }
                }

                foreach (var file in Files)
                {
                    var filePath = Path.Combine(appDataPath, file.FileName);

                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                }

                TempData["UploadMessage"] = "Files have been uploaded successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"An error occurred while uploading the files: {ex.Message}");
                return Page();
            }
        }
    }
}



