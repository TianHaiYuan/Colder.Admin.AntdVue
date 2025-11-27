using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 用户角色表
    /// </summary>
    [Table("Base_UserRole")]
    public class Base_UserRole : BaseEntity
    {
        /// <summary>
        /// 用户Id
        /// </summary>
        [MaxLength(50)]
        public string UserId { get; set; }

        /// <summary>
        /// 角色Id
        /// </summary>
        [MaxLength(50)]
        public string RoleId { get; set; }
    }
}