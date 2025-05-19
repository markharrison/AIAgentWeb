using Azure;
using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Client;
using System;
using System.Collections.Concurrent;
using System.Numerics;

namespace AIAgentWeb.Pages
{
    public class LoadFilesModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly AppConfig _appconfig;
        private static bool _inUse = false;
        private static bool _completed = false;
        private static string strHtmlProgress = "";
        private static string strHtmlVectorStore = "";
        public string strFilenames = "";
        public bool AreFilesAvailable { get; private set; } = false;
        public LoadFilesModel(AppConfig appconfig, IWebHostEnvironment environment)
        {
            _environment = environment;
            _appconfig = appconfig;
        }
        public IActionResult OnGetLoadFiles()
        {
            if (_inUse)
            {
                return StatusCode(429, "Process already running");
            }

            _inUse = true;
            strHtmlProgress = "";

            // Start the long-running process in the background
            Task.Run(async () => await LongRunningProcess());

            return StatusCode(202, "Process started");
        }

        public async Task<string> DoFileUpload(AgentsClient client, string docFilePath)
        {

            try
            {
                strHtmlProgress += $"Uploading: {Path.GetFileName(docFilePath)} ... ";
                Response<AgentFile> uploadAgentFileResponse = await client!.UploadFileAsync(
                                filePath: docFilePath,
                                purpose: AgentFilePurpose.Agents);

                AgentFile uploadedAgentFile = uploadAgentFileResponse.Value;

                // Wait for file processing to complete
                while (true)
                {
                    Response<AgentFile> fileResponse = await client!.GetFileAsync(uploadedAgentFile.Id);
                    AgentFile file = fileResponse.Value;

                    if (file.Status == FileState.Error)
                    {
                        strHtmlProgress += "Failed";
                        strHtmlProgress += $"Error processing file: {file.StatusDetails}";
                    }
                    else if (file.Status == FileState.Processed)
                    {
                        strHtmlProgress += "Done<br/>";
                        strHtmlProgress += $"Id: {file.Id}, Status: {file.Status}, Purpose: {file.Purpose}<br /><br />";
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    strHtmlProgress += ".";
                }

                return uploadedAgentFile.Id;

            }
            catch (RequestFailedException ex)
            {
                strHtmlProgress += $"Error: {ex.Message}";
            }

            return string.Empty;
        }

        public async Task DoCreateVectorStore(AgentsClient client, List<string> fileIds)
        {

            try
            {
                strHtmlProgress += "Creating Vector Store ... ";
                string storeName = "vector-store-" + DateTime.Now.ToString("ddMMMyy-HHmm");
                // Create a vector store with the file and wait for it to be processed.
                Response<VectorStore> createVectorStoreResponse = await client!.CreateVectorStoreAsync(
                    fileIds: fileIds,
                    name: storeName);

                VectorStore vectorStore = createVectorStoreResponse.Value;

                // Wait for vector store processing to complete
                while (true)
                {
                    Response<VectorStore> vectorStoreResponse = await client!.GetVectorStoreAsync(vectorStore.Id);
                    vectorStore = vectorStoreResponse.Value;

                    if (vectorStore.Status == VectorStoreStatus.Completed)
                    {
                        strHtmlProgress += "Done<br/>";
                        strHtmlProgress += $"Vector Store Name: {vectorStore.Name}, Id: {vectorStore.Id}, Status: {vectorStore.Status}, FileCount: {vectorStore.FileCounts.Completed}<br><br>";
                        strHtmlVectorStore = $"Vector Store Name: {vectorStore.Name}, Id: ";
                        strHtmlVectorStore += $"<span id='IdTextToCopy'>{vectorStore.Id}</span>&nbsp;";
                        strHtmlVectorStore += "<button id='IdCopyBut' class='btn btn-success btn-sm copy-button' onclick='copyText()' aria-label='Copy'>";
                        strHtmlVectorStore += "Copy Id";
                        strHtmlVectorStore += "</button>";
                        break;
                    }

                    await Task.Delay(TimeSpan.FromSeconds(1));
                    strHtmlProgress += ".";
                }
            }
            catch (RequestFailedException ex)
            {
                strHtmlProgress += $"Error: {ex.Message}";
            }
        }

        private async Task LongRunningProcess()
        {
            _completed = false;
            strHtmlVectorStore = "";

            List<string> fileIds = new List<string>();

            AgentsClient client = new AgentsClient(_appconfig.ProjectCS, new DefaultAzureCredential());

            var appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data\\files");
            if (Directory.Exists(appDataPath))
            {
                var files = Directory.GetFiles(appDataPath);
                if (files.Length > 0)
                {
                    foreach (var file in files)
                    {
                        string strFilename = Path.GetFileName(file);

                        fileIds.Add(await DoFileUpload(client, file));
                    }
                }
            }

            if (fileIds.Count > 0)
            {
                await DoCreateVectorStore(client, fileIds);
            }
            else
            {
                strHtmlProgress += "No files uploaded<br />";
            }

            _completed = true;
            _inUse = false;
        }

        public IActionResult OnGetProgress()
        {
            if (_completed)
            {

                strHtmlProgress = $"<div class=\"alert alert-success\">Loadfiles completed - {strHtmlVectorStore}</div>" + strHtmlProgress;
                return StatusCode(200, strHtmlProgress);
            }

            return StatusCode(202, strHtmlProgress);

        }

        public IActionResult OnGetReset()
        {
            _inUse = false;
            return StatusCode(200, "Reset");
        }

        public void OnGet()
        {
            AreFilesAvailable = false;
            var appDataPath = Path.Combine(_environment.ContentRootPath, "App_Data\\files");
            if (Directory.Exists(appDataPath))
            {
                var files = Directory.GetFiles(appDataPath);
                if (files.Length > 0)
                {
                    strFilenames = "<div>Interim Storage:<br/>";
                    foreach (var file in files)
                    {
                        strFilenames += "&bull;&nbsp;" + Path.GetFileName(file) + "<br/>";
                    }
                    strFilenames += "</div><br/>";
                    AreFilesAvailable = true;
                    return;
                }
            }

            strFilenames = "<div class=\"alert alert-warning\" role=\"alert\">No files available.</div>";

        }
    }
}