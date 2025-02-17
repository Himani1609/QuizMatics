namespace QuizMatics.Models.ViewModels
{
    public class QuizDetails
    {
        public QuizDto Quiz { get; set; }
        public List<ListLessonDto> Lessons { get; set; }
    }
}
