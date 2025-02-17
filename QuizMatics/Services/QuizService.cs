using QuizMatics.Interfaces;
using QuizMatics.Models;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using Microsoft.AspNetCore.Mvc;


namespace QuizMatics.Services
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;
        // dependency injection of database context
        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Quizzes, each represented by a QuizDto along with it's Lessons count.
        /// </summary>
        /// <param name="QuizDto">It includes Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// List of quizzes with it's Id, Title, Description, Difficulty level, Max Minutes alloted for the quiz, Grade level, Total Lessons in which this quiz is a part of.
        /// </returns>
        public async Task<IEnumerable<QuizDto>> ListQuizzes()
        {
            List<Quiz> Quizzes = await _context.Quizzes
                .Include(q => q.Lessons)
                .ToListAsync();

            List<QuizDto> QuizDtos = new List<QuizDto>();

            foreach (Quiz Quiz in Quizzes)
            {
                QuizDtos.Add(new QuizDto()
                {
                    QuizId = Quiz.QuizId,
                    Title = Quiz.Title,
                    Description = Quiz.Description,
                    DifficultyLevel = Quiz.DifficultyLevel,
                    Grade = Quiz.Grade,
                    MaxMinsAlotted = Quiz.MaxMinsAlotted,
                    TotalLessons = Quiz.Lessons?.Count() ?? 0,
                    LessonNames = Quiz.Lessons != null ? Quiz.Lessons.Select(l => l.Title).ToList() : new List<string>()

                });

            }
            // return QuizDtos
            return QuizDtos;
        }

        /// <summary>
        /// Returns a Quiz specified by its {id}, represented by an QuizDto along with it's lesson count.
        /// </summary>
        /// <param name="id">Quiz id</param>
        /// <param name="QuizDto">It includes Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// QuizDto : Quiz with it's given Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.
        /// or
        /// null: when there is no Quiz of that id
        /// </returns>
        public async Task<QuizDto?> FindQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(l => l.Lessons)
                .FirstOrDefaultAsync(l => l.QuizId == id);

            if (quiz == null)
            {
                return null;
            }

            QuizDto quizDto = new QuizDto()
            {
                QuizId = quiz.QuizId,
                Title = quiz.Title,
                Description = quiz.Description,
                DifficultyLevel = quiz.DifficultyLevel,
                Grade = quiz.Grade,
                MaxMinsAlotted = quiz.MaxMinsAlotted,
                TotalLessons = quiz.Lessons?.Count() ?? 0,
                LessonNames = quiz.Lessons.Select(q => q.Title).ToList()

            };

            return quizDto;
        }

        /// <summary>
        /// It updates a Quiz
        /// </summary>
        /// <param name="id">The ID of Quiz which we want to update</param>
        /// <param name="UpdateQuizDto">The required information to update the Quiz</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// 400 Bad Request - If the ID in the route does not match the Quiz ID.
        /// or
        /// "Quiz not found.": If the Quiz does not exist.
        /// or
        /// "An error occurred updating the record" - if there is an error updating the quiz.
        /// or
        /// set status to Updated - If the update is successful, returns a success message.
        /// </returns>       
        public async Task<ServiceResponse> UpdateQuiz(int id, UpdateQuizDto updatequizDto)
        {
            ServiceResponse serviceResponse = new();

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Quiz not found.");
                return serviceResponse;
            }

            // Update only the necessary fields
            quiz.Title = updatequizDto.Title;
            quiz.Description = updatequizDto.Description;
            quiz.DifficultyLevel = updatequizDto.DifficultyLevel;
            quiz.Grade = updatequizDto.Grade;
            quiz.MaxMinsAlotted = updatequizDto.MaxMinsAlotted;


            // This tells Entity Framework(EF) that the record has changed
            _context.Entry(quiz).State = EntityState.Modified;


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
        /// Adds a Quiz in the Quizzes table
        /// </summary>
        /// <remarks>
        /// We add a Quiz as AddQuizDto which is the required information we input to add a quiz
        /// and QuizDto is the information about the Quiz displayed in the output
        /// </remarks>
        /// <param name="AddQuizDto">The required information to add the Quiz</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Created - If the quiz is successfully added.
        /// or
        /// 404 Not Found: "Lesson not found.": If the specified Lesson does not exist.
        /// </returns>
        public async Task<ServiceResponse> AddQuiz(AddQuizDto addquizDto)
        {
            ServiceResponse serviceResponse = new();

            
            var lesson = await _context.Lessons.FindAsync(addquizDto.LessonId);
            if (lesson == null)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.NotFound;
                serviceResponse.Messages.Add("Lesson not found.");
                return serviceResponse;
            }

            
            Quiz quiz = new Quiz()
            {
                Title = addquizDto.Title,
                Description = addquizDto.Description,
                DateCreated = addquizDto.DateCreated,
                MaxMinsAlotted = addquizDto.MaxMinsAlotted,
                Grade = addquizDto.Grade,
                DifficultyLevel = addquizDto.DifficultyLevel,
                Lessons = new List<Lesson>() { lesson }
            };

            try
            {
                _context.Quizzes.Add(quiz);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                serviceResponse.Status = ServiceResponse.ServiceStatus.Error;
                serviceResponse.Messages.Add("There was an error adding the Quiz.");
                serviceResponse.Messages.Add(ex.Message);
                return serviceResponse;
            }

            serviceResponse.Status = ServiceResponse.ServiceStatus.Created;
            serviceResponse.CreatedId = quiz.QuizId;
            return serviceResponse;
        }


        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// 200 OK - If deletion is successful, returns a success message.
        /// or
        /// 404 Not Found
        /// </returns>
        public async Task<ServiceResponse> DeleteQuiz(int id)
        {
            ServiceResponse response = new();

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Quiz cannot be deleted because it does not exist.");
                return response;
            }

            try
            {
                _context.Quizzes.Remove(quiz);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                response.Status = ServiceResponse.ServiceStatus.Error;
                response.Messages.Add("Error encountered while deleting the quiz.");
                return response;
            }

            response.Status = ServiceResponse.ServiceStatus.Deleted;
            response.Messages.Add($"Quiz {id} deleted successfully.");
            return response;
        }


        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.QuizId == id);
        }


        /// <summary>
        /// Retrieves a list of lessons associated with a specific quiz identified by its ID.
        /// Each lesson is mapped to a <see cref="ListLessonDto"/> containing lesson details
        /// and the name of the teacher associated with the lesson.
        /// </summary>
        /// <param name="id">The ID of the quiz to retrieve lessons for.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// ListLessonDto - A list of lessons (with their title, description, date created, teacher name) associated with the quiz.
        /// or
        /// Empty list: []: If the quiz does not exist or if no lessons are found for the quiz.
        /// </returns>
        public async Task<IEnumerable<ListLessonDto>> ListOfLessons(int id)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Lessons)
                .ThenInclude(l => l.Teacher)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null || quiz.Lessons == null || !quiz.Lessons.Any()) 
            {
                return new List<ListLessonDto>();
            }

            return quiz.Lessons.Select(l => new ListLessonDto
            {
                LessonId = l.LessonId,
                Title = l.Title,
                Description = l.Description,
                DateCreated = l.DateCreated,
                Name = l.Teacher?.Name ?? "Unknown Teacher"
            }).ToList();
        }




        /// <summary>
        /// Links an existing Quiz to a Lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Updated: if the Quiz is linked to the Lesson.
        /// "Lesson or Quiz not found.": if the Lesson or Quiz is not found.
        /// "Quiz is already linked to this Lesson.": if the Quiz is already linked to the Lesson.
        /// </returns>
        public async Task<ServiceResponse> LinkQuizToLesson(int lessonId, int quizId)
        {
            ServiceResponse response = new();
         
            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            var quiz = await _context.Quizzes.FindAsync(quizId);

           
            if (lesson == null || quiz == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Lesson or Quiz not found.");
                return response;
            }


            if (lesson.Quizzes.Any(q => q.QuizId == quizId))
            {
                response.Status = ServiceResponse.ServiceStatus.AlreadyExists;
                response.Messages.Add("Quiz is already linked to this Lesson.");
                return response;
            }

            lesson.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            // set status to Updated
            response.Status = ServiceResponse.ServiceStatus.Updated;
            return response;
        }



        /// <summary>
        /// Unlinks an existing Quiz from a Lesson without deleting the Quiz and by removing the association in the junction table.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// SERVICE RESPONSE
        /// set status to Updated: if the Quiz is unlinked from the Lesson.
        /// "Lesson not found.": if the Lesson or Quiz is not found.
        /// "Quiz is not linked to this Lesson.": if the Quiz is not linked to the Lesson.
        /// </returns>
        public async Task<ServiceResponse> UnlinkQuizFromLesson(int lessonId, int quizId)
        {
            ServiceResponse response = new();

            
            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            
            if (lesson == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotFound;
                response.Messages.Add("Lesson or Quiz not found.");
                return response;
            }

            
            var quiz = lesson.Quizzes.FirstOrDefault(q => q.QuizId == quizId);
            if (quiz == null)
            {
                response.Status = ServiceResponse.ServiceStatus.NotLinked;
                response.Messages.Add("Quiz is not linked to this Lesson.");
                return response;
            }

            
            lesson.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            // set status to Updated
            response.Status = ServiceResponse.ServiceStatus.Updated;
            return response;
        }

    }
}
