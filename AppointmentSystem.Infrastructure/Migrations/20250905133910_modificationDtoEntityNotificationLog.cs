using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class modificationDtoEntityNotificationLog : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationTemplateId",
                table: "NotificationLogs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NotificationTemplateId",
                table: "NotificationLogs",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}
