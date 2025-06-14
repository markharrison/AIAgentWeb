using Azure.AI.Agents.Persistent;
using Azure.Identity;
using System.Collections.Concurrent;

namespace AIAgentWeb.Services
{
    public class AgentStateService
    {
        private readonly PersistentAgentsClient _agentsClient;
//        private readonly AgentsClient _agentsClient;
        private readonly AppConfig _appconfig;

        // Use ThreadId and AgentId as dictionary keys
        private readonly ConcurrentDictionary<string, PersistentAgentThread> _threadCache = new();
        private readonly ConcurrentDictionary<string, PersistentAgent> _agentCache = new();
        private readonly ConcurrentDictionary<string, string> _filePathCache = new();

        private readonly IWebHostEnvironment _environment;
        public AgentStateService(AppConfig appConfig, IWebHostEnvironment environment)
        {
            _environment = environment;
            _appconfig = appConfig;
            try
            { 
                 _agentsClient = new(appConfig.ProjectEndpoint, new DefaultAzureCredential());
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize AgentsClient", ex);
            }

            try
            {
                // To warm up the connection 
                //  Response<Agent> agentResponse = _agentsClient.GetAgent("asst_Junk");
                var agentResponse = _agentsClient!.Administration.GetAgent("asst_Junk");
            }
            catch 
            {
                // ignore 
            }

            DeleteAgentThreads();

            DeleteTemp();

            try
            {
                var agentResponse = _agentsClient!.Files.GetFiles();
                var files = agentResponse.Value;

                foreach (var file in files)
                {
                    StoreAgentFileName(file.Id, file.Filename);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to initialize AgentsClient 2", ex);
            }

        }

        public PersistentAgentsClient agentsClient
        {
            get => this._agentsClient;
        }

        public async Task<string?> GetAgentFileNameAsync(string? fileId)
        {
            if (fileId == null)
                return "";

            string? fileName = await GetAgentFilePathAsync(fileId);

            return Path.GetFileName(fileName);

        }


        public async Task<string?> GetAgentFilePathAsync(string fileId)
        {

            if (_filePathCache.TryGetValue(fileId, out string? fileName))
            {
                return fileName;
            }

            PersistentAgentFileInfo agentFile = await agentsClient.Files.GetFileAsync(fileId);

            StoreAgentFileName(fileId, agentFile.Filename);

            return agentFile.Filename;
        }


        public void StoreAgentFileName(string fileId, string fileName)
        {
            _filePathCache[fileId] = fileName;

        }


        public async Task<(PersistentAgent? agent, string? error)> GetAgentAsync(string agentId)
        {
            if (TryGetAgent(agentId, out PersistentAgent? cachedAgent))
            {
                return (cachedAgent, null);
            }

            try
            {
                var agentResponse = await _agentsClient.Administration.GetAgentAsync(agentId);
                if (agentResponse.GetRawResponse().Status != 200)
                {
                    return (null, $"GetAgentAsync - An error {agentResponse.GetRawResponse().Status} occurred while getting the agent.");
                }

                PersistentAgent agent = agentResponse.Value;
                StoreAgent(agentId, agent);

                return (agent, null);
            }
            catch (Exception ex)
            {
                return (null, $"GetAgentAsync - An exception occurred: {ex.Message}");
            }
        }

        public void StoreAgent(string agentId, PersistentAgent agent)
        {
            _agentCache[agentId] = agent;
        }

        public bool TryGetAgent(string agentId, out PersistentAgent? agent)
        {
            return _agentCache.TryGetValue(agentId, out agent);
        }

        public async Task<(PersistentAgentThread? thread, string? error)> GetAgentThreadAsync(string threadId)
        {
            await Task.Run(() => { });

            if (TryGetAgentThread(threadId, out PersistentAgentThread? cachedThread))
            {
                return (cachedThread, null);
            }

            return (null, $"GetThreadAsync - Could not find thread {threadId}.");

        }

        public async Task<(PersistentAgentThread? thread, string? error)> CreateAgentThreadAsync()
        {
            try
            {
                var threadResponse = await _agentsClient.Threads.CreateThreadAsync();
                if (threadResponse.GetRawResponse().Status != 200)
                {
                    return (null, $"CreateAgentThreadAsync - An error {threadResponse.GetRawResponse().Status} occurred while creating the thread.");
                }

                PersistentAgentThread thread = threadResponse.Value;
                StoreAgentThread(thread.Id, thread);

                return (thread, null);
            }
            catch (Exception ex)
            {
                return (null, $"CreateAgentThreadAsync - An exception occurred: {ex.Message}");
            }
        }

        public void StoreAgentThread(string threadId, PersistentAgentThread thread)
        {
            _threadCache[threadId] = thread;
        }

        public void DeleteAgentThreads()
        {
            try
            {
                var oneHourAgo = DateTime.UtcNow.AddHours(-1);

                var threads = _agentsClient.Threads.GetThreads();

                foreach (var thread in threads)
                {
                    if (thread.CreatedAt < oneHourAgo)
                    {
                        var rsp = _agentsClient.Threads.DeleteThread(thread.Id);
                    }
                }

            }
            catch
            {
            }

        }


        public bool TryGetAgentThread(string threadId, out PersistentAgentThread? thread)
        {
            return _threadCache.TryGetValue(threadId, out thread);
        }

        public void DeleteTemp()
        {
            var tempPath = Path.Combine(_environment.WebRootPath, "temp");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
            }
        }

    }
}
