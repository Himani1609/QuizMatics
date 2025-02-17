using Microsoft.AspNetCore.Mvc;
using QuizMatics.Models;

namespace QuizMatics.Interfaces
{
    public interface ILessonService
    {
        Task<IEnumerable<LessonDto>> ListLessons();

        Task<LessonDto?> FindLesson(int id);

        Task<ServiceResponse> UpdateLesson(int id, UpdateLessonDto updatelessonDto);

        Task<ServiceResponse> AddLesson(AddLessonDto addlessonDto);

        Task<ServiceResponse> DeleteLesson(int id);

        Task<IEnumerable<ListQuizDto>> ListOfQuizzes(int id);

        Task<IEnumerable<LessonDto>> ListLessonsByTeacherId(int teacherId);


    }
}
