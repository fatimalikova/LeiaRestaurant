using Leia.Application.Exceptions;
using Leia.Application.Helpers;
using Leia.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Leia.Api.Controllers
{
    [ApiController]
    [Route("api/upload")]
    public class UploadController(IFileUploadService fileUploadService) : ControllerBase
    {
        private static readonly string[] AllowedTypes = { "image/jpeg", "image/png", "image/webp" };
        private const long MaxBytes = 5 * 1024 * 1024;

        [HttpPost("image")]
        [Authorize]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new BadRequestException("File is required.");

            if (file.Length > MaxBytes)
                throw new BadRequestException("File size cannot exceed 5 MB.");

            if (!AllowedTypes.Contains(file.ContentType))
                throw new BadRequestException("Only JPEG, PNG or WEBP images are allowed.");

            await using var stream = file.OpenReadStream();
            var url = await fileUploadService.UploadImageAsync(stream, file.FileName);
            return Ok(ResponseModelHelper.SuccessResult(new { url }));
        }
    }
}