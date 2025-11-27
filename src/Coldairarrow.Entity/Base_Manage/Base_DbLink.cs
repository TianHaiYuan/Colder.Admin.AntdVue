using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 数据库连接表
    /// </summary>
    [Table("Base_DbLink")]
    public class Base_DbLink : BaseEntity
    {
        /// <summary>
        /// 连接名
        /// </summary>
        [MaxLength(100)]
        public string LinkName { get; set; }

        /// <summary>
        /// 连接字符串
        /// </summary>
        [MaxLength(500)]
        public string ConnectionStr { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        [MaxLength(50)]
        public string DbType { get; set; }
    }
}