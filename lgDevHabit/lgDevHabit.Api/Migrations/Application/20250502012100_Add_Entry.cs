using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lgDevHabit.Api.Migrations.Application;

    /// <inheritdoc />
    public partial class Add_Entry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "automation_source",
                schema: "lgdev_habit",
                table: "habits",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "automation_source",
                schema: "lgdev_habit",
                table: "habits");
        }
    }

