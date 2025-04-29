using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lgDevHabit.Api.Migrations;

    /// <inheritdoc />
    public partial class test_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refresh_token",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    token = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refresh_token", x => x.id);
                    table.ForeignKey(
                        name: "fk_refresh_token_identity_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "identity_user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_token",
                schema: "lgdev_habit",
                table: "refresh_token",
                column: "token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_refresh_token_user_id",
                schema: "lgdev_habit",
                table: "refresh_token",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "lgdev_habit");
        }
    }
