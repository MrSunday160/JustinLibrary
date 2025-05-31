using Microsoft.AspNetCore.Http;

namespace Justin.EntityFramework.Model {
    public class PagedOptions {

        public string OrderBy { get; set; } = string.Empty;
        public string SearchString { get; set; } = string.Empty;
        public int Page { get { return _page; } set { _page = value == 0 ? _defaultPage : value; } }
        public int PageSize { get { return _pageSize; } set { _pageSize = value == 0 ? _defaultPageSize : value; } }

        // private properties
        private static readonly int _defaultPage = 1;
        private static readonly int _defaultPageSize = 10;

        private int _page = _defaultPage;
        private int _pageSize = _defaultPageSize;

        public PagedOptions() {
        }

        public PagedOptions(HttpRequest request) {

            if(request != null && !string.IsNullOrEmpty(request.Headers["X-PAGINATION"])) {

                _ = bool.TryParse(request.Headers["X-PAGINATION"], out bool isPagination);
                if(isPagination) {

                    Page = GetIntHeader(GetHeader(request, "X-PAGE"));
                    PageSize = GetIntHeader(GetHeader(request, "X-PAGESIZE"));
                    OrderBy = GetHeader(request, "X-ORDERBY");
                    SearchString = GetHeader(request, "X-SEARCH");

                }

            }

        }

        public static PagedOptions GetPagedOptions(HttpRequest request) {

            if(request is null) return null;

            var isPaginationString = request.Headers["X-PAGINATION"];
            if(string.IsNullOrEmpty(isPaginationString)) return null;

            _ = bool.TryParse(isPaginationString, out bool isPagination);
            if(!isPagination) return null;

            try {

                return new PagedOptions() {

                    Page = GetIntHeader(GetHeader(request, "X-PAGE")),
                    PageSize = GetIntHeader(GetHeader(request, "X-PAGESIZE")),
                    OrderBy = GetHeader(request, "X-ORDERBY"),
                    SearchString = GetHeader(request, "X-SEARCH"),

                };

            }
            catch(Exception ex) { throw new ArgumentException(ex.Message); }

        }

        #region Private

        private static int GetIntHeader(string headerValue) {
            if(!string.IsNullOrEmpty(headerValue)) {
                return Convert.ToInt32(headerValue);
            }
            return 0;
        }

        private static string GetHeader(HttpRequest request, string key) {
            var result = request.Headers[key];
            return string.IsNullOrEmpty(result) ? string.Empty : result.ToString();
        }

        #endregion

    }
}