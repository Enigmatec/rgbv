using Core.Entities;

namespace Service.Models
{
    public class ChangeComplaintFormStatusRequest
    {
        public int Id { get; set; }
        public ComplaintStatus Status { get; set; }
    }
}