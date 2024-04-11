using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVFaceRecognitionAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateHistoryRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TypeActions",
                columns: table => new
                {
                    ActionId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActionDescription = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypeActions", x => x.ActionId);
                });

            // Добавление начальных данных в таблицу TypeActions при создании базы данных
            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 1, "Авторизация в систему" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 2, "Выход из системы" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 3, "Создание изображения" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 4, "Редактирование изображения" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 5, "Удаление изображения" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 6, "Создание пользователя" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 7, "Редактирование пользователя" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 8, "Смена статуса пользователю" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 9, "Обновление пароля" });

            migrationBuilder.InsertData(
                table: "TypeActions",
                columns: new[] { "ActionId", "ActionDescription" },
                values: new object[] { 10, "Удаление пользователя" });

            migrationBuilder.CreateTable(
                name: "HistoryRecords",
                columns: table => new
                {
                    HistoryRecordId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DateAction = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TypeActionId = table.Column<long>(type: "bigint", nullable: false),
                    UserId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistoryRecords", x => x.HistoryRecordId);
                    table.ForeignKey(
                        name: "FK_HistoryRecords_TypeActions_TypeActionId",
                        column: x => x.TypeActionId,
                        principalTable: "TypeActions",
                        principalColumn: "ActionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HistoryRecords_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_TypeActionId",
                table: "HistoryRecords",
                column: "TypeActionId");

            migrationBuilder.CreateIndex(
                name: "IX_HistoryRecords_UserId",
                table: "HistoryRecords",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistoryRecords");

            migrationBuilder.DropTable(
                name: "TypeActions");
        }
    }
}
