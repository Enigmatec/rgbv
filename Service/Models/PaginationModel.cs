using System;

namespace Service.Models
{
    public class PaginationModel
    {
        public int? PageIndex { get; set; }
        public int? PageSize { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}