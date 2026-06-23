using Microsoft.AspNetCore.Http;

namespace Service.Models
{
    public class FileUploadRequest
    {
        public string Name { get; set; }

        public string Description { get; set; }

        public IFormFile File { get; set; }
    }
}