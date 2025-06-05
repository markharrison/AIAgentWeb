using AIAgentWeb.Services;
using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{
    public class GetFilesModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
        private readonly PersistentAgentsClient _agentsClient;

        public GetFilesModel(AppConfig appconfig, IAntiforgery antiforgery, AgentStateService agentStateService)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;
        }

        public async Task<IActionResult> OnPostDeleteFilesAsync(List<string> selectedFiles)
        {

            try
            {
                if (selectedFiles != null && selectedFiles.Count > 0)
                {

                    foreach (var fileId in selectedFiles)
                    {
                        await _agentsClient.Files.DeleteFileAsync(fileId);
                    }
                    TempData["DeleteMessage"] = "Selected files(s) deleted";
                }
            }
            catch (Exception ex)
            {
                TempData["DeleteMessage"] = $"Exception {ex.Message}";
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetGetFilesAsync()
        {

            string strHtml = "";

            var fileStoreList = new List<(string FileId, string StoreId)>();

            await foreach (var store in _agentsClient.VectorStores.GetVectorStoresAsync())
            {
                await foreach (var f in _agentsClient.VectorStores.GetVectorStoreFilesAsync(store.Id))
                {
                    fileStoreList.Add((f.Id, store.Id));
                }
            }

            var files = await _agentsClient.Files.GetFilesAsync();

            if (files.Value.Count == 0)
            {
                strHtml += "<div class=\"alert alert-info\" role=\"alert\">No files.</div>";
            }
            else
            {
                var token = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

                strHtml += "<form id='IdFileForm' method='post' action='/GetFiles?handler=DeleteFiles'>";
                strHtml += $"<input type='hidden' name='__RequestVerificationToken' value='{token}' />";
                strHtml += "<table style='border-collapse: separate; border-spacing: 10px;'>";
                strHtml += "<thead><tr><th></th><th>File Id</th><th>Name</th><th>Status</th><th>Vector Store Id</th></tr></thead>";
                strHtml += "<tbody>";
                foreach (var file in files.Value)
                {
                    strHtml += "<tr style=\"vertical-align: top;\">";
                    strHtml += $"<td><input type='checkbox' name='selectedFiles' value='{file.Id}' /></td>";
                    strHtml += $"<td>{file.Id}</td><td>{Path.GetFileName(file.Filename)}</td><td>{file.Status}</td>";

                    strHtml += "<td>";

                    var result = fileStoreList.FirstOrDefault(x => x.FileId == file.Id);
                    if (result != default)
                    {
                        strHtml += result.StoreId;
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
