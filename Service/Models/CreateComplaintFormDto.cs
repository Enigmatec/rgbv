using Core.Entities;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;

namespace Service.Models
{
    public class CreateComplaintFormDto
    {
        public ComplaintType ComplaintType { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public List<IFormFile> Attachements { get; set; }
    }
}