using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Models
{
    public class BaseSearchViewModel
    {
        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageIndex must be greater than 0")]
        public int PageIndex { get; set; } = 1;

        public int PageTotal { get; set; }
        /// <summary>
        /// Gets the page skip.
        /// </summary>
        /// <value>The page skip.</value>
        public int PageSkip => (PageIndex - 1) * PageSize;
        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0")]

        public int PageSize { get; set; } = 10;

        public string Filter { get; set; }

        public string Keyword { get; set; }

        // public DateFilter DateFilter { get; set; }
        //public List<DateFilter> DateFilters { get; set; } = new List<DateFilter>();

        //public List<Filter> Filters = new List<Filter>(); 
    }
    /*
    public class BaseSearchViewModel
    {
        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>The index of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageIndex must be greater than 0")]
        public int PageIndex { get; set; } = CoreConstants.DefaultPageIndex;

        /// <summary>
        /// Gets or sets the page total.
        /// </summary>
        /// <value>The page total.</value>
        public int? PageTotal { get; set; }

        /// <summary>
        /// Gets or sets the size of the page.
        /// </summary>
        /// <value>The size of the page.</value>
        [Range(1, int.MaxValue, ErrorMessage = "PageSize must be greater than 0")]
        public int PageSize { get; set; } = 10;

        /// <summary>
        /// Gets the page skip.
        /// </summary>
        /// <value>The page skip.</value>
        public int PageSkip => (PageIndex - 1) * PageSize;
        /// <summary>
        /// Gets or sets the filters.
        /// </summary>
        /// <value>The filters.</value>
        public List<Filter> Filters { get; set; } = new List<Filter>();

    }

    */
    public class UsersSearchViewModel : BaseSearchViewModel
    {
        public string RoleName { get; set; }
        public Guid Id { get; set; }
    }

    public class DateFilter
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
    }

    public class StateAndLgaFilter:BaseSearchViewModel
    {
        public string Lga { get; set; }

        public string State { get; set; }
        public string Ward { get; set; }

    }


    /// <summary>
    /// Class Filter.
    /// </summary>
    public class Filter
    {
        /// <summary>
        /// Gets or sets the keyword.
        /// </summary>
        /// <value>The keyword.</value>
        [Required] public string Keyword { get; set; }

        /// <summary>
        /// Gets or sets the filter column.
        /// </summary>
        /// <value>The filter column.</value>
        [Required] public string FilterColumn { get; set; }

        /// <summary>
        /// Gets or sets the operation.
        /// </summary>
        /// <value>The operation.</value>
        [Required] public FilterOperation? Operation { get; set; }
    }

    public enum FilterOperation
    {
        /// <summary>
        /// Determines whether this instance contains the object.
        /// </summary>
        Contains = 1,
        /// <summary>
        /// The starts with
        /// </summary>
        StartsWith = 2,
        /// <summary>
        /// The ends with
        /// </summary>
        EndsWith = 3,
        /// <summary>
        /// The greater than
        /// </summary>
        GreaterThan = 4,
        /// <summary>
        /// The less than
        /// </summary>
        LessThan = 5,
        /// <summary>
        /// The equals
        /// </summary>
        Equals = 6,
        /// <summary>
        /// The not equals
        /// </summary>
        NotEquals = 7,
        /// <summary>
        /// The greater than or equals
        /// </summary>
        GreaterThanOrEquals = 8,
        /// <summary>
        /// The less thank or equals
        /// </summary>
        LessThankOrEquals = 9,
        /// <summary>
        /// The ascending
        /// </summary>
        Ascending = 10,
        /// <summary>
        /// The descending
        /// </summary>
        Descending = 11,
        /// <summary>
        /// The date before
        /// </summary>
        DateBefore = 12,
        /// <summary>
        /// The date after
        /// </summary>
        DateAfter = 13
    }
}
