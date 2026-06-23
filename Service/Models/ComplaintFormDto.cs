using Core.Entities;
using System;
using System.Collections.Generic;

namespace Service.Models
{
    public class ComplaintFormDto
    {
        public int Id { get; set; }

        public string ComplaintCode { get; private set; }

        public string UserId { get; set; }

        public string UserName { get; set; }

        public string UserOrganisation { get; set; }

        public string UserState { get; set; }

        public string UserType { get; set; }

        public ComplaintType ComplaintType { get; set; }

        public ComplaintStatus Status { get; set; }

        public string Subject { get; set; }

        public string Body { get; set; }

        public List<FileUploadDto> Attachements { get; set; }

        public string ResolvedByUserId { get; set; }

        public string ResolvedByUserName { get; set; }

        public DateTime? ResolvedDate { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime? ModifiedAt { get; set; }
    }
}