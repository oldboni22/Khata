using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;

namespace MinIoService;

public interface IMinioService
{
    Task UploadFileAsync(IFormFile file, string bucketName, string key);

    Task<FormFile> GetFileAsync(string bucketName, string key);
    
    Task DeleteFileAsync(string bucketName, string key);
}
