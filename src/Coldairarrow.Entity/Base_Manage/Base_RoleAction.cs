using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 角色权限表
    /// </summary>
    [Table("Base_RoleAction")]
    public class Base_RoleAction : BaseEntity
    {
        /// <summary>
        /// 角色Id
        /// </summary>
        [MaxLength(50)]
        public string RoleId { get; set; }

        /// <summary>
        /// 权限Id
        /// </summary>
        [MaxLength(50)]
        public string ActionId { get; set; }
    }
}