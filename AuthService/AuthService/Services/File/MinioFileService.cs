using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;

namespace AuthService.Services.File;

public class MinioFileService(IMinioClient minioClient, IOptions<MinioSettings> settings) : IFileService
{
    public async Task<string?> UploadFileAsync(string objectName, Stream fileStream, string contentType)
    {
        var bucketName = settings.Value.Bucket;

        var bucketExists = await minioClient.BucketExistsAsync(new BucketExistsArgs().WithBucket(bucketName));
        if (!bucketExists)
        {
            return null;
        }

        await minioClient.PutObjectAsync(new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(fileStream)
            .WithObjectSize(fileStream.Length)
            .WithContentType(contentType));

        return objectName;
    }
}

public class MinioSettings
{
    public required string Endpoint { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }
    public required string Bucket { get; set; }
    public bool UsePathStyle { get; set; }
}