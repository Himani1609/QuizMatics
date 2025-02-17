using QuizMatics.Models;

namespace QuizMatics.Interfaces
{

    // It defines contracts for the service layer.
    // It define the methods that a service must implement.
    public interface ITeacherService
    {
        Task<IEnumerable<TeacherDto>> ListTeachers();

        Task<TeacherDto?> FindTeacher(int id);

        Task<ServiceResponse> UpdateTeacher(int id, UpdateTeacherDto updateteacherDto);

        Task<ServiceResponse> AddTeacher(AddTeacherDto addteacherDto);

        Task<ServiceResponse> DeleteTeacher(int id);
    }
}
