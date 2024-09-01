using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace distributedDeliveryBackend.Migrations
{
    /// <inheritdoc />
    public partial class RiderDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdOrder",
                table: "orderDb",
                newName: "IdArticle");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IdArticle",
                table: "orderDb",
                newName: "IdOrder");
        }
    }
}
