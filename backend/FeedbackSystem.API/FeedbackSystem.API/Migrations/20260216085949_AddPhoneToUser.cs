using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackSystem.API.Migrations
{
    /// <inheritdoc />
<<<<<<<< HEAD:backend/FeedbackSystem.API/FeedbackSystem.API/Migrations/20260213082659_InitCreate.cs
    public partial class InitCreate : Migration
========
    public partial class AddPhoneToUser : Migration
>>>>>>>> 067218bfd231463a3f5cd4d68163708b50793ef7:backend/FeedbackSystem.API/FeedbackSystem.API/Migrations/20260216085949_AddPhoneToUser.cs
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Phone",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Phone",
                table: "Users");
        }
    }
}
