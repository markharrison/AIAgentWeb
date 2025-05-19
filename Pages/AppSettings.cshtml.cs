using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AIAgentWeb.Pages
{
    public class AppSettingsModel : PageModel
    {
        private readonly AppConfig _appconfig;

        public AppSettingsModel(AppConfig appconfig)
        {
            _appconfig = appconfig;
        }

        [BindProperty]
        [Required(ErrorMessage = "Project Connection String is required.")]
        public string? ProjectCS { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "VectorStoreId is required.")]
        public string? VectorStoreId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "AgentId is required.")]
        public string? AgentId { get; set; }

        public void OnGet()
        {

            ProjectCS = _appconfig.ProjectCS;
            VectorStoreId = _appconfig.VectorStoreId;
            AgentId = _appconfig.AgentId;
        }

        public IActionResult OnPost()
        {
            ProjectCS = ProjectCS?.Trim();
            AgentId = AgentId?.Trim();
            VectorStoreId = VectorStoreId?.Trim();

            if (string.IsNullOrEmpty(ProjectCS))
            {
                ModelState.AddModelError("ProjectCS", "Project Connection String is required.");
            }

            if (string.IsNullOrEmpty(AgentId))
            {
                ModelState.AddModelError("AgentId", "Agent Id is required.");
            }
            else if (!Regex.IsMatch(AgentId, @"^asst_[a-zA-Z0-9_-]+$"))
            {
                ModelState.AddModelError("AgentId", "Agent Id must start with 'asst_' and can only contain letters, numbers, underscores, or dashes.");
            }

            if (string.IsNullOrEmpty(VectorStoreId))
            {
                ModelState.AddModelError("VectorStoreId", "Vector Store Id is required.");
            }
            else if (!Regex.IsMatch(VectorStoreId, @"^vs_[a-zA-Z0-9_-]+$"))
            {
                ModelState.AddModelError("VectorStoreId", "Vector Store Id must start with 'vs_' and can only contain letters, numbers, underscores, or dashes.");
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }





            _appconfig.ProjectCS = ProjectCS!;
            _appconfig.VectorStoreId = VectorStoreId!;
            _appconfig.AgentId = AgentId!;

            TempData["SaveMessage"] = "Settings saved";
            return RedirectToPage();

        }
    }
}