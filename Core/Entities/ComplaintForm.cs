using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core.Entities
{
    public class ComplaintForm : BaseEnity<int>
    {
        private ComplaintForm()
        {

        }

        public ComplaintForm(string userId, ComplaintType complaintType, string subject, string body, List<FileUpload> attachements, string complaintCode, int serialNumber)
        {
            UserId = userId;
            ComplaintType = complaintType;
            Subject = subject;
            Body = body;
            Attachements = attachements;
            ComplaintCode = complaintCode;
            SerialNumber = serialNumber;
        }

        public int SerialNumber { get; private set; }

        [MaxLength(30)]
        public string ComplaintCode { get; private set; }

        public string UserId { get; private set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; private set; }

        [MaxLength(50)]
        public ComplaintType ComplaintType { get; private set; }

        [MaxLength(16)]
        public ComplaintStatus Status { get; private set; }

        [MaxLength(200)]
        public string Subject { get; private set; }

        [MaxLength(1000)]
        public string Body { get; private set; }

        public List<FileUpload> Attachements { get; private set; }

        public string ResolvedByUserId { get; private set; }

        [ForeignKey(nameof(ResolvedByUserId))]
        public ApplicationUser ResolvedByUser { get; private set; }

        [MaxLength(27)]
        public DateTime? ResolvedDate { get; private set; }

        public void ResolveComplaint(string resolvdeByUserId, DateTime resolvedDate)
        {
            ResolvedByUserId = resolvdeByUserId;
            ResolvedDate = resolvedDate;
            Status = ComplaintStatus.Resolved;
        }
    }

    public enum ComplaintType
    {
        CaseSubmission,
        CaseValidation,
        ServiceProvidedSubmission,
        ServiceProvidedValidation,
        Others
    }

    public enum ComplaintStatus
    {
        Open,
        Resolved
    }
}