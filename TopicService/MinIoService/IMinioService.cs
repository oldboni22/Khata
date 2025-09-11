using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Minio.DataModel;

namespace MinIoService;

public interface IMinioService
{
    Task UploadFileAsync(IFormFile file, string bucketName, string key);

    Task<(Stream stream, ObjectStat stats)?> GetFileAsync(string bucketName, string key);
    
    Task DeleteFileAsync(string bucketName, string key);
}
