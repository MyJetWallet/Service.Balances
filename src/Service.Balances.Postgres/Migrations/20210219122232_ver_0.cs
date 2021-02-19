using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Service.Balances.Postgres.Migrations
{
    public partial class ver_0 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "balances");

            migrationBuilder.CreateTable(
                name: "balances",
                schema: "balances",
                columns: table => new
                {
                    AssetId = table.Column<string>(type: "text", nullable: false),
                    WalletId = table.Column<string>(type: "text", nullable: false),
                    BrokerId = table.Column<string>(type: "text", nullable: true),
                    ClientId = table.Column<string>(type: "text", nullable: true),
                    Balance = table.Column<double>(type: "double precision", precision: 20, nullable: false),
                    Reserve = table.Column<double>(type: "double precision", precision: 20, nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    SequenceId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_balances_balances", x => new { x.WalletId, x.AssetId });
                });

            migrationBuilder.CreateIndex(
                name: "IX_balances_balances_broker_client",
                schema: "balances",
                table: "balances",
                columns: new[] { "BrokerId", "ClientId" });

            migrationBuilder.CreateIndex(
                name: "IX_balances_balances_wallet",
                schema: "balances",
                table: "balances",
                column: "WalletId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "balances",
                schema: "balances");
        }
    }
}
