using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrainingProgramManager.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkshopOrganizer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EN: Step 1: add the column as nullable first, because existing Workshops rows have no owner yet.
            // HU: 1. lépés: az oszlopot először nullable-ként adjuk hozzá, mert a meglévő Workshops sorokhoz még nincs tulajdonos.
            migrationBuilder.AddColumn<string>(
                name: "OrganizerId",
                table: "Workshops",
                type: "TEXT",
                nullable: true);

            // EN: Step 2: backfill EVERY existing row (including soft-deleted ones, because raw SQL ignores the EF query filter) with the real Id of organizer@example.com; no fake or hardcoded user id is used.
            // HU: 2. lépés: MINDEN meglévő sort (a soft delete-teltet is, mert a nyers SQL nem veszi figyelembe az EF lekérdezési szűrőt) a valódi organizer@example.com felhasználó Id-jával töltünk ki; hamis vagy beégetett felhasználó azonosító nem szerepel.
            migrationBuilder.Sql(
                """
                UPDATE "Workshops"
                SET "OrganizerId" = (
                    SELECT "Id"
                    FROM "AspNetUsers"
                    WHERE "NormalizedEmail" = 'ORGANIZER@EXAMPLE.COM')
                WHERE "OrganizerId" IS NULL;
                """);

            // EN: Step 3: make the column required. If the organizer user was missing, rows would still be NULL and this step fails instead of silently inventing an owner.
            // HU: 3. lépés: kötelezővé tesszük az oszlopot. Ha az organizer felhasználó hiányozna, a sorok NULL-ok maradnának, és ez a lépés hibával leáll ahelyett, hogy tulajdonost találna ki.
            migrationBuilder.AlterColumn<string>(
                name: "OrganizerId",
                table: "Workshops",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            // EN: Step 4: index and foreign key to AspNetUsers.Id; Restrict prevents deleting a user who still owns workshops.
            // HU: 4. lépés: index és idegen kulcs az AspNetUsers.Id-ra; a Restrict megakadályozza, hogy olyan felhasználó törlődjön, akinek még vannak workshopjai.
            migrationBuilder.CreateIndex(
                name: "IX_Workshops_OrganizerId",
                table: "Workshops",
                column: "OrganizerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Workshops_AspNetUsers_OrganizerId",
                table: "Workshops",
                column: "OrganizerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Workshops_AspNetUsers_OrganizerId",
                table: "Workshops");

            migrationBuilder.DropIndex(
                name: "IX_Workshops_OrganizerId",
                table: "Workshops");

            migrationBuilder.DropColumn(
                name: "OrganizerId",
                table: "Workshops");
        }
    }
}
