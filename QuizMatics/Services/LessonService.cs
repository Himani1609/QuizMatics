using QuizMatics.Interfaces;
using QuizMatics.Models;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using System;
using Microsoft.AspNetCore.Mvc;

namespace QuizMatics.Services
{
    public class LessonService : ILessonService
    {
        private readonly ApplicationDbContext _context;
        // dependency injection of database context
        public LessonService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Lessons, each represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// <param name="LessonDto">It includes Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson</param>
        /// <returns>
        /// List of lessons with it's Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// </returns>
        public async Task<IEnumerable<LessonDto>> ListLessons()
        {
            List<Lesson> Lessons = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Quizzes)
                .ToListAsync();

            List<LessonDto> LessonDtos = new List<LessonDto>();

            foreach (Lesson Lesson in Lessons)
            {
                LessonDtos.Add(new LessonDto()
                {
                    LessonId = Lesson.LessonId,
                    Title = Lesson.Title,
                    Description = Lesson.Description,
                    DateCreated = Lesson.DateCreated,
                    Name = Lesson.Teacher.Name,
                    TotalQuizzes = Lesson.Quizzes?.Count() ?? 0,
                    QuizNames = Lesson.Quizzes != null ? Lesson.Quizzes.Select(q => q.Title).ToList() : new List<string>()

                });

            }
            // return LessonDtos
            return LessonDtos;
        }

        /// <summary>
        /// Returns a Lesson specified by its {id}, represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// <param name="id">Lesson id</param>
        /// <param name="LessonDto">It includes Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson</param>
        /// <returns>
        /// LessonDto : Lesson with it's given Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// or
        /// null: when there is no Lesson of that id
        /// </returns>
        public async Task<LessonDto?> FindLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
            {
                return null;
            }

            LessonDto lessonDto = new LessonDto()
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                DateCreated = lesson.DateCreated,
                Name = lesson.Teacher.Name,
                TotalQuizzes = lesson.Quizzes?.Count() ?? 0,
                QuizNames = lesson.Quizzes != null ? lesson.Quizzes.Select(q => q.Title).ToList() : new List<string>()
            };

            return lessonDto;
        }

        /// <summary>
        /// It updates a Lesson
        /// </summary>
        /// <param name="id">The ID of Lesson which we want to update</param>
        /// <param name="UpdateLessonDto">The required information to update the Lesson</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// "Lesson ID mismatch." - If the ID in the route does not match the Lesson ID.
        /// or
        /// "Lesson not found." or "Teacher not found." - If the Lesson or Teacher does not exist respectively.
        /// or
        /// set status to Updated - If the update is successful.
        /// or
        /// "An error occurred updating the record" -If there is an error updating the lesson.
        /// </returns>       
        public async Task<ServiceResponse> UpdateLesson(int id, UpdateLessonDto updatelessonDto)
        {
            ServiceResponse serviceResponse = new();

            if (id != updatelessonDto.LessonId)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Lesson ID mismatch.");
                return serviceResponse;
            }

            // finding lesson of that {id}
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Lesson not found.");
                return serviceResponse;
            }

            // finding teacher of the teacher id provided in updatelessonDto
            var teacher = await _context.Teachers.FindAsync(updatelessonDto.TeacherId);
            if (teacher == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Teacher not found.");
                return serviceResponse;
            }

            // Update only the necessary fields
            lesson.Title = updatelessonDto.Title;
            lesson.Description = updatelessonDto.Description;
            lesson.DateCreated = updatelessonDto.DateCreated;
            lesson.TeacherId = updatelessonDto.TeacherId;


            // This tells Entity Framework(EF) that the record has changed
            _context.Entry(lesson).State = EntityState.Modified;

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

            serviceResponse.Status = ServiceResponse.ServiceStatus.Updated;
            return serviceResponse;
            }


            private bool LessonExists(int id)
            {
                return _context.Lessons.Any(e => e.LessonId == id);
            }



        /// <summary>
        /// Adds a Lesson in the Lessons table
        /// </summary>
        /// <remarks>
        /// We add a Lesson as AddLessonDto which is the required information we input to add a lesson
        /// and LessonDto is the information about the Lesson displayed in the output
        /// </remarks>
        /// <param name="AddLessonDto">The required information to add the Lesson</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Created - If the Lesson is successfully added.
        /// or
        /// "There was an error adding the lesson." : If there is an error adding the lesson.
        /// or
        /// "Teacher Not Found.": If the specified Teacher does not exist.
        /// </returns>
        public async Task<ServiceResponse> AddLesson(AddLessonDto addlessonDto)
        {
            ServiceResponse serviceResponse = new();

            var teacher = await _context.Teachers.FindAsync(addlessonDto.TeacherId);
            if (teacher == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Teacher not found.");
                return serviceResponse;
            }

            Lesson lesson = new Lesson()
            {
                Title = addlessonDto.Title,
                Description = addlessonDto.Description,
                DateCreated = addlessonDto.DateCreated,
                TeacherId = addlessonDto.TeacherId
            };

            try
            {
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the lesson.");
                serviceResponse.Messages.Add(ex.Message);
                return serviceResponse;
            }

            // Set status as Created
            serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
            serviceResponse.CreatedId = lesson.LessonId;
            return serviceResponse;

        }


        /// <summary>
        /// Delete a Lesson specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Lesson we want to delete</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Deleted - If deletion is successful, returns a success message.
        /// or
        /// "Error encountered while deleting the lesson" : If there is any error deleting the lesson.
        /// or
        /// 404 Not Found:"Lesson cannot be deleted because it does not exist.": If the Lesson does not exist.
        /// </returns>
        public async Task<ServiceResponse> DeleteLesson(int id)
        {
            ServiceResponse serviceResponse = new();

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Lesson cannot be deleted because it does not exist.");
                return serviceResponse;
            }

            try
            {
                _context.Lessons.Remove(lesson);
                await _context.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("Error encountered while deleting the lesson");
                return serviceResponse;
            }

            // Set status as Deleted
            serviceResponse.Status = ServiceResponse.ServiceStatus.Deleted;
            return serviceResponse;
        }


        /// <summary>
        /// Returns a list of quizzes associated with a specific Lesson, including quiz details like title, grade, and difficulty.
        /// </summary>
        /// <param name="id">The ID of the lesson to retrieve quizzes for.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// ListQuizDto - A list of quizzes (with their title, grade, and difficulty level) associated with the lesson.
        /// or
        /// Empty list: []: If the lesson does not exist or if no quizzes are found for the lesson.
        /// </returns>
        public async Task<IEnumerable<ListQuizDto>> ListOfQuizzes(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null || lesson.Quizzes == null || !lesson.Quizzes.Any())
            {
                return new List<ListQuizDto>();
            }

            return lesson.Quizzes.Select(q => new ListQuizDto
            {
                QuizId = q.QuizId,
                Title = q.Title,
                Grade = q.Grade,
                DifficultyLevel = q.DifficultyLevel
            }).ToList();
        }






        // List lessons by teacher ID
        public async Task<IEnumerable<LessonDto>> ListLessonsByTeacherId(int teacherId)
        {
            return await _context.Lessons
                .Where(l => l.TeacherId == teacherId) // Assuming 'TeacherId' is the foreign key in 'Lessons'
                .Select(l => new LessonDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Description = l.Description,
                    DateCreated = l.DateCreated
                })
                .ToListAsync();
        }


    }
}
