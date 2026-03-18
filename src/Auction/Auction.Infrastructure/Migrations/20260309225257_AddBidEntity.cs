using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBidEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("1563318e-7548-4620-a936-9d0ed752f2bc"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("28477285-2e12-43a6-8270-de0bf9519d20"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("2a0e3fab-1ef8-4b95-8e4a-c10277e159e7"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("71b730fb-7549-4cb2-bdb2-091b0e7652bd"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("75bbd8cf-c16c-4428-a9ca-e39b732b5a84"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("7f7fa068-16c8-43e0-b2c5-20e50311d474"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("984bc72b-4c10-4826-8024-6c478f0c3c60"));

            migrationBuilder.CreateTable(
                name: "bids",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    auction_id = table.Column<Guid>(type: "uuid", nullable: false),
                    bidder_id = table.Column<Guid>(type: "uuid", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    is_auto_bid = table.Column<bool>(type: "boolean", nullable: false),
                    bid_status = table.Column<int>(type: "integer", nullable: false),
                    bid_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bids", x => x.id);
                    table.ForeignKey(
                        name: "FK_bids_auctions_auction_id",
                        column: x => x.auction_id,
                        principalTable: "auctions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_bids_users_bidder_id",
                        column: x => x.bidder_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "created_at", "description", "name", "updated_at", "version" },
                values: new object[,]
                {
                    { new Guid("09ede924-5eac-4ccc-88ef-bedd335ece17"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4381), "Obras de arte e colecionáveis", "Arte", null, 0L },
                    { new Guid("0ec67cca-1ece-4e11-bd70-5b335774345e"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4383), "Itens antigos e vintage", "Antiguidades", null, 0L },
                    { new Guid("448fb831-8414-4181-81dd-318b5a569194"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4376), "Carros, motos e outros veículos", "Veículos", null, 0L },
                    { new Guid("62fb8846-ac7d-481f-8b91-e19941180753"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(1586), "Dispositivos eletrônicos e gadgets", "Eletrônicos", null, 0L },
                    { new Guid("71b9be8f-278e-4a86-a945-61e71edd134e"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4384), "Outras categorias", "Outros", null, 0L },
                    { new Guid("c9d47c38-d3ea-423d-a089-5ba55e14055e"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4380), "Casas, apartamentos e terrenos", "Imóveis", null, 0L },
                    { new Guid("ddebd330-f35b-4e8f-b820-e110168be0bf"), new DateTime(2026, 3, 9, 22, 52, 56, 798, DateTimeKind.Utc).AddTicks(4382), "Joias e pedras preciosas", "Joias", null, 0L }
                });

            migrationBuilder.CreateIndex(
                name: "ix_bids_auction_id",
                table: "bids",
                column: "auction_id");

            migrationBuilder.CreateIndex(
                name: "ix_bids_auction_status",
                table: "bids",
                columns: new[] { "auction_id", "bid_status" });

            migrationBuilder.CreateIndex(
                name: "ix_bids_bid_time",
                table: "bids",
                column: "bid_time");

            migrationBuilder.CreateIndex(
                name: "ix_bids_bidder_id",
                table: "bids",
                column: "bidder_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bids");

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("09ede924-5eac-4ccc-88ef-bedd335ece17"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("0ec67cca-1ece-4e11-bd70-5b335774345e"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("448fb831-8414-4181-81dd-318b5a569194"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("62fb8846-ac7d-481f-8b91-e19941180753"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("71b9be8f-278e-4a86-a945-61e71edd134e"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("c9d47c38-d3ea-423d-a089-5ba55e14055e"));

            migrationBuilder.DeleteData(
                table: "categories",
                keyColumn: "id",
                keyValue: new Guid("ddebd330-f35b-4e8f-b820-e110168be0bf"));

            migrationBuilder.InsertData(
                table: "categories",
                columns: new[] { "id", "created_at", "description", "name", "updated_at", "version" },
                values: new object[,]
                {
                    { new Guid("1563318e-7548-4620-a936-9d0ed752f2bc"), new DateTime(2026, 3, 4, 1, 16, 58, 871, DateTimeKind.Utc).AddTicks(9170), "Dispositivos eletrônicos e gadgets", "Eletrônicos", null, 0L },
                    { new Guid("28477285-2e12-43a6-8270-de0bf9519d20"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3437), "Carros, motos e outros veículos", "Veículos", null, 0L },
                    { new Guid("2a0e3fab-1ef8-4b95-8e4a-c10277e159e7"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3452), "Itens antigos e vintage", "Antiguidades", null, 0L },
                    { new Guid("71b730fb-7549-4cb2-bdb2-091b0e7652bd"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3450), "Joias e pedras preciosas", "Joias", null, 0L },
                    { new Guid("75bbd8cf-c16c-4428-a9ca-e39b732b5a84"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3453), "Outras categorias", "Outros", null, 0L },
                    { new Guid("7f7fa068-16c8-43e0-b2c5-20e50311d474"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3446), "Casas, apartamentos e terrenos", "Imóveis", null, 0L },
                    { new Guid("984bc72b-4c10-4826-8024-6c478f0c3c60"), new DateTime(2026, 3, 4, 1, 16, 58, 872, DateTimeKind.Utc).AddTicks(3448), "Obras de arte e colecionáveis", "Arte", null, 0L }
                });
        }
    }
}
