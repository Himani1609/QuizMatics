using QuizMatics.Interfaces;
using QuizMatics.Models;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Channels;


namespace QuizMatics.Services
{
    // The service layer contains all business logic and interacts with the database via Entity Framework Core.
    // It implements the interface.
    public class TeacherService : ITeacherService
    {
        private readonly ApplicationDbContext _context;

        // dependency injection of database context
        public TeacherService(ApplicationDbContext context)
        {
            _context = context;
        }


        /// <summary>
        /// Returns a list of Teachers including the Number of Teachers they created.
        /// </summary>
        /// <param name="TeacherDto">It includes ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// List of Teachers including ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// </returns>
        public async Task<IEnumerable<TeacherDto>> ListTeachers()
        {
            List<Teacher> Teachers = await _context.Teachers
                .Include(t => t.Lessons)
                .ThenInclude(t => t.Quizzes)
                .ToListAsync();

            List<TeacherDto> TeacherDtos = new List<TeacherDto>();

            foreach (Teacher Teacher in Teachers)
            {
                TeacherDtos.Add(new TeacherDto()
                {
                    TeacherId = Teacher.TeacherId,
                    Name = Teacher.Name,
                    TotalLessons = Teacher.Lessons?.Count() ?? 0,
                    TotalQuizzes = Teacher.Lessons != null ? Teacher.Lessons.SelectMany(l => l.Quizzes).Count() : 0
                });

            }
            // return TeacherDtos
            return TeacherDtos;
        }


        /// <summary>
        /// Return a Teacher specified by it's {id}
        /// </summary>
        /// /// <param name="id">Teacher's id</param>
        /// <param name="TeacherDto">It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// TeacherDto : It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// or
        /// null when there is no Teacher for that {id}
        /// </returns>
        /// TeacherDto? : it refers that TeacherDto is nullable here
        public async Task<TeacherDto?> FindTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Lessons)
                .ThenInclude(t => t.Quizzes)
                .FirstOrDefaultAsync(l => l.TeacherId == id);

            if (teacher == null)
            {
                return null;
            }

            TeacherDto teacherDto = new TeacherDto()
            {
                TeacherId = teacher.TeacherId,
                Name = teacher.Name,
                TotalLessons = teacher.Lessons?.Count() ?? 0,
                TotalQuizzes = teacher.Lessons != null ? teacher.Lessons.SelectMany(l => l.Quizzes).Count() : 0
            };

            return teacherDto;
        }



        /// <summary>
        /// It updates a Teacher
        /// </summary>
        /// <param name="id">The ID of Teacher which we want to update</param>
        /// <param name="UpdateTeacherDto">The required information to update the Teacher</param>
        /// <returns>
        /// SERVICE RESPONSE (meant for displaying messages for error handling and setting status for success)
        /// "Teacher not found." - If the Teacher does not exist.
        /// or
        /// "An error occurred updating the record" - if there is an error updating the teacher.
        /// or
        /// set status to Updated - If the update is successful, returns a success message.
        /// </returns>  
        public async Task<ServiceResponse> UpdateTeacher(int id, UpdateTeacherDto updateteacherDto)
        {
            ServiceResponse serviceResponse = new();

            Teacher? teacher = await _context.Teachers.FindAsync(id);

            if (teacher == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Teacher not found.");
                return serviceResponse;
            }

            // Update only the necessary fields
            teacher.Name = updateteacherDto.Name;
            teacher.Email = updateteacherDto.Email;

            // This tells Entity Framework(EF) that the record has changed
            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                // This commits the changes to the database
                await _context.SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("An error occurred updating the record");
                return serviceResponse;
            }

            // Set status as Updated
            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            return serviceResponse;
        }


        /// <summary>
        /// Adds a Teacher in the Teachers table
        /// </summary>
        /// <remarks>
        /// We add a Teacher as AddTeacherDto which is the required information we input to add a teacher
        /// and TeacherDto is the information about the Teacher displayed in the output
        /// </remarks>
        /// <param name="AddTeacherDto">The required information to add the Teacher</param>
        /// <returns>
        /// SERVICE RESPONSE
        ///  set status to Created- If the teacher is successfully added.
        ///  or
        ///  "There was an error adding the Teacher." - If there is any error adding teacher to the database
        /// </returns>
        public async Task<ServiceResponse> AddTeacher(AddTeacherDto addteacherDto)
        {
            ServiceResponse serviceResponse = new();

            Teacher teacher = new Teacher()
            {
                Name = addteacherDto.Name,
                Email = addteacherDto.Email
            };

            try
            {
                _context.Teachers.Add(teacher);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Teacher.");
                serviceResponse.Messages.Add(ex.Message);
                return serviceResponse;
            }

            // Set status as Created
            serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
            serviceResponse.CreatedId = teacher.TeacherId;
            return serviceResponse;
        }

        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Deleted - If deletion is successful, returns a success message.
        /// or
        /// "Error encountered while deleting the teacher" - If there is an error deleting the teacher
        /// or
        /// "Teacher cannot be deleted because it does not exist." - If the teacher does not exist.
        /// </returns>

        public async Task<ServiceResponse> DeleteTeacher(int id)
        {
            ServiceResponse serviceResponse = new();

            var Teacher = await _context.Teachers.FindAsync(id);
            if (Teacher == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Teacher cannot be deleted because it does not exist.");
                return serviceResponse;
            }

            try
            {
                _context.Teachers.Remove(Teacher);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Error encountered while deleting the teacher");
                return serviceResponse;
            }

            // Set status as Deleted
            serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
            return serviceResponse;
        }

    }
}
