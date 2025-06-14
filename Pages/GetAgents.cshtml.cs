using AIAgentWeb.Services;
using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel.Agents;
using System.IO;
using System.Numerics;
using System.Threading;

namespace AIAgentWeb.Pages
{
    public class GetAgentsModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly IWebHostEnvironment _environment;
        private readonly AgentStateService _agentStateService;
        private readonly PersistentAgentsClient _agentsClient;

        public GetAgentsModel(AppConfig appconfig, IAntiforgery antiforgery, IWebHostEnvironment environment, AgentStateService agentStateService)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _environment = environment;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;
        }

        public async Task<IActionResult> OnPostDeleteAgentsAsync(List<string> selectedAgents)
        {

            try
            {
                if (selectedAgents != null && selectedAgents.Count > 0)
                {

                    foreach (var agent in selectedAgents)
                    {
                        await _agentsClient.Administration.DeleteAgentAsync(agent);
                    }
                    TempData["DeleteMessage"] = "Selected agent(s) deleted";
                }
            }
            catch (Exception ex)
            {
                return RedirectToPage("/Error", new { msg = ex.Message });
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetGetAgentsAsync()
        {
            string strHtml = "";
            int count = 0;
            string appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents");
            string filePath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents", "agents.txt");

            Directory.CreateDirectory(appDataPath);
            System.IO.File.WriteAllText(filePath, string.Empty);

            var token = _antiforgery.GetAndStoreTokens(HttpContext).RequestToken;

            strHtml += "<form id='IdStoreForm' method='post' action='/GetAgents?handler=DeleteAgents'>";
            strHtml += $"<input type='hidden' name='__RequestVerificationToken' value='{token}' />";
            strHtml += "<table style='border-collapse: separate; border-spacing: 10px;'>";
            strHtml += "<thead><tr><th></th><th>Agent Id</th><th>Name</th></tr></thead>";
            strHtml += "<tbody>";
            await foreach (var agent in _agentsClient.Administration.GetAgentsAsync())
            {
                count++;
                strHtml += "<tr>";
                strHtml += $"<td><input type='checkbox' name='selectedAgents' value='{agent.Id}' /></td>";
                strHtml += $"<td>{agent.Id}</td><td>{agent.Name}</td>";
                strHtml += "</tr>";

                System.IO.File.AppendAllText(filePath, agent.Id + "," + agent.Name + Environment.NewLine);

            }
            strHtml += "</tbody>";
            strHtml += "</table>";
            strHtml += "<button class='btn btn-primary' id='IdSubmitBut' type='submit'>Delete Selected</button>";
            strHtml += "</form>";

            if (count == 0)
            {
                strHtml = "<div class=\"alert alert-info\" role=\"alert\">No agents.</div>";

            }


            return Content(strHtml, "text/html");
        }

        private void InitSaveAgentIdToDisk()
        {
            string appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents");
            string filePath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents", "agents.txt");

            Directory.CreateDirectory(appDataPath);
            System.IO.File.WriteAllText(filePath, string.Empty);

        }
        private void SaveAgentIdToDisk(string agentId, string agentName)
        {
            string filePath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents", "agents.txt");
            System.IO.File.AppendAllText(filePath, agentId + "," + agentName + Environment.NewLine);
        }


        public void OnGet()
        {


        }
    }
}
