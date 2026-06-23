using Core.Entities;

namespace Service.Models
{
    public class FileUploadDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public FileUploadMetaData MetaData { get; set; }

        public string Description { get; set; }

        public FileUploadCategory UploadCategory { get; set; }

        public string Url { get; set; }

        public byte[] Data { get; set; }

        public bool IsOnExternalStorage { get; set; }

        public string CreatedById { get; set; }

        public string CreatedBy { get; set; }
    }
}