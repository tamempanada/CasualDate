using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CasualDate.Migrations;

public partial class RestoreCosmeticModel : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Synchronizes the EF model snapshot after legacy runtime-schema repairs.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Existing player data is intentionally preserved on downgrade.
    }
}
