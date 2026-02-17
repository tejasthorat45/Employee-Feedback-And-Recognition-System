using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FeedbackSystem.API.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceCategoryWithBadgeInRecognition : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackReview_Feedback",
                table: "FeedbackReview");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackReview_User",
                table: "FeedbackReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Recognition_Category",
                table: "Recognition");

            migrationBuilder.DropIndex(
                name: "IX_FeedbackReview_ReviewedBy",
                table: "FeedbackReview");

            migrationBuilder.RenameColumn(
                name: "CategoryId",
                table: "Recognition",
                newName: "BadgeId");

            migrationBuilder.RenameIndex(
                name: "IX_Recognition_CategoryId",
                table: "Recognition",
                newName: "IX_Recognition_BadgeId");

            migrationBuilder.AddColumn<string>(
                name: "ReviewerUserId",
                table: "FeedbackReview",
                type: "nvarchar(20)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Badges",
                columns: table => new
                {
                    BadgeId = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    BadgeName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(225)", maxLength: 225, nullable: true),
                    IconClass = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "GETDATE()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Badges", x => x.BadgeId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReview_ReviewerUserId",
                table: "FeedbackReview",
                column: "ReviewerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Badges_BadgeName",
                table: "Badges",
                column: "BadgeName",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackReview_Feedback_FeedbackId",
                table: "FeedbackReview",
                column: "FeedbackId",
                principalTable: "Feedback",
                principalColumn: "FeedbackId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackReview_Users_ReviewerUserId",
                table: "FeedbackReview",
                column: "ReviewerUserId",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Recognition_Badge",
                table: "Recognition",
                column: "BadgeId",
                principalTable: "Badges",
                principalColumn: "BadgeId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackReview_Feedback_FeedbackId",
                table: "FeedbackReview");

            migrationBuilder.DropForeignKey(
                name: "FK_FeedbackReview_Users_ReviewerUserId",
                table: "FeedbackReview");

            migrationBuilder.DropForeignKey(
                name: "FK_Recognition_Badge",
                table: "Recognition");

            migrationBuilder.DropTable(
                name: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_FeedbackReview_ReviewerUserId",
                table: "FeedbackReview");

            migrationBuilder.DropColumn(
                name: "ReviewerUserId",
                table: "FeedbackReview");

            migrationBuilder.RenameColumn(
                name: "BadgeId",
                table: "Recognition",
                newName: "CategoryId");

            migrationBuilder.RenameIndex(
                name: "IX_Recognition_BadgeId",
                table: "Recognition",
                newName: "IX_Recognition_CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_FeedbackReview_ReviewedBy",
                table: "FeedbackReview",
                column: "ReviewedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackReview_Feedback",
                table: "FeedbackReview",
                column: "FeedbackId",
                principalTable: "Feedback",
                principalColumn: "FeedbackId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_FeedbackReview_User",
                table: "FeedbackReview",
                column: "ReviewedBy",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Recognition_Category",
                table: "Recognition",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
