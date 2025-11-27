using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Coldairarrow.Migrations.Migrations
{
    public partial class AddAuditFields : Migration
    {
        // 需要添加审计字段的所有表
        private static readonly string[] Tables = new[]
        {
            "Base_Action",
            "Base_AppSecret",
            "Base_DbLink",
            "Base_Department",
            "Base_Role",
            "Base_RoleAction",
            "Base_User",
            "Base_UserLog",
            "Base_UserRole"
        };

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                // 添加 CreatorName 字段
                migrationBuilder.AddColumn<string>(
                    name: "CreatorName",
                    table: table,
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: true);

                // 添加 ModifyTime 字段
                migrationBuilder.AddColumn<DateTime>(
                    name: "ModifyTime",
                    table: table,
                    type: "datetime2",
                    nullable: true);

                // 添加 ModifierId 字段
                migrationBuilder.AddColumn<string>(
                    name: "ModifierId",
                    table: table,
                    type: "nvarchar(50)",
                    maxLength: 50,
                    nullable: true);

                // 添加 ModifierName 字段
                migrationBuilder.AddColumn<string>(
                    name: "ModifierName",
                    table: table,
                    type: "nvarchar(100)",
                    maxLength: 100,
                    nullable: true);

                // 添加 TenantId 字段
                migrationBuilder.AddColumn<string>(
                    name: "TenantId",
                    table: table,
                    type: "nvarchar(50)",
                    maxLength: 50,
                    nullable: true);
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in Tables)
            {
                migrationBuilder.DropColumn(name: "CreatorName", table: table);
                migrationBuilder.DropColumn(name: "ModifyTime", table: table);
                migrationBuilder.DropColumn(name: "ModifierId", table: table);
                migrationBuilder.DropColumn(name: "ModifierName", table: table);
                migrationBuilder.DropColumn(name: "TenantId", table: table);
            }
        }
    }
}
