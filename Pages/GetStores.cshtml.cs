using AIAgentWeb.Services;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.Numerics;

namespace AIAgentWeb.Pages
{
    public class GetStoresModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
        private readonly AgentsClient _agentsClient;

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

                        var files = await _agentsClient.GetVectorStoreFilesAsync(storeId);

                        foreach (var file in files.Value.Data)
                        {
                            await _agentsClient.DeleteFileAsync(file.Id);
                        }

                        await _agentsClient.DeleteVectorStoreAsync(storeId);
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

            var vectorStores = await _agentsClient.GetVectorStoresAsync();

            if (vectorStores.Value.Data.Count == 0)
            {
                strHtml += "<div class=\"alert alert-info\" role=\"alert\">No vector stores.</div>";
            }
            else
            {
                var token = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

                strHtml += "<form id='IdStoreForm' method='post' action='/GetStores?handler=DeleteStores'>";
                strHtml += $"<input type='hidden' name='__RequestVerificationToken' value='{token}' />";
                strHtml += "<table style='border-collapse: separate; border-spacing: 10px;'>";
                strHtml += "<thead><tr><th></th><th>Vector Store Id</th><th>Name</th><th>Status</th><th>FileCount</th><th>File Ids</th></tr></thead>";
                strHtml += "<tbody>";
                foreach (var store in vectorStores.Value.Data)
                {
                    strHtml += "<tr style=\"vertical-align: top;\">";
                    strHtml += $"<td><input type='checkbox' name='selectedStores' value='{store.Id}' /></td>";
                    strHtml += $"<td>{store.Id}</td><td>{store.Name}</td><td>{store.Status}</td><td>{store.FileCounts.Completed}</td>";

                    strHtml += "<td>";                 
                    var files = await _agentsClient.GetVectorStoreFilesAsync(store.Id);
                    foreach (var file in files.Value.Data)
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
            }

            return Content(strHtml, "text/html");
        }


        public void OnGet()
        {


        }
    }
}
