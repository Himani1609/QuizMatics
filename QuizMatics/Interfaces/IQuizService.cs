using QuizMatics.Models;

namespace QuizMatics.Interfaces
{
    public interface IQuizService
    {

        Task<IEnumerable<QuizDto>> ListQuizzes();

        Task<QuizDto?> FindQuiz(int id);

        Task<ServiceResponse> UpdateQuiz(int id, UpdateQuizDto updatequizDto);

        Task<ServiceResponse> AddQuiz(AddQuizDto addquizDto);

        Task<ServiceResponse> DeleteQuiz(int id);

        Task<IEnumerable<ListLessonDto>> ListOfLessons(int id);

        Task<ServiceResponse> LinkQuizToLesson(int lessonId, int quizId);

        Task<ServiceResponse> UnlinkQuizFromLesson(int lessonId, int quizId);
    }
}
