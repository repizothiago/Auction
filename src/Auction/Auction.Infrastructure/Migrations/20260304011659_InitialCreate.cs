using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Auction.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categories", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    user_type = table.Column<string>(type: "character varying(13)", maxLength: 13, nullable: false),
                    cnpj = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: true),
                    company_name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    cpf = table.Column<string>(type: "character varying(11)", maxLength: 11, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "auctions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    starting_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    starting_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    reserve_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    reserve_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    buy_now_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    buy_now_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: true),
                    current_price_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    current_price_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    bid_increment_amount = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    bid_increment_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "BRL"),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    seller_id = table.Column<Guid>(type: "uuid", nullable: false),
                    winner_id = table.Column<Guid>(type: "uuid", nullable: true),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_bids = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    current_winning_bid_id = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<long>(type: "bigint", nullable: false),
                    rules = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_auctions", x => x.id);
                    table.ForeignKey(
                        name: "FK_auctions_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

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

            migrationBuilder.CreateIndex(
                name: "idx_auctions_active_ending",
                table: "auctions",
                columns: new[] { "end_date", "status" },
                filter: "status = 'Active'");

            migrationBuilder.CreateIndex(
                name: "idx_auctions_category",
                table: "auctions",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "idx_auctions_end_date",
                table: "auctions",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "idx_auctions_seller",
                table: "auctions",
                column: "seller_id");

            migrationBuilder.CreateIndex(
                name: "idx_auctions_status",
                table: "auctions",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "idx_categories_name",
                table: "categories",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "idx_users_email",
                table: "users",
                column: "email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "auctions");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "categories");
        }
    }
}
