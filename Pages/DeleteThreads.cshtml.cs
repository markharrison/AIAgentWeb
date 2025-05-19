using AIAgentWeb.Services;
using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using Microsoft.SemanticKernel.Agents;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Numerics;

namespace AIAgentWeb.Pages
{
    public class DeleteThreadsModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AgentsClient _agentsClient;

        //private static bool _inUse = false;
        //private static bool _completed = false;
        //private static string strHtmlProgress = "";
        //private static string strHtmlVectorStore = "";
        //public string strFilenames = "";
        //public bool AreFilesAvailable { get; private set; } = false;
        public DeleteThreadsModel(AgentStateService agentStateService, IWebHostEnvironment environment)
        {
            _agentsClient = agentStateService.agentsClient;
            _environment = environment;
        }
        public async Task<IActionResult> OnGetDeleteThreadsAsync()
        {

            try
            {
                var threads = await _agentsClient.GetThreadsAsync();

                if (threads.Value.Data.Count > 0)
                {
                    foreach (var thread in threads.Value.Data)
                    {
                        var rsp = await _agentsClient.DeleteThreadAsync(thread.Id);
                    }
                }

            }
            catch (Exception ex)
            {
                return StatusCode(500,ex.Message);
            }

            return StatusCode(200);

        }


        public void OnGet()
        {


        }
    }
}