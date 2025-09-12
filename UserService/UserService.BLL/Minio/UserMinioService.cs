using Minio;
using MinIoService;

namespace UserService.BLL.Minio;

public class UserMinioService(IMinioClient client) : MinioService(client)
{
    protected override string BucketName => "userdata";
}
