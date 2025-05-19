using AIAgentWeb.Services;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.Numerics;
using System.Text.RegularExpressions;

namespace AIAgentWeb.Pages
{
    public class CreateAgentModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
        private readonly AgentsClient _agentsClient;

        private float fTemperature = 0;
        private float fTopP = 0;

        public string strHtml = "";

        [BindProperty]
        public bool FileSearchTool { get; set; }

        [BindProperty]
        public string VectorStoreId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Agent Name is required.")]
        public string AgentName { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Model Deployment is required.")]
        public string ModelDeployment { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Agent Instructions is required.")]
        public string AgentInstructions { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "Temperature is required.")]
        public string Temperature { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "TopP is required.")]
        public string TopP { get; set; }

        public CreateAgentModel(AppConfig appconfig, IAntiforgery antiforgery, AgentStateService agentStateService)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;
            VectorStoreId = _appconfig.VectorStoreId;

            AgentName = "Agent-Name-" + DateTime.Now.ToString("ddMMMyy-HHmm");
            ModelDeployment = "gpt-4o-mini";
            AgentInstructions = "You are a helpful agent. Professional tone.";
            Temperature = "0.5";
            TopP = "0.5";

        }

        public async Task<IActionResult> OnPostAsync()
        {

            List<ToolDefinition> toolDefinitions = new List<ToolDefinition>();
            ToolResources? toolResources = null;

            AgentName = (AgentName?.Trim()) ?? "";
            if (string.IsNullOrEmpty(AgentName))
            {
                ModelState.AddModelError("AgentName", "Agent Name is required.");
            }

            ModelDeployment = (ModelDeployment?.Trim()) ?? "";
            if (string.IsNullOrEmpty(ModelDeployment))
            {
                ModelState.AddModelError("ModelDeployment", "ModelDeployment is required.");
            }

            AgentInstructions = (AgentInstructions?.Trim()) ?? "";
            if (string.IsNullOrEmpty(AgentInstructions))
            {
                ModelState.AddModelError("AgentInstructions", "AgentInstructions is required.");
            }

            Temperature = (Temperature?.Trim()) ?? "";
            if (string.IsNullOrEmpty(Temperature))
            {
                ModelState.AddModelError("Temperature", "Temperature is required.");
            }
            else if (!float.TryParse(Temperature, out float temp))
            {
                ModelState.AddModelError("Temperature", "Temperature must be a number.");
            }
            else if (temp < 0 || temp > 1)
            {
                ModelState.AddModelError("Temperature", "Temperature must be between 0 and 1.");
            }
            else
            {
                fTemperature = temp;
            }

            TopP = (TopP?.Trim()) ?? "";
            if (string.IsNullOrEmpty(TopP))
            {
                ModelState.AddModelError("TopP", "TopP is required.");
            }
            else if (!float.TryParse(TopP, out float temp))
            {
                ModelState.AddModelError("TopP", "TopP must be a number.");
            }
            else if (temp < 0 || temp > 1)
            {
                ModelState.AddModelError("TopP", "TopP must be between 0 and 1.");
            }
            else
            {
                fTopP = temp;
            }



            if (FileSearchTool)
            {
                VectorStoreId = VectorStoreId.Trim();

                if (string.IsNullOrEmpty(VectorStoreId))
                {
                    ModelState.AddModelError("VectorStoreId", "Vector Store Id is required.");
                    return Page();
                }
                else if (!Regex.IsMatch(VectorStoreId, @"^vs_[a-zA-Z0-9_-]+$"))
                {
                    ModelState.AddModelError("VectorStoreId", "Vector Store Id must start with 'vs_' and can only contain letters, numbers, underscores, or dashes.");
                    return Page();
                }
                else
                {
                    FileSearchToolResource fileSearchToolResource = new FileSearchToolResource();
                    fileSearchToolResource.VectorStoreIds.Add(VectorStoreId);
                    toolDefinitions.Add(new FileSearchToolDefinition());
                    toolResources = new ToolResources() { FileSearch = fileSearchToolResource };
                }
            }


            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                Response<Agent> agentResponse = await _agentsClient.CreateAgentAsync(
                    model: ModelDeployment,
                    name: AgentName,
                    instructions: AgentInstructions,
                    temperature: fTemperature,
                    topP: fTopP,
                    tools: toolDefinitions,
                    toolResources: toolResources
                    );

                if (agentResponse.GetRawResponse().Status != 200)
                {
                    TempData["ErrorMessage"] = $"An error {agentResponse.GetRawResponse().Status} occurred while creating the agent.";
                    return Page();
                }

                Agent agent = agentResponse.Value;

                var strId = $"<span id='IdTextToCopy'>{agent.Id}</span>&nbsp;";
                strId += "<button id='IdCopyBut' class='btn btn-success btn-sm copy-button' onclick='copyText()' aria-label='Copy'>";
                strId += "Copy Id";
                strId += "</button>";

                TempData["CreateMessage"] = $"Created agent - Name: {agent.Name}, Id: " + strId;

            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message.Substring(0, 160);
                return Page();
            }


            return RedirectToPage();
        }

        public void OnGet()
        {



        }
    }
}


