using Microsoft.EntityFrameworkCore;
using System.Linq.Dynamic.Core;

namespace Justin.EntityFramework.Model {
    public class PagedList<T> where T : class {

        public List<T> Data { get; set; } = [];
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPage { get { return (int)Math.Ceiling(TotalData / (double)PageSize); } }
        public int TotalData { get; set; }
        public bool HasNext => CurrentPage < TotalPage;
        public bool HasPrevious => CurrentPage > 1;

        public PagedList(List<T> data, int currentPage, int pageSize, int totalData) {
            Data = data;
            CurrentPage = currentPage;
            PageSize = pageSize;
            TotalData = totalData;
        }

        public static async Task<PagedList<T>> GetPagedList(IQueryable<T> datas, PagedOptions pagedOptions) {

            var totalCount = datas.Count();

            // Manually specify ordering
            if(!string.IsNullOrEmpty(pagedOptions.OrderBy)) {
                var orderByClause = GetOrderByClause(pagedOptions.OrderBy);
                datas = datas.OrderBy(orderByClause);
            }

            var items = await datas
                .Skip((pagedOptions.Page - 1) * totalCount)
                .Take(pagedOptions.PageSize)
                .ToListAsync();

            return new PagedList<T>(items, totalCount, pagedOptions.Page, pagedOptions.PageSize);

        }

        private static string GetOrderByClause(string orderBy) {

            if(string.IsNullOrEmpty(orderBy))
                return string.Empty;

            var orderByClause = orderBy.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var propertyName = orderByClause[0];
            var sortDirection = (orderByClause.Length > 1 ? orderByClause[1] : "asc").ToLower();

            var property = typeof(T).GetProperties().FirstOrDefault(p => string.Equals(p.Name, propertyName, StringComparison.OrdinalIgnoreCase));

            if(property == null)
                throw new ArgumentException($"Property '{propertyName}' not found on type '{typeof(T).Name}'");

            return $"{property.Name} {sortDirection}";

        }

    }
}
