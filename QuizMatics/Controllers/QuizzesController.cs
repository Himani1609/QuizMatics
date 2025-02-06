using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using QuizMatics.Data.Migrations;
using QuizMatics.Models;

namespace QuizMatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public QuizzesController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Quizzes, each represented by a QuizDto along with it's Lessons count.
        /// </summary>
        /// <returns>
        /// 200 Ok
        /// List of quizzes with it's Id, Title, Description, Difficulty level, Max Minutes alloted for the quiz, Grade level, Total Lessons in which this quiz is a part of.
        /// </returns>
        /// <example>
        /// GET: api/Quizzes/List -> [{QuizId:1, Title:"Algebra Basics Quiz", Difficulty Level:0, Grade:9, MaxMinsAlotted:60, TotalLessons:2 },{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> ListQuizzes()
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
            // return 200 OK with QuizDtos
            return Ok(QuizDtos);
        }

        /// <summary>
        /// Returns a Quiz specified by its {id}, represented by an QuizDto along with it's lesson count.
        /// </summary>
        /// /// <param name="id">Quiz id</param>
        /// <returns>
        /// 200 Ok
        /// QuizDto : Quiz with it's given Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.
        /// or
        /// 404 Not Found when there is no Quiz of that id
        /// </returns>
        /// <example>
        /// GET: api/Quizzes/Find/{id} -> {QuizDto} represented as {QuizId:1, Title:"Algebra Basics Quiz", Difficulty Level:0, Grade:9, MaxMinsAlotted:60, TotalLessons:2 }
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<QuizDto>> FindQuiz(int id)
        {
            var quiz = await _context.Quizzes
                .Include(l => l.Lessons)
                .FirstOrDefaultAsync(l => l.QuizId == id);

            if (quiz == null)
            {
                return NotFound();
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

            return Ok(quizDto);
        }

        /// <summary>
        /// It updates a Quiz
        /// </summary>
        /// <param name="id">The ID of Quiz which we want to update</param>
        /// <param name="QuizDto">The required information to update the Quiz</param>
        /// <returns>
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 204 No Content
        /// </returns>       
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, UpdateQuizDto updatequizDto)
        {
            if (id != updatequizDto.QuizId)
            {
                return BadRequest();
            }

            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            quiz.Title = updatequizDto.Title;
            quiz.Description = updatequizDto.Description;   
            quiz.DifficultyLevel = updatequizDto.DifficultyLevel;
            quiz.Grade = updatequizDto.Grade;
            quiz.MaxMinsAlotted = updatequizDto.MaxMinsAlotted;


            _context.Entry(quiz).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!QuizExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }


        /// <summary>
        /// Adds a Quiz in the Quizzes table
        /// </summary>
        /// <remarks>
        /// We add a Quiz as AddQuizDto which is the required information we input to add a quiz
        /// and QuizDto is the information about the Quiz displayed in the output
        /// </remarks>
        /// <param name="AUQuizDto">The required information to add the Quiz</param>
        /// <param name="QuizDto">The output information displayed about the Quiz we added</param>
        /// <returns>
        /// 201 Created
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Quizzes/Add -> Add the Quiz in the Quizzes table
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<ActionResult<Quiz>> AddQuiz(AddQuizDto addquizDto)
        {

            var lesson = await _context.Lessons.FindAsync(addquizDto.LessonId);
            if (lesson == null)
            {
                return NotFound("Lesson not found.");
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

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();


            AddQuizDto quizDto = new AddQuizDto()
            {
                Title = quiz.Title,
                Description = quiz.Description,
                DateCreated = quiz.DateCreated,
                DifficultyLevel = quiz.DifficultyLevel,
                Grade = quiz.Grade,
                MaxMinsAlotted = quiz.MaxMinsAlotted,
                LessonId = addquizDto.LessonId


            };

            return CreatedAtAction("FindQuiz", new { id = quiz.QuizId }, quizDto);
        }

        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// 201 No Content
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Quizzes/Delete/{id} -> Deletes the quiz associated with {id}
        /// </example>
        [HttpDelete(template: "Delete/{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            var quiz = await _context.Quizzes.FindAsync(id);
            if (quiz == null)
            {
                return NotFound();
            }

            _context.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool QuizExists(int id)
        {
            return _context.Quizzes.Any(e => e.QuizId == id);
        }


        /// <summary>
        /// Returns a list of Lesson names linked to a specific Quiz.
        /// </summary>
        /// <param name="id">Quiz ID</param>
        /// <returns>
        /// 200 Ok - List of lesson names associated with the quiz.
        /// or
        /// 404 Not Found - If the quiz does not exist.
        /// </returns>
        /// <example>
        /// GET: api/Quizzes/ListOfLessons/1 -> Returns all lesson names associated with Quiz ID 1.
        /// </example>
        [HttpGet("ListOfLessons/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> ListOfLessons(int id)
        {
            // Fetch the quiz with its related lessons
            var quiz = await _context.Quizzes
                .Include(q => q.Lessons)
                .FirstOrDefaultAsync(q => q.QuizId == id);

            if (quiz == null)
            {
                return NotFound($"Quiz with ID {id} not found.");
            }

            // Return only the lesson titles (names)
            var lessonNames = quiz.Lessons?.Select(l => l.Title).ToList();

            return Ok(lessonNames);
        }




        /// <summary>
        /// Links an existing Quiz to a Lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// Returns a success message if the Quiz is linked to the Lesson.
        /// Returns a 404 if the Lesson or Quiz is not found.
        /// Returns a 400 if the Quiz is already linked to the Lesson.
        /// </returns>
        [HttpPost("LinkQuiz")]
        public async Task<IActionResult> LinkQuizToLesson(int lessonId, int quizId)
        {
            var lesson = await _context.Lessons.FindAsync(lessonId);
            var quiz = await _context.Quizzes.FindAsync(quizId);

            if (lesson == null || quiz == null)
            {
                return NotFound("Lesson or Quiz not found.");
            }

            if (lesson.Quizzes == null)
            {
                lesson.Quizzes = new List<Quiz>();
            }

            bool exists = await _context.Lessons
                .Where(l => l.LessonId == lessonId)
                .SelectMany(l => l.Quizzes)
                .AnyAsync(q => q.QuizId == quizId);

            if (exists)
            {
                return BadRequest("Quiz is already linked to this Lesson.");
            }

            lesson.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return Ok($"Quiz {quizId} linked to Lesson {lessonId}.");
        }


        /// <summary>
        /// Unlinks an existing Quiz from a Lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// Returns a success message if the Quiz is unlinked from the Lesson.
        /// Returns a 404 if the Lesson or Quiz is not found.
        /// Returns a 404 if the Quiz is not linked to the Lesson.
        /// </returns>
        [HttpDelete("UnlinkQuiz")]
        public async Task<IActionResult> UnlinkQuizFromLesson(int lessonId, int quizId)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null)
            {
                return NotFound("Lesson not found.");
            }

            var quiz = lesson.Quizzes.FirstOrDefault(q => q.QuizId == quizId);
            if (quiz == null)
            {
                return NotFound("Quiz is not linked to this Lesson.");
            }

            lesson.Quizzes.Remove(quiz);
            await _context.SaveChangesAsync();

            return Ok($"Quiz {quizId} unlinked from Lesson {lessonId}.");
        }


    }
}
