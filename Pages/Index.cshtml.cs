using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{
    public class IndexModel : PageModel
    {
 //       public string JoinValue { get; private set; } = string.Empty;

        public IndexModel( )
        {
 
        }

        public void OnGet()
        {
 //           JoinValue = Request.Query.FirstOrDefault(q => string.Equals(q.Key, "join", StringComparison.OrdinalIgnoreCase)).Value.ToString();


        }
    }
}
