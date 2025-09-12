using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Minio;
using Minio.DataModel;
using Minio.DataModel.Args;

namespace MinIoService;

public abstract class MinioService(IMinioClient client) : IMinioService
{
    protected abstract string BucketName { get; }

    public async Task UploadFileAsync(IFormFile file, string key)
    {
        await using var stream = file.OpenReadStream();

        var putObjectArgs = new PutObjectArgs()
            .WithBucket(BucketName)
            .WithObject(key)
            .WithStreamData(stream)
            .WithObjectSize(stream.Length)
            .WithContentType(file.ContentType);

        await client.PutObjectAsync(putObjectArgs);
    }

    public async Task<(Stream stream, ObjectStat stats)?> GetFileAsync(string key)
    {
        try
        {
            var statArgs = new StatObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key);

            var stats = await client.StatObjectAsync(statArgs);

            var objectStream = new MemoryStream();
        
            var getObjectArgs = new GetObjectArgs()
                .WithBucket(BucketName)
                .WithObject(key)
                .WithCallbackStream((s) =>
                {
                    s.CopyTo(objectStream);
                    objectStream.Position = 0;
                });
            
            await client.GetObjectAsync(getObjectArgs);

            return (objectStream, stats);
        }
        catch
        {
            return null; 
        }
    }
    
    public async Task DeleteFileAsync(string key)
    {
        var removeObjectArgs = new RemoveObjectArgs()
            .WithBucket(BucketName)
            .WithObject(key);

        await client.RemoveObjectAsync(removeObjectArgs);
    }
}
