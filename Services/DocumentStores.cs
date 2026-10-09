using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Functions.Services
{
    public interface IDocumentStore
    {
        Task UploadDocumentAsync(string fileName, Stream content);
        Task<IEnumerable<string>> ListDocumentsAsync();
    }

    public class DocumentStores : IDocumentStore
    {
        private readonly string _connectionString;
        private readonly string _shareName = "staff-docs";

        public DocumentStores(IConfiguration configuration)
        {
            _connectionString = configuration["AzureWebJobsStorage"] ?? "UseDevelopmentStorage=true";
        }

        private ShareClient GetShareClient()
        {
            var serviceClient = new ShareClient(_connectionString, _shareName);
            serviceClient.CreateIfNotExists();
            return serviceClient;
        }

        public async Task UploadDocumentAsync(string fileName, Stream content)
        {
            var shareClient = GetShareClient();
            var directoryClient = shareClient.GetRootDirectoryClient();

            // Ensure directory exists
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);

            // Create or overwrite file with the stream length
            await fileClient.CreateAsync(content.Length);

            // Upload range content correctly
            await fileClient.UploadRangeAsync(new HttpRange(0, content.Length), content);
        }

        public async Task<IEnumerable<string>> ListDocumentsAsync()
        {
            var shareClient = GetShareClient();
            var directoryClient = shareClient.GetRootDirectoryClient();

            var files = new List<string>();
            await foreach (ShareFileItem item in directoryClient.GetFilesAndDirectoriesAsync())
            {
                if (!item.IsDirectory)
                {
                    files.Add(item.Name);
                }
            }

            return files;
        }
    }
}