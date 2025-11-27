using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 操作记录表
    /// </summary>
    [Table("Base_UserLog")]
    public class Base_UserLog : BaseEntity
    {
        /// <summary>
        /// 日志类型
        /// </summary>
        [MaxLength(50)]
        public string LogType { get; set; }

        /// <summary>
        /// 日志内容
        /// </summary>
        [MaxLength(2000)]
        public string LogContent { get; set; }
    }
}