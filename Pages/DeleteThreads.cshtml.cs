using AIAgentWeb.Services;
using Azure.AI.Agents.Persistent;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AIAgentWeb.Pages
{
    public class DeleteThreadsModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly PersistentAgentsClient _agentsClient;

        public DeleteThreadsModel(AgentStateService agentStateService, IWebHostEnvironment environment)
        {
            _agentsClient = agentStateService.agentsClient;
            _environment = environment;
        }
        public async Task<IActionResult> OnGetDeleteThreadsAsync()
        {

            try
            {
                await foreach (var thread in _agentsClient.Threads.GetThreadsAsync())
                {
                    // process each thread
                    var rsp = await _agentsClient.Threads.DeleteThreadAsync(thread.Id);
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