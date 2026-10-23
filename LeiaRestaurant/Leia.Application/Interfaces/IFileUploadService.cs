using System;
using System.Collections.Generic;
using System.Text;

namespace Leia.Application.Interfaces
{
    public interface IFileUploadService
    {
        Task<string> UploadImageAsync(Stream fileStream, string fileName);
    }
}
