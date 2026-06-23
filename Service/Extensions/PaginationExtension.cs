using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Service.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Extensions
{
    public static class PaginationExtension
    {
        public static PaginatedList<T> Page<T>(this IOrderedQueryable<T> query, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;
            var count = query.Count();
            var items = query.Skip((Page - 1) * Size).Take(Size).ToList();

            return new PaginatedList<T>(items, count, Page, Size);
        }

        public static async Task<PaginatedList<T>> PageAsync<T>(this IOrderedQueryable<T> query, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;
            var count = await query.CountAsync();
            var items = await query.Skip(((Page) - 1) * Size).Take(Size).ToListAsync();

            return new PaginatedList<T>(items, count, Page, Size);
        }

        public static async Task<PaginatedList<T>> PageAsync<T>(this IQueryable<T> query, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;

            var count = await query.CountAsync();
            var items = await query.Skip(((Page) - 1) * Size).Take(Size).ToListAsync();

            return new PaginatedList<T>(items, count, Page, Size);
        }

        public static async Task<PaginatedList<R>> PageProjectAsync<T, R>(this IQueryable<T> query, IMapper mapper, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;

            var count = await query.CountAsync();
            var items = await query.Skip(((Page) - 1) * Size).Take(Size).ProjectTo<R>(mapper.ConfigurationProvider).ToListAsync();

            return new PaginatedList<R>(items, count, Page, Size);
        }

        public static async Task<PaginatedList<R>> PageMapAsync<T, R>(this IQueryable<T> query, IMapper mapper, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;

            var count = await query.CountAsync();
            var items = mapper.Map<IEnumerable<R>>(await query.Skip(((Page) - 1) * Size).Take(Size).ToListAsync());

            return new PaginatedList<R>(items, count, Page, Size);
        }

        public static PaginatedList<T> Page<T>(this IEnumerable<T> source, int? page, int? size) where T : class
        {
            var Page = !page.HasValue ? 1 : page.Value == 0 ? 1 : page.Value;
            var Size = !size.HasValue ? 20 : size.Value == 0 ? 20 : size.Value;
            var count = source.Count();
            var items = source.Skip((Page - 1) * Size).Take(Size).ToList();

            return new PaginatedList<T>(items, count, Page, Size);
        }
    }
}