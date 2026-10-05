using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingProgramManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "DeletedAt",
                table: "Workshops",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Workshops",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Workshops");
        }
    }
}
