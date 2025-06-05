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
        [Required(ErrorMessage = "Project Endpoint is required.")]
        public string? ProjectEndPoint{ get; set; }

        [BindProperty]
        [Required(ErrorMessage = "VectorStoreId is required.")]
        public string? VectorStoreId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "AgentId is required.")]
        public string? AgentId { get; set; }

        public void OnGet()
        {
            ProjectEndPoint = _appconfig.ProjectEndpoint;
            VectorStoreId = _appconfig.VectorStoreId;
            AgentId = _appconfig.AgentId;
        }

        public IActionResult OnPost()
        {
            ProjectEndPoint = ProjectEndPoint?.Trim();
            AgentId = AgentId?.Trim();
            VectorStoreId = VectorStoreId?.Trim();

            if (string.IsNullOrEmpty(ProjectEndPoint))
            {
                ModelState.AddModelError("ProjectEndpoint", "Project Endpoint is required.");
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

            _appconfig.ProjectEndpoint = ProjectEndPoint!;
            _appconfig.VectorStoreId = VectorStoreId!;
            _appconfig.AgentId = AgentId!;

            TempData["SaveMessage"] = "Settings saved";
            return RedirectToPage();

        }
    }
}