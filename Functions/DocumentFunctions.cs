using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using CoffeeNChill.Functions.Services;

namespace CoffeeNChill.Functions
{
    public class DocumentFunctions
    {
        private readonly ILogger<DocumentFunctions> _logger;
        private readonly IDocumentStore _documentStore;

        public DocumentFunctions(ILogger<DocumentFunctions> logger, IDocumentStore documentStore)
        {
            _logger = logger;
            _documentStore = documentStore;
        }

        [Function("UploadDocument")]
        public async Task<IActionResult> UploadDocument(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "documents")] HttpRequest req)
        {
            _logger.LogInformation("Processing document upload request.");

            if (!req.HasFormContentType)
            {
                return new BadRequestObjectResult("Request must be multipart/form-data.");
            }

            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file == null || file.Length == 0)
            {
                return new BadRequestObjectResult("No file uploaded.");
            }

            using var stream = file.OpenReadStream();
            await _documentStore.UploadDocumentAsync(file.FileName, stream);

            return new OkObjectResult(new { message = $"File '{file.FileName}' uploaded successfully." });
        }

        [Function("GetDocuments")]
        public async Task<IActionResult> GetDocuments(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "documents")] HttpRequest req)
        {
            _logger.LogInformation("Processing get documents request.");

            var documents = await _documentStore.ListDocumentsAsync();
            return new OkObjectResult(documents);
        }
    }
}