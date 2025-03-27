using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace kizwaonlineshop.Server.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePRODUCT_CARTtable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "category",
                table: "prodcart",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "image",
                table: "prodcart",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "size",
                table: "prodcart",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "category",
                table: "prodcart");

            migrationBuilder.DropColumn(
                name: "image",
                table: "prodcart");

            migrationBuilder.DropColumn(
                name: "size",
                table: "prodcart");
        }
    }
}
