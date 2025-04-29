using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lgDevHabit.Api.Migrations.Application;

    /// <inheritdoc />
    public partial class test_05 : Migration
    {
        

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                DELETE FROM lgdev_habit.tags;
                DELETE FROM lgdev_habit.habits;
                DELETE FROM lgdev_habit.habit_tags;
                """
            );
        }

        /// <inheritdoc />

    }

