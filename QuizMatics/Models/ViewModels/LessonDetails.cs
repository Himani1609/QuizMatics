namespace QuizMatics.Models.ViewModels
{
    public class LessonDetails
    {
        public required LessonDto Lesson { get; set; }
        public required IEnumerable<ListQuizDto> ListQuizzes { get; set; }

        public required IEnumerable<ListQuizDto> AvailableQuizzes { get; set; }
    }
}
