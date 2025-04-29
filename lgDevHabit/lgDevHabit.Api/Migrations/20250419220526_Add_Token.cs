using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lgDevHabit.Api.Migrations;

    /// <inheritdoc />
    public partial class Add_Token : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "identity_user",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    user_name = table.Column<string>(type: "text", nullable: true),
                    normalized_user_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    normalized_email = table.Column<string>(type: "text", nullable: true),
                    email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: true),
                    security_stamp = table.Column<string>(type: "text", nullable: true),
                    concurrency_stamp = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone_number_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    two_factor_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    lockout_end = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    lockout_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    access_failed_count = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_identity_user", x => x.id);
                });

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

            migrationBuilder.DropTable(
                name: "identity_user",
                schema: "lgdev_habit");
        }
    }
;
