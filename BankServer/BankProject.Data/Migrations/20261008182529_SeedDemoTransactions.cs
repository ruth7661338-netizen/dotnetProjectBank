using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BankProject.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Transactions",
                columns: new[] { "Id", "AccountId", "Amount", "BalanceAfter", "Date", "Note", "Type" },
                values: new object[,]
                {
                    { 1, 1, 8000m, 8000m, new DateTime(2026, 9, 1, 9, 0, 0, 0, DateTimeKind.Utc), "יתרת פתיחה", 0 },
                    { 2, 1, 5000m, 3000m, new DateTime(2026, 9, 5, 10, 30, 0, 0, DateTimeKind.Utc), "העברה לחשבון 1000-0002", 2 },
                    { 3, 1, 2500m, 5500m, new DateTime(2026, 9, 10, 8, 15, 0, 0, DateTimeKind.Utc), "משכורת", 0 },
                    { 4, 1, 500m, 5000m, new DateTime(2026, 9, 18, 17, 45, 0, 0, DateTimeKind.Utc), "משיכה מכספומט", 1 },
                    { 5, 2, 10000m, 10000m, new DateTime(2026, 9, 1, 9, 5, 0, 0, DateTimeKind.Utc), "יתרת פתיחה", 0 },
                    { 6, 2, 5000m, 15000m, new DateTime(2026, 9, 5, 10, 30, 0, 0, DateTimeKind.Utc), "העברה מחשבון 1000-0001", 3 },
                    { 7, 3, 4000m, 4000m, new DateTime(2026, 9, 2, 11, 0, 0, 0, DateTimeKind.Utc), "יתרת פתיחה", 0 },
                    { 8, 3, 800m, 3200m, new DateTime(2026, 9, 20, 14, 20, 0, 0, DateTimeKind.Utc), "תשלום חשבון חשמל", 1 }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CustomerId", "Email", "FullName", "PasswordHash", "Role" },
                values: new object[] { 100, 2, "rachel@bank.local", "רחל כהן", "100000.AAECAwQFBgcICQoLDA0ODw==.oWtrAeSs7kbOgbjzpTpGVXeHiOWTIZMNxApazrC9MAk=", 0 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Transactions",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 100);
        }
    }
}
