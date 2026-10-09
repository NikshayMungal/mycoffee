using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Services; // <-- This fixes the IDocumentStore error

namespace CoffeeNChill.Functions
{
    public class DocumentFunctions
    {
        private readonly IDocumentStore _documentStore;
        private readonly ILogger<DocumentFunctions> _logger;

        public DocumentFunctions(IDocumentStore documentStore, ILogger<DocumentFunctions> logger)
        {
            _documentStore = documentStore;
            _logger = logger;
        }

        [Function("UploadStaffDocument")]
        public async Task<IActionResult> UploadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "staff/documents/{fileName}")] HttpRequest req,
            string fileName)
        {
            _logger.logInformation($"Uploading staff document: {fileName}");

            if (req.Body == null || req.Body.Length == 0)
            {
                return new BadRequestObjectResult("Please provide a file body to upload.");
            }

            string contentType = req.ContentType ?? "application/octet-stream";

            await _documentStore.UploadDocumentAsync(fileName, req.Body, contentType);

            return new OkObjectResult(new { message = $"Document '{fileName}' uploaded successfully." });
        }

        [Function("DownloadStaffDocument")]
        public async Task<IActionResult> DownloadStaffDocument(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "staff/documents/{fileName}")] HttpRequest req,
            string fileName)
        {
            _logger.logInformation($"Downloading staff document: {fileName}");

            try
            {
                var (content, contentType, name) = await _documentStore.DownloadDocumentAsync(fileName);
                return new FileStreamResult(content, contentType)
                {
                    FileDownloadName = name
                };
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, $"Error downloading document {fileName}");
                return new NotFoundObjectResult(new { error = $"Document '{fileName}' not found." });
            }
        }
    }
}