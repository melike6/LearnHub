using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LearnHub.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CourseId1",
                table: "Reviews",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_CourseId1",
                table: "Reviews",
                column: "CourseId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Courses_CourseId1",
                table: "Reviews",
                column: "CourseId1",
                principalTable: "Courses",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Courses_CourseId1",
                table: "Reviews");

            migrationBuilder.DropIndex(
                name: "IX_Reviews_CourseId1",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "CourseId1",
                table: "Reviews");
        }
    }
}
