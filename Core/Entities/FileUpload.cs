using Newtonsoft.Json;
using System;
using System.IO;

namespace Core.Entities
{
    public class FileUpload : BaseEnity<int>
    {
        private FileUpload()
        {
        }

        public FileUpload(string name,
            FileUploadMetaData metaData,
            string description,
            FileUploadCategory uploadCategory,
            Uri url,
            byte[] data,
            bool isOnExternalStorage,
            string createdById)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Ensure file name is not empty");

            if (isOnExternalStorage)
            {
                if (url is null)
                {
                    throw new InvalidOperationException("External file must have url");
                }

                Url = url.AbsolutePath;
            }

            if (string.IsNullOrWhiteSpace(createdById))
                throw new InvalidOperationException("Created by cannot be empty");

            Name = name;
            MetaData = JsonConvert.SerializeObject(metaData);
            Description = description;
            UploadCategory = uploadCategory;

            Data = data;
            IsOnExternalStorage = isOnExternalStorage;
            CreatedById = createdById;
        }

        public string Name { get; private set; }

        public string MetaData { get; private set; }

        public string Description { get; private set; }

        public FileUploadCategory UploadCategory { get; private set; }

        public string Url { get; private set; }

        public byte[] Data { get; private set; }

        public bool IsOnExternalStorage { get; private set; }

        public string CreatedById { get; private set; }

        public ApplicationUser CreatedBy { get; private set; }

        public void ChangeInfo(string name, string description)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new InvalidOperationException("Ensure file name is not empty");

            Name = name;
            Description = description;
        }
    }

    public enum FileUploadCategory
    {
        Resource,Template,ComplaintForm
    }

    public class FileUploadMetaData
    {
        public FileUploadMetaData(string fileName, string contentType, long size)
        {
            FileName = fileName;
            Extenstion = Path.GetExtension(fileName);
            ContentType = contentType;
            Size = size;
        }

        public string FileName { get; set; }

        public string Extenstion { get; set; }

        public string ContentType { get; set; }

        public long Size { get; set; }
    }
}