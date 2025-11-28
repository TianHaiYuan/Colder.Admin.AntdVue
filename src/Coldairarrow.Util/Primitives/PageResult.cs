using System.Collections.Generic;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 分页返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PageResult<T> : AjaxResult<List<T>> where T : new()
    {
        /// <summary>
        /// 总记录数
        /// </summary>
        public int TotalCount { get; set; }

        public int TotalPages { get; set; }

        public bool HasPrevPages { get; set; }

        public bool HasNextPages { get; set; }
    }
}
