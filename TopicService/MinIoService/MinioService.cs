using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Minio;
using Minio.DataModel.Args;

namespace MinIoService;

internal class MinioService(IMinioClient client) : IMinioService
{
    public async Task UploadFileAsync(IFormFile file, string bucketName, string key)
    {
        await using var stream = file.OpenReadStream();

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(key)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await client.PutObjectAsync(putObjectArgs);
    }

    public async Task<FormFile> GetFileAsync(string bucketName, string key)
    {
        var objectStream = new MemoryStream();
        
        var getObjectArgs = new GetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(key)
            .WithCallbackStream(stream =>
            {
                stream.CopyTo(objectStream);
            });

        var objectData = await client.GetObjectAsync(getObjectArgs);
        
        var fileData = new FormFile(objectStream, 0, objectStream.Length, "media", key)
        {
            Headers = new HeaderDictionary(),
            ContentType = "application/octet-stream"
        };

        return fileData;
    }
    
    public async Task DeleteFileAsync(string bucketName, string key)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(key);

        await client.RemoveObjectAsync(removeObjectArgs);
    }
}
