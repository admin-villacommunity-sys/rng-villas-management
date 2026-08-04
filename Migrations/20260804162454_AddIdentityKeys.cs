using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VillaCommunityManagement.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // MAINTENANCE TABLE
            // ============================================================

            // 1. Drop the existing primary key (Villa_No)
            migrationBuilder.DropPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance");

            // 2. Add the new MaintenanceId column (identity)
            migrationBuilder.AddColumn<int>(
                name: "MaintenanceId",
                table: "Maintenance",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 3. Make the old key columns nullable (allow nulls temporarily)
            migrationBuilder.AlterColumn<int>(
                name: "Villa_No",
                table: "Maintenance",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // 4. Set the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance",
                column: "MaintenanceId");

            // ============================================================
            // INCOME TABLE
            // ============================================================

            // 1. Drop the existing primary key (month)
            migrationBuilder.DropPrimaryKey(
                name: "PK_Income",
                table: "Income");

            // 2. Add the new IncomeId column (identity)
            migrationBuilder.AddColumn<int>(
                name: "IncomeId",
                table: "Income",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 3. Set the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Income",
                table: "Income",
                column: "IncomeId");

            // ============================================================
            // EXPENDITURE TABLE
            // ============================================================

            // 1. Drop the existing primary key (Payment_date)
            migrationBuilder.DropPrimaryKey(
                name: "PK_Expenditure",
                table: "Expenditure");

            // 2. Add the new ExpenditureId column (identity)
            migrationBuilder.AddColumn<int>(
                name: "ExpenditureId",
                table: "Expenditure",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1, 1");

            // 3. Set the new primary key
            migrationBuilder.AddPrimaryKey(
                name: "PK_Expenditure",
                table: "Expenditure",
                column: "ExpenditureId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // ============================================================
            // MAINTENANCE TABLE
            // ============================================================

            // 1. Drop the new primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance");

            // 2. Drop the MaintenanceId column
            migrationBuilder.DropColumn(
                name: "MaintenanceId",
                table: "Maintenance");

            // 3. Restore the old primary key (Villa_No)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Maintenance",
                table: "Maintenance",
                column: "Villa_No");

            // ============================================================
            // INCOME TABLE
            // ============================================================

            // 1. Drop the new primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Income",
                table: "Income");

            // 2. Drop the IncomeId column
            migrationBuilder.DropColumn(
                name: "IncomeId",
                table: "Income");

            // 3. Restore the old primary key (month)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Income",
                table: "Income",
                column: "month");

            // ============================================================
            // EXPENDITURE TABLE
            // ============================================================

            // 1. Drop the new primary key
            migrationBuilder.DropPrimaryKey(
                name: "PK_Expenditure",
                table: "Expenditure");

            // 2. Drop the ExpenditureId column
            migrationBuilder.DropColumn(
                name: "ExpenditureId",
                table: "Expenditure");

            // 3. Restore the old primary key (Payment_date)
            migrationBuilder.AddPrimaryKey(
                name: "PK_Expenditure",
                table: "Expenditure",
                column: "Payment_date");
        }
    }
}
