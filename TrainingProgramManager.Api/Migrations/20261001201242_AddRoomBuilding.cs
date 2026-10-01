using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingProgramManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomBuilding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Building",
                table: "Rooms",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Building",
                table: "Rooms");
        }
    }
}
