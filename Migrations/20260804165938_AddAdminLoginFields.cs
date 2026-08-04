using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaCommunityManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddAdminLoginFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminLogin",
                table: "AdminLogin");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "AdminLogin",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "AdminLogin",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AdminLogin",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "AdminLogin",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AdminLogin",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "AdminLogin",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiry",
                table: "AdminLogin",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminLogin",
                table: "AdminLogin",
                column: "AdminId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_AdminLogin",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "AdminLogin");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiry",
                table: "AdminLogin");

            migrationBuilder.AlterColumn<string>(
                name: "Username",
                table: "AdminLogin",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AdminLogin",
                table: "AdminLogin",
                column: "Username");
        }
    }
}
