using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RVFaceRecognitionAPI.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserRoleAndUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "UserStatus",
                table: "Users",
                newName: "UserStatusId");

            migrationBuilder.RenameColumn(
                name: "UserRole",
                table: "Users",
                newName: "UserRoleId");

            migrationBuilder.CreateTable(
                name: "UserRoles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleTitle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRoles", x => x.RoleId);
                });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "RoleTitle" },
                values: new object[] { 1, "Пользователь" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "RoleTitle" },
                values: new object[] { 2, "Наблюдатель" });

            migrationBuilder.InsertData(
                table: "UserRoles",
                columns: new[] { "RoleId", "RoleTitle" },
                values: new object[] { 3, "Администратор" });

            migrationBuilder.CreateTable(
                name: "UserStatuses",
                columns: table => new
                {
                    StatusId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StatusTitle = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserStatuses", x => x.StatusId);
                });

            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: new[] { "StatusId", "StatusTitle" },
                values: new object[] { 1, "Активен" });

            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: new[] { "StatusId", "StatusTitle" },
                values: new object[] { 2, "Заблокирован" });

            migrationBuilder.InsertData(
                table: "UserStatuses",
                columns: new[] { "StatusId", "StatusTitle" },
                values: new object[] { 3, "Удалён" });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "UserId", "UserRoleId", "UserStatusId", "FullName", "Photo", "Login", "Password" },
                values: new object[] { 1, 3, 1, "Admin", null, "rvtech\\admin", BCrypt.Net.BCrypt.HashPassword("P@ssw0rd") });

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserRoleId",
                table: "Users",
                column: "UserRoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_UserStatusId",
                table: "Users",
                column: "UserStatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users",
                column: "UserRoleId",
                principalTable: "UserRoles",
                principalColumn: "RoleId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_UserStatuses_UserStatusId",
                table: "Users",
                column: "UserStatusId",
                principalTable: "UserStatuses",
                principalColumn: "StatusId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserRoles_UserRoleId",
                table: "Users");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_UserStatuses_UserStatusId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "UserRoles");

            migrationBuilder.DropTable(
                name: "UserStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserRoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_UserStatusId",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "UserStatusId",
                table: "Users",
                newName: "UserStatus");

            migrationBuilder.RenameColumn(
                name: "UserRoleId",
                table: "Users",
                newName: "UserRole");
        }
    }
}
