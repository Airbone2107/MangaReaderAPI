// path: Persistence/Migrations/..._AddForeignKeyForChapterUser.cs
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddForeignKeyForChapterUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // <<< XÓA KHỐI NÀY ĐI >>>
            // migrationBuilder.CreateIndex(
            //     name: "IX_Chapters_UploadedByUserId",
            //     table: "Chapters",
            //     column: "UploadedByUserId");

            // <<< CHỈ GIỮ LẠI KHỐI NÀY >>>
            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_AspNetUsers_UploadedByUserId",
                table: "Chapters",
                column: "UploadedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // <<< CHỈ GIỮ LẠI KHỐI NÀY >>>
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_AspNetUsers_UploadedByUserId",
                table: "Chapters");

            // <<< XÓA KHỐI NÀY ĐI >>>
            // migrationBuilder.DropIndex(
            //     name: "IX_Chapters_UploadedByUserId",
            //     table: "Chapters");
        }
    }
}