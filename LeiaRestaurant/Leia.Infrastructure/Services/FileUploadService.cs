using Leia.Application.Exceptions;
using Leia.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace Leia.Infrastructure.Services
{
    public class FileUploadService(HttpClient http, IConfiguration config) : IFileUploadService
    {
        public async Task<string> UploadImageAsync(Stream fileStream, string fileName)
        {
            var apiKey = config["ImgBB:ApiKey"]
                ?? throw new InvalidOperationException("ImgBB:ApiKey is not configured.");

            using var content = new MultipartFormDataContent();
            content.Add(new StreamContent(fileStream), "image", fileName);

            var response = await http.PostAsync($"https://api.imgbb.com/1/upload?key={apiKey}", content);
            if (!response.IsSuccessStatusCode)
                throw new BadRequestException("Image upload failed.");

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            return json.RootElement.GetProperty("data").GetProperty("url").GetString()!;
        }
    }
}