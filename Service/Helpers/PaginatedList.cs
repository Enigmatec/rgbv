using System;
using System.Collections.Generic;

namespace Service.Helpers
{
    public class PaginatedList<T>
    {
        public int PageSize { get; set; }
        public int PageIndex { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }

        public IEnumerable<T> Items { get; set; }

        public PaginatedList(IEnumerable<T> items, int count, int pageIndex, int pageSize)
        {
            PageSize = pageSize;
            PageIndex = pageIndex;
            TotalCount = count;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            HasNext = pageIndex < TotalPages;
            HasPrevious = pageIndex > 1;

            Items = items;
        }
    }


}