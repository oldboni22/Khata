using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Minio;
using Minio.DataModel;
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

    public async Task<(Stream stream, ObjectStat stats)?> GetFileAsync(string bucketName, string key)
    {
        try
        {
            var statArgs = new StatObjectArgs()
                .WithBucket(bucketName)
                .WithObject(key);

            var stats = await client.StatObjectAsync(statArgs);

            var objectStream = new MemoryStream();
        
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(bucketName)
                .WithObject(key)
                .WithCallbackStream((s) =>
                {
                    s.CopyTo(objectStream);
                    objectStream.Position = 0;
                });
            
            await client.GetObjectAsync(getObjectArgs);

            return (objectStream, stats);
        }
        catch (Minio.Exceptions.ObjectNotFoundException e)
        {
            return null; 
        }
    }
    
    public async Task DeleteFileAsync(string bucketName, string key)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(bucketName)
            .WithObject(key);

        await client.RemoveObjectAsync(removeObjectArgs);
    }
}
