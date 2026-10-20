using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TodoAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Todos_Title",
                table: "Todos");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Todos",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Todos_UserId_Title",
                table: "Todos",
                columns: new[] { "UserId", "Title" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Todos_Users_UserId",
                table: "Todos",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Todos_Users_UserId",
                table: "Todos");

            migrationBuilder.DropIndex(
                name: "IX_Todos_UserId_Title",
                table: "Todos");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Todos");

            migrationBuilder.CreateIndex(
                name: "IX_Todos_Title",
                table: "Todos",
                column: "Title",
                unique: true);
        }
    }
}
