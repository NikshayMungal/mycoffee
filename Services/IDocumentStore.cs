using System.IO;
using System.Threading.Tasks;

namespace CoffeeNChill.Services
{
    public interface IDocumentStore
    {
        Task UploadDocumentAsync(string fileName, Stream content, string contentType);
        Task<(Stream Content, string ContentType, string FileName)> DownloadDocumentAsync(string fileName);
    }
}