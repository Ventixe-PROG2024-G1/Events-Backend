using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace EventsWebApi.Migrations
{
    /// <inheritdoc />
    public partial class SeedingCategories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "CategoryName" },
                values: new object[,]
                {
                    { new Guid("043c6f2a-adef-494b-9038-f8d7cdc154a8"), "Crafts & Hobbies" },
                    { new Guid("13267c45-1219-4cea-ba4b-336b585dca96"), "Theater & Performing Arts" },
                    { new Guid("14fa8c26-d45f-48a4-9fab-214558ef179a"), "Community & Local Events" },
                    { new Guid("16b2d573-ad6f-460d-8db4-168a6fce6044"), "Festival" },
                    { new Guid("207a467e-b44b-4c95-a0e9-f7e0ef66a455"), "Gaming & eSports" },
                    { new Guid("58253062-17d0-48cd-8cb5-35ee499fb56d"), "Music" },
                    { new Guid("7b536871-cc9f-491f-854d-289fefb93cc7"), "Food & Culinary" },
                    { new Guid("7e95dcfb-2051-4983-bc68-e9adfeaa86d1"), "Health & Wellness" },
                    { new Guid("8172ad1b-5298-48fd-a346-a4ed873484f3"), "Fashion" },
                    { new Guid("915f321e-1ca7-4ebf-98c8-01e7f8188f68"), "Literature & Book Fairs" },
                    { new Guid("987444d6-4256-4cc9-b76e-cb6c9be1875c"), "Outdoor & Activities" },
                    { new Guid("a0b781b4-48fe-4331-b809-4ee25cb25301"), "Art & Design" },
                    { new Guid("ad2ccfe4-54d3-44c7-a0a5-d25e692376f1"), "Sports & Fitness" },
                    { new Guid("c585c7a6-c5ac-4f52-83c2-fbcf0274e257"), "History & Heritage" },
                    { new Guid("db013c07-6c5d-4b7e-8d82-d2eab1a860f4"), "Film & Cinema" },
                    { new Guid("e910e72c-3d6f-4f30-b271-9e7cf46862a0"), "Technology" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("043c6f2a-adef-494b-9038-f8d7cdc154a8"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("13267c45-1219-4cea-ba4b-336b585dca96"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("14fa8c26-d45f-48a4-9fab-214558ef179a"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("16b2d573-ad6f-460d-8db4-168a6fce6044"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("207a467e-b44b-4c95-a0e9-f7e0ef66a455"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("58253062-17d0-48cd-8cb5-35ee499fb56d"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7b536871-cc9f-491f-854d-289fefb93cc7"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("7e95dcfb-2051-4983-bc68-e9adfeaa86d1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("8172ad1b-5298-48fd-a346-a4ed873484f3"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("915f321e-1ca7-4ebf-98c8-01e7f8188f68"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("987444d6-4256-4cc9-b76e-cb6c9be1875c"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("a0b781b4-48fe-4331-b809-4ee25cb25301"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("ad2ccfe4-54d3-44c7-a0a5-d25e692376f1"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("c585c7a6-c5ac-4f52-83c2-fbcf0274e257"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("db013c07-6c5d-4b7e-8d82-d2eab1a860f4"));

            migrationBuilder.DeleteData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: new Guid("e910e72c-3d6f-4f30-b271-9e7cf46862a0"));
        }
    }
}
