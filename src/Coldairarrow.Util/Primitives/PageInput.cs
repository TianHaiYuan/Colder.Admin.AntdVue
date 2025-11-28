using System.Collections.Generic;
using System.Linq;

namespace Coldairarrow.Util
{
    /// <summary>
    /// 分页查询基类
    /// </summary>
    public class PageInput
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int PageIndex { get; set; } = 1;

        /// <summary>
        /// 每页行数
        /// </summary>
        public int PageRows { get; set; } = int.MaxValue;

        /// <summary>
        /// 排序集合
        /// </summary>
        public ICollection<Sort> Sorts { get; set; }

        /// <summary>
        /// 排序项
        /// </summary>
        public class Sort
        {
            /// <summary>
            /// 排序列
            /// </summary>
            public string Field { get; set; } = "Id";

            /// <summary>
            /// 排序类型 (asc/desc)
            /// </summary>
            public string Type { get; set; } = "asc";


            public Sort() { }

            public Sort(string field, string type = "asc")
            {
                Field = field;
                Type = type;
            }
        }
    }
}
