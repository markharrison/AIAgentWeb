using AIAgentWeb.Services;
using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.ChatCompletion;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using SKAgent = Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgent;
using SKAgentThread = Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgentThread;

#pragma warning disable SKEXP0110


namespace AIAgentWeb.Pages
{
    public class ChatSKModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
        private readonly AgentStateServiceSK _agentStateServiceSK;
        private readonly PersistentAgentsClient _agentsClient;
        private readonly IWebHostEnvironment _environment;
        public string strHtml = "";

        public enum FormType
        {
            GetThread,
            Chat
        }
        [BindProperty]
        public FormType CurrentForm { get; set; }


        [BindProperty]
        [Required(ErrorMessage = "Agent Id is required.")]
        public string? AgentId { get; set; }

        public List<SelectListItem>? AgentList { get; set; }

        [BindProperty]
        public string? ThreadId { get; set; }

        [BindProperty]
        public string? AgentDetails { get; set; }

        [BindProperty]
        public string? Ask { get; set; }

        public ChatSKModel(AppConfig appconfig, IAntiforgery antiforgery, AgentStateService agentStateService, 
            AgentStateServiceSK agentStateServiceSK, IWebHostEnvironment environment)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _agentStateService = agentStateService;
            _agentStateServiceSK = agentStateServiceSK;
            _agentsClient = agentStateService.agentsClient;
            _environment = environment;
        }

        public async Task<IActionResult> OnPostChatAsync()
        {
            DateTime currentTime = DateTime.Now;

            ModelState.Remove(nameof(AgentId));

            try
            {
                (SKAgent? skagent, string? error) = await _agentStateServiceSK.GetAgentSKAsync(AgentId!);
                if (error != null)
                {
                    return StatusCode(400, error);
                }

                (SKAgentThread? skthread, string? error2) = await _agentStateServiceSK.GetAgentThreadSKAsync(ThreadId!);
                if (error2 != null)
                {
                    return StatusCode(400, error2);
                }

                ChatMessageContent message = new(AuthorRole.User, Ask);

                List<AnnotationContent> footnotes = [];

                await foreach (ChatMessageContent response in skagent!.InvokeAsync(message, skthread))
                {
                    footnotes.AddRange(response.Items.OfType<AnnotationContent>());

                    strHtml += response.Content?.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>");

                }

                var annotationsList = new List<(string Text, string Filename)>();
                foreach (AnnotationContent footnote in footnotes)
                {
                    if (!annotationsList.Any(a => a.Text == footnote.Label))
                    {
                        var filename = await _agentStateService.GetAgentFileNameAsync(footnote.ReferenceId!);
                        annotationsList.Add((footnote.Label!, filename!));

                    }
                }

                if (annotationsList.Count > 0)
                {
                    string strAnnotHtml = "";
                    string lastFileName = "";
                    annotationsList = annotationsList.OrderBy(a => a.Filename).ToList();

                    int idx = 1;

                    foreach (var annotation in annotationsList)
                    {
                        strHtml = strHtml.Replace(annotation.Text, $"<b>[{idx}]</b>");

                        if (idx > 1 && annotation.Filename != lastFileName)
                        {
                            strAnnotHtml += $" {lastFileName}<br />";
                        }

                        strAnnotHtml += $"<b>[{idx}]</b> ";

                        idx++;
                        lastFileName = annotation.Filename;

                    }

                    strAnnotHtml += $" {lastFileName}<br />";

                    strHtml += "<hr/>" + strAnnotHtml;
                }

                strHtml = AddAnchorTags(strHtml);

                TimeSpan timeTaken = DateTime.Now - currentTime;
                Console.WriteLine($"Chat SK response time: {timeTaken.TotalMilliseconds} ms");
            }

            catch (Exception ex)
            {
                return StatusCode(500, $"Exception: {ex.Message}");
            }

            return StatusCode(200, strHtml);

        }

        public string AddAnchorTags(string input)
        {
            string pattern = @"(?i)(http[s]?://[^\s<>"",;:]+)";
            string result = Regex.Replace(input, pattern, match =>
            {
                string url = match.Value;
                if (url.EndsWith("."))
                {
                    url = url.Substring(0, url.Length - 1);
                }

                return $"<a target=\"_blank\" href=\"{url}\">{url}</a>";
            });

            return result;
        }

        public async Task<IActionResult> OnPostAsync()
        {

            ModelState.Remove(nameof(Ask));

            AgentId = AgentId?.Trim();
            if (string.IsNullOrEmpty(AgentId))
            {
                ModelState.AddModelError("AgentId", "Agent Id is required.");
                //return Page();
            }
            else if (!Regex.IsMatch(AgentId, @"^asst_[a-zA-Z0-9_-]+$"))
            {
                ModelState.AddModelError("AgentId", "Agent Id must start with 'asst_' and can only contain letters, numbers, underscores, or dashes.");
                //return Page();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            (SKAgent? skagent, string? error) = await _agentStateServiceSK.GetAgentSKAsync(AgentId!);
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return Page();
            }

            (SKAgentThread? skthread, string? error2) = await _agentStateServiceSK.CreateAgentThreadSKAsync();
            if (error2 != null)
            {
                TempData["ErrorMessage"] = error2;
                return Page();
            }

            AgentDetails = $"<b>{skagent?.Name}</b>";

            if (skagent?.Definition.Tools != null)
            {
                AgentDetails += "  Tools: ";

                foreach (ToolDefinition tool in skagent?.Definition.Tools!)
                {
                    AgentDetails += tool.GetType().Name.ToString() + "; ";
                }
            }

            ThreadId = skthread?.Id;
            CurrentForm = FormType.Chat;

            return Page();

        }

        public void OnGet()
        {
            Ask = "";
            CurrentForm = FormType.GetThread;
            strHtml = "";
            AgentId = "";

            AgentList = new List<SelectListItem>
                {
                    new SelectListItem { Value = "", Text = "-- Select an Agent --", Disabled = true, Selected = true }
                };

            string filePath = Path.Combine(_environment.ContentRootPath, "App_Data", "agents", "agents.txt");
            if (System.IO.File.Exists(filePath))
            {
                foreach (var line in System.IO.File.ReadLines(filePath))
                {
                    // Split the line into AgentId and AgentName
                    var parts = line.Split(',');
                    if (parts.Length == 2)
                    {
                        var agentId = parts[0].Trim();
                        var agentName = parts[1].Trim();

                        // Add to the AgentList
                        AgentList.Add(new SelectListItem { Value = agentId, Text = $"{agentName} - {agentId}" });
                    }
                }
            }
        }

    }

}
