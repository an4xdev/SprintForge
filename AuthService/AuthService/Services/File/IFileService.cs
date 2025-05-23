namespace AuthService.Services.File;

public interface IFileService
{
    public Task<string?> UploadFileAsync(string objectName, Stream fileStream,
        string contentType);
}