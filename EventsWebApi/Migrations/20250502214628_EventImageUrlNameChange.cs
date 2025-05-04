using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EventsWebApi.Migrations
{
    /// <inheritdoc />
    public partial class EventImageUrlNameChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageUrl",
                table: "Events",
                newName: "EventImageUrl");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EventImageUrl",
                table: "Events",
                newName: "ImageUrl");
        }
    }
}
