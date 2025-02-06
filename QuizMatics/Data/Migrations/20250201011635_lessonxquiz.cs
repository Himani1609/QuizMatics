using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizMatics.Data.Migrations
{
    /// <inheritdoc />
    public partial class lessonxquiz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LessonQuiz",
                columns: table => new
                {
                    LessonsLessonId = table.Column<int>(type: "int", nullable: false),
                    QuizzesQuizId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonQuiz", x => new { x.LessonsLessonId, x.QuizzesQuizId });
                    table.ForeignKey(
                        name: "FK_LessonQuiz_Lessons_LessonsLessonId",
                        column: x => x.LessonsLessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonQuiz_Quizzes_QuizzesQuizId",
                        column: x => x.QuizzesQuizId,
                        principalTable: "Quizzes",
                        principalColumn: "QuizId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonQuiz_QuizzesQuizId",
                table: "LessonQuiz",
                column: "QuizzesQuizId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonQuiz");
        }
    }
}
