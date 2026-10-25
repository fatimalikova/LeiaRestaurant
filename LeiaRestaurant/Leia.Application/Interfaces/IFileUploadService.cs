namespace Leia.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
    }
}