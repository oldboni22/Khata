using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Minio.DataModel;

namespace MinIoService;

public interface IMinioService
{
    Task UploadFileAsync(IFormFile file, string key);

    Task<(Stream stream, ObjectStat stats)?> GetFileAsync(string key);
    
    Task DeleteFileAsync(string key);
}
