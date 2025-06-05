using AIAgentWeb.Services;
using Azure;
using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AIAgentWeb.Pages
{
    public class ChatModel : PageModel
    {
        private readonly AppConfig _appconfig;
        private readonly IAntiforgery _antiforgery;
        private readonly AgentStateService _agentStateService;
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


        public ChatModel(AppConfig appconfig, IAntiforgery antiforgery, AgentStateService agentStateService, IWebHostEnvironment environment)
        {
            _antiforgery = antiforgery;
            _appconfig = appconfig;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;
            _environment = environment;

        }

        public async Task<IActionResult> OnPostChatAsync()
        {

            DateTime currentTime = DateTime.Now;

            ModelState.Remove(nameof(AgentId));

            try
            {
                (PersistentAgent? agent, string? error) = await _agentStateService.GetAgentAsync(AgentId!);
                if (error != null)
                {
                    return StatusCode(400, error);
                }

                (PersistentAgentThread? thread, string? error2) = await _agentStateService.GetAgentThreadAsync(ThreadId!);
                if (error2 != null)
                {
                    return StatusCode(400, error2);
                }

                Response<PersistentThreadMessage> messageResponse = await _agentsClient.Messages.CreateMessageAsync(ThreadId, MessageRole.User, Ask);

                if (messageResponse.GetRawResponse().Status != 200)
                {
                    return StatusCode(400, $"CreateMessageAsync - Error {messageResponse.GetRawResponse().Status}");
                }

                PersistentThreadMessage message = messageResponse.Value;

                Response<ThreadRun> runResponse = await _agentsClient.Runs.CreateRunAsync(thread, agent);
                if (runResponse.GetRawResponse().Status != 200)
                {
                    return StatusCode(400, $"CreateRunAsync - Error {runResponse.GetRawResponse().Status}");

                }

                ThreadRun threadRun = runResponse.Value;

                do
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(200));
                    runResponse = await _agentsClient.Runs.GetRunAsync(ThreadId, threadRun.Id);
                    if (runResponse.GetRawResponse().Status != 200)
                    {
                        return StatusCode(400, $"GetRunAsync - Error {runResponse.GetRawResponse().Status}");

                    }

                    threadRun = runResponse.Value;

                }
                while (threadRun.Status == RunStatus.Queued
                    || threadRun.Status == RunStatus.InProgress);


                PersistentThreadMessage? threadMessage = null;

                // Bug - limit doesnt work https://github.com/Azure/azure-sdk-for-net/issues/50378

                await foreach (var msg in _agentsClient.Messages.GetMessagesAsync(ThreadId, limit: 1))
                {
                    threadMessage = msg;
                    break;
                }

                if (threadMessage == null)
                {
                    return StatusCode(400, "No messages found in the thread.");
                }

                var annotationsList = new List<(string Text, string Filename)>();
                foreach (MessageContent contentItem in threadMessage.ContentItems)
                {

                    switch (contentItem)
                    {
                        case MessageTextContent messagetextItem:

                            strHtml += $"{messagetextItem.Text.Replace("\r\n", "<br>").Replace("\n", "<br>").Replace("\r", "<br>")}";

                            foreach (MessageTextAnnotation annotation in messagetextItem.Annotations)
                            {

                                switch (annotation)
                                {
                                    case MessageTextFileCitationAnnotation annot:

                                        if (!annotationsList.Any(a => a.Text == annot.Text))
                                        {
                                            var filename = await _agentStateService.GetAgentFileNameAsync(annot.FileId);

                                            annotationsList.Add((annot.Text, filename!));
                                        }

                                        break;

                                    case MessageTextUriCitationAnnotation annot:

                                        if (!annotationsList.Any(a => a.Text == annot.Text))
                                        {
                                            var filename = annot.UriCitation.Uri;

                                            annotationsList.Add((annot.Text, filename!));
                                        }

                                        break;

                                    default:
                                        strHtml += "[[unknown annotation type]]";
                                        break;
                                }

                            }

                            break;

                        case MessageImageFileContent imageFileItem:

                            var fileId = imageFileItem.FileId;

                            PersistentAgentFileInfo agentFile = await _agentsClient.Files.GetFileAsync(fileId);
                            var fileName = agentFile.Filename;

                            BinaryData fileData = await _agentsClient.Files.GetFileContentAsync(fileId);

                            var appDataPath = Path.Combine(_environment.WebRootPath, "temp");
                            if (!Directory.Exists(appDataPath))
                            {
                                Directory.CreateDirectory(appDataPath);
                            }

                            string filePath = Path.Combine(appDataPath, fileName);

                            await System.IO.File.WriteAllBytesAsync(filePath, fileData.ToArray());

                            strHtml += $"<img style='max-width: 75%; height: auto;' src='/temp/{fileName}' ><br />";

                            break;
                        default:
                            strHtml += "[[unknown contentItem type]]";
                            break;


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
                Console.WriteLine($"Chat response time: {timeTaken.TotalMilliseconds} ms");

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

            (PersistentAgent? agent, string? error) = await _agentStateService.GetAgentAsync(AgentId!);
            if (error != null)
            {
                TempData["ErrorMessage"] = error;
                return Page();
            }

            AgentDetails = $"<b>{agent?.Name}</b>";

            if (agent?.Tools != null)
            {
                AgentDetails += "  Tools: ";

                foreach (ToolDefinition tool in agent?.Tools!)
                {
                    AgentDetails += tool.GetType().Name.ToString() + "; ";
                }
            }

            (PersistentAgentThread? thread, string? error2) = await _agentStateService.CreateAgentThreadAsync();
            if (error2 != null)
            {
                TempData["ErrorMessage"] = error2;
                return Page();
            }

            ThreadId = thread?.Id;
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
