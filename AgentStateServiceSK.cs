using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Agents;
using Microsoft.SemanticKernel.Agents.AzureAI;
using System.Collections.Concurrent;
using System.IO;
using static System.Runtime.InteropServices.JavaScript.JSType;
using SKAgent = Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgent;
using SKAgentThread = Microsoft.SemanticKernel.Agents.AzureAI.AzureAIAgentThread;


#pragma warning disable SKEXP0110

namespace AIAgentWeb.Services
{
    public class AgentStateServiceSK
    {
        private readonly AgentsClient _agentsClient;
        private readonly AppConfig _appconfig;
        private readonly AgentStateService _agentStateService;

        // Use ThreadId and AgentId as dictionary keys
        private readonly ConcurrentDictionary<string, SKAgentThread> _skthreadCache = new();
        private readonly ConcurrentDictionary<string, SKAgent> _skagentCache = new();

        private readonly IWebHostEnvironment _environment;
        public AgentStateServiceSK(AppConfig appConfig, IWebHostEnvironment environment, AgentStateService agentStateService)
        {
            _environment = environment;
            _appconfig = appConfig;
            _agentStateService = agentStateService;
            _agentsClient = agentStateService.agentsClient;

        }

        public AgentsClient agentsClient
        {
            get => this._agentsClient;
        }


        public async Task<(SKAgent? agent, string? error)> GetAgentSKAsync(string agentId)
        {
            if (TryGetAgentSK(agentId, out SKAgent? cachedAgent))
            {
                return (cachedAgent, null);
            }


            try
            {
                (Azure.AI.Projects.Agent? agent, string? error) = await _agentStateService.GetAgentAsync(agentId);
                if (error != null)
                {
                    return (null, error);
                }

                SKAgent skagent = new(agent!, _agentsClient);
                if (skagent == null)
                {
                    return (null, $"Could not create AzureAIAgent for agent {agentId}.");
                }

                StoreAgentSK(agentId, skagent);

                return (skagent, null);
            }
            catch (Exception ex)
            {
                return (null, $"GetAgentAsync - An exception occurred: {ex.Message}");
            }
        }

        public void StoreAgentSK(string agentId, SKAgent skagent)
        {
            _skagentCache[agentId] = skagent;
        }

        public bool TryGetAgentSK(string agentId, out SKAgent? agent)
        {
            return _skagentCache.TryGetValue(agentId, out agent);
        }

        public async Task<(SKAgentThread? thread, string? error)> GetAgentThreadSKAsync(string threadId)
        {
            await Task.Run(() => { });

            if (TryGetAgentThreadSK(threadId, out SKAgentThread? cachedSKThread))
            {
                return (cachedSKThread, null);
            }

            return (null, $"GetAgentThreadSKAsync - Could not find thread {threadId}.");

        }

        public async Task<(SKAgentThread? thread, string? error)> CreateAgentThreadSKAsync()
        {
            try
            {

                (Azure.AI.Projects.AgentThread? thread, string? error) = await _agentStateService.CreateAgentThreadAsync();
                if (error != null)
                {
                    return (null, error);
                }

                SKAgentThread skagentThread = new AzureAIAgentThread(_agentsClient, thread!.Id);

                StoreAgentThreadSK(thread!.Id, skagentThread);

                return (skagentThread, null);
            }
            catch (Exception ex)
            {
                return (null, $"CreateAgentThreadSKAsync - An exception occurred: {ex.Message}");
            }
        }

        public void StoreAgentThreadSK(string threadId, SKAgentThread thread)
        {
            _skthreadCache[threadId] = thread;

        }

        public bool TryGetAgentThreadSK(string threadId, out SKAgentThread? skthread)
        {
            return _skthreadCache.TryGetValue(threadId, out skthread);
        }

    }
}
