using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace lgDevHabit.Api.Migrations.Application;

    /// <inheritdoc />
    public partial class dataBase_init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "lgdev_habit");

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
                name: "users",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    email = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    identity_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_users", x => x.id);
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

            migrationBuilder.CreateTable(
                name: "github_access_tokens",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    token = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_git_hub_access_tokens", x => x.id);
                    table.ForeignKey(
                        name: "fk_git_hub_access_tokens_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "habits",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    frequency_type = table.Column<int>(type: "integer", nullable: false),
                    frequency_times_per_period = table.Column<int>(type: "integer", nullable: false),
                    target_value = table.Column<int>(type: "integer", nullable: false),
                    target_unit = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_archived = table.Column<bool>(type: "boolean", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    milestone_target = table.Column<int>(type: "integer", nullable: true),
                    milestone_current = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_completed_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_habits", x => x.id);
                    table.ForeignKey(
                        name: "fk_habits_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tags",
                schema: "lgdev_habit",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_id = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tags", x => x.id);
                    table.ForeignKey(
                        name: "fk_tags_users_user_id",
                        column: x => x.user_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "habit_tags",
                schema: "lgdev_habit",
                columns: table => new
                {
                    habit_id = table.Column<string>(type: "character varying(500)", nullable: false),
                    tag_id = table.Column<string>(type: "character varying(500)", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_habit_tags", x => new { x.habit_id, x.tag_id });
                    table.ForeignKey(
                        name: "fk_habit_tags_habits_habit_id",
                        column: x => x.habit_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "habits",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_habit_tags_tags_tag_id",
                        column: x => x.tag_id,
                        principalSchema: "lgdev_habit",
                        principalTable: "tags",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_git_hub_access_tokens_user_id",
                schema: "lgdev_habit",
                table: "git_hub_access_tokens",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_habit_tags_tag_id",
                schema: "lgdev_habit",
                table: "habit_tags",
                column: "tag_id");

            migrationBuilder.CreateIndex(
                name: "ix_habits_user_id",
                schema: "lgdev_habit",
                table: "habits",
                column: "user_id");

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

            migrationBuilder.CreateIndex(
                name: "ix_tags_name",
                schema: "lgdev_habit",
                table: "tags",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_tags_user_id",
                schema: "lgdev_habit",
                table: "tags",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                schema: "lgdev_habit",
                table: "users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_identity_id",
                schema: "lgdev_habit",
                table: "users",
                column: "identity_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "git_hub_access_tokens",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "habit_tags",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "refresh_token",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "habits",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "tags",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "identity_user",
                schema: "lgdev_habit");

            migrationBuilder.DropTable(
                name: "users",
                schema: "lgdev_habit");
        }
    }

