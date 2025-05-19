using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace AIAgentWeb.Pages
{
    public class UserSettingsModel : PageModel
    {

        public UserSettingsModel()
        {

        }

        [BindProperty]
        [Required]
        public string? Theme { get; set; }

        public void OnGet()
        {
            var themeCookie = HttpContext.Request.Cookies["mhtheme"];

            Theme = (!string.IsNullOrEmpty(themeCookie)) ? themeCookie : "Dark";

        }

        public IActionResult OnPost()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }   

            var options = new CookieOptions
            {
                Expires = DateTime.Now.AddYears(1)
            };

            HttpContext.Response.Cookies.Append("mhtheme", Theme ?? string.Empty, options);

            TempData["SaveMessage"] = "Settings saved";
            return RedirectToPage();
        }
    }
}