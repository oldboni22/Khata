using Minio;
using MinIoService;

namespace Infrastructure.Minio;

public class TopicMinioService(IMinioClient client) : MinioService(client)
{
    protected override string BucketName => "topicscontent";
}
