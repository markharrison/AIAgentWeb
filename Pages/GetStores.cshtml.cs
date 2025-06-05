using AIAgentWeb.Services;
using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{
    public class GetStoresModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
        private readonly PersistentAgentsClient _agentsClient;

        public GetStoresModel(AppConfig appconfig, IAntiforgery antiforgery, AgentStateService agentStateService)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;
        }

        public async Task<IActionResult> OnPostDeleteStoresAsync(List<string> selectedStores)
        {
            try
            {
                if (selectedStores != null && selectedStores.Count > 0)
                {

                    foreach (var storeId in selectedStores)
                    {

                        await foreach (var file in _agentsClient.VectorStores.GetVectorStoreFilesAsync(storeId))
                        {
                            await _agentsClient.Files.DeleteFileAsync(file.Id);
                        }

                        await _agentsClient.VectorStores.DeleteVectorStoreAsync(storeId);
                    }
                    TempData["DeleteMessage"] = "Selected store(s) deleted";
                }
            }
            catch (Exception ex)
            {
                TempData["DeleteMessage"] = $"Exception {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetGetStoresAsync()
        {
            string strHtml = "";
            int count = 0;

            var token = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

            strHtml += "<form id='IdStoreForm' method='post' action='/GetStores?handler=DeleteStores'>";
            strHtml += $"<input type='hidden' name='__RequestVerificationToken' value='{token}' />";
            strHtml += "<table style='border-collapse: separate; border-spacing: 10px;'>";
            strHtml += "<thead><tr><th></th><th>Vector Store Id</th><th>Name</th><th>Status</th><th>FileCount</th><th>File Ids</th></tr></thead>";
            strHtml += "<tbody>";

            await foreach (var store in _agentsClient.VectorStores.GetVectorStoresAsync())
            {
                count++;

                strHtml += "<tr style=\"vertical-align: top;\">";
                strHtml += $"<td><input type='checkbox' name='selectedStores' value='{store.Id}' /></td>";
                strHtml += $"<td>{store.Id}</td><td>{store.Name}</td><td>{store.Status}</td><td>{store.FileCounts.Completed}</td>";
                strHtml += "<td>";

                await foreach (var file in _agentsClient.VectorStores.GetVectorStoreFilesAsync(store.Id))
                {
                    strHtml += file.Id + "<br/>";
                }
                strHtml += "</td>";

                strHtml += "</tr>";
            }
            strHtml += "</tbody>";
            strHtml += "</table>";
            strHtml += "<button class='btn btn-primary' id='IdSubmitBut' type='submit'>Delete Selected</button>";
            strHtml += "</form>";

            if (count == 0)
            {
                strHtml = "<div class=\"alert alert-info\" role=\"alert\">No vector stores.</div>";
            }

            return Content(strHtml, "text/html");
        }


        public void OnGet()
        {


        }
    }
}
