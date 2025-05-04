using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsWebApi.Migrations
{
    /// <inheritdoc />
    public partial class NullableChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventImageUrl",
                table: "Events");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Events",
                type: "datetime2(0)",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<Guid>(
                name: "EventImageId",
                table: "Events",
                type: "uniqueidentifier",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EventImageId",
                table: "Events");

            migrationBuilder.AlterColumn<DateTime>(
                name: "EventDate",
                table: "Events",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2(0)",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventImageUrl",
                table: "Events",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
