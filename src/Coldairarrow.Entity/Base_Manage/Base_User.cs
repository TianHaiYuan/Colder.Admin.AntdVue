using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Coldairarrow.Entity.Base_Manage
{
    /// <summary>
    /// 系统用户表
    /// </summary>
    [Table("Base_User")]
    public class Base_User : BaseEntity
    {
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(100)]
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        [MaxLength(100)]
        public string Password { get; set; }

        /// <summary>
        /// 姓名
        /// </summary>
        [MaxLength(100)]
        public string RealName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public Sex Sex { get; set; }

        /// <summary>
        /// 出生日期
        /// </summary>
        public DateTime? Birthday { get; set; }

        /// <summary>
        /// 所属部门Id
        /// </summary>
        [MaxLength(50)]
        public string DepartmentId { get; set; }

    }

    public enum Sex
    {
        [Description("男人")]
        Man = 1,

        [Description("女人")]
        Woman = 0
    }
}