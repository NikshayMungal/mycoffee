using System;
using System.IO;
using System.Threading.Tasks;
using Azure;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Microsoft.Extensions.Configuration;

namespace CoffeeNChill.Services
{
    public class DocumentStores : IDocumentStore
    {
        private readonly string _connectionString;
        private readonly string _shareName;

        public DocumentStores(IConfiguration configuration)
        {
            _connectionString = configuration["AzureWebJobsStorage"] ??
                "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;QueueEndpoint=http://azurite:10001/devstoreaccount1;TableEndpoint=http://azurite:10002/devstoreaccount1;";

            _shareName = "staff-documents";
        }

        public async Task<ShareClient> GetShareClientAsync()
        {
            var shareClient = new ShareClient(_connectionString, _shareName);
            await shareClient.CreateIfNotExistsAsync();
            return shareClient;
        }

        public async Task UploadDocumentAsync(string fileName, Stream content, string contentType)
        {
            var shareClient = await GetShareClientAsync();
            var directoryClient = shareClient.GetRootDirectoryClient();
            await directoryClient.CreateIfNotExistsAsync();

            var fileClient = directoryClient.GetFileClient(fileName);

            content.Position = 0;
            await fileClient.CreateAsync(content.Length);

            await fileClient.UploadAsync(content);
            await fileClient.SetHttpHeadersAsync(new ShareFileHttpHeaders
            {
                ContentType = contentType
            });
        }

        public async Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string fileName)
        {
            var shareClient = await GetShareClientAsync();
            var directoryClient = shareClient.GetRootDirectoryClient();
            var fileClient = directoryClient.GetFileClient(fileName);

            var downloadResponse = await fileClient.DownloadAsync();
            string contentType = downloadResponse.Value.ContentType ?? "application/octet-stream";

            return (downloadResponse.Value.Content, contentType, fileName);
        }
    }
}