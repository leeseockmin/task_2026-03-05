using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DB.Data.Migrations.AccountMigrations
{
    /// <inheritdoc />
    public partial class addIndexEmployee : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Name",
                table: "Employee");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Name",
                table: "Employee",
                column: "name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Employees_Name",
                table: "Employee");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Name",
                table: "Employee",
                column: "name",
                unique: true);
        }
    }
}
