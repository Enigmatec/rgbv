using Core.Entities;

namespace Service.Models
{
    public class GetAllCompaintFormsRequest : PaginationModel
    {
        public ComplaintType? ComplaintType { get; set; }

        public ComplaintStatus? Status { get; set; }
    }
}