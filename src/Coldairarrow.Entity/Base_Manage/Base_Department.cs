using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 部门表
    /// </summary>
    [Table("Base_Department")]
    public class Base_Department : BaseEntity
    {
        /// <summary>
        /// 部门名
        /// </summary>
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// 上级部门Id
        /// </summary>
        [MaxLength(50)]
        public string ParentId { get; set; }
    }
}