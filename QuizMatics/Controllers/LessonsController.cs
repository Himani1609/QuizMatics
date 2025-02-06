using System;
using System.Collections.Generic;
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
    public class LessonsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LessonsController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Lessons, each represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// <returns>
        /// 200 Ok
        /// List of lessons with it's Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// </returns>
        /// <example>
        /// GET: api/Lessons/List -> [{LessonId:1, Title:"Algebra Basics", DateCreated:2025-01-01, Name: "Apurva",TotalQuizzes: 1},{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> ListLessons()
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
                    DateCreated = Lesson.DateCreated,
                    Name = Lesson.Teacher.Name,
                    TotalQuizzes = Lesson.Quizzes?.Count() ?? 0,
                    QuizNames = Lesson.Quizzes != null ? Lesson.Quizzes.Select(q => q.Title).ToList() : new List<string>()

                });

            }
            // return 200 OK with LessonDtos
            return Ok(LessonDtos);
        }

        /// <summary>
        /// Returns a Lesson specified by its {id}, represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// /// <param name="id">Lesson id</param>
        /// <returns>
        /// 200 Ok
        /// LessonDto : Lesson with it's given Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// or
        /// 404 Not Found when there is no Lesson of that id
        /// </returns>
        /// <example>
        /// GET: api/Lessons/Find/{id} -> {LessonDto} represented as {LessonId:1, Title:"Algebra Basics", DateCreated:2025-01-01, Name: "Apurva",TotalQuizzes: 1}
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<LessonDto>> FindLesson(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Teacher)
                .Include(l => l.Quizzes)
                .FirstOrDefaultAsync(l => l.LessonId == id); 

            if ( lesson == null)
            {
                return NotFound();
            }

            LessonDto lessonDto = new LessonDto()
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                DateCreated = lesson.DateCreated,
                Name = lesson.Teacher.Name,
                TotalQuizzes = lesson.Quizzes?.Count() ?? 0,
                QuizNames = lesson.Quizzes != null ? lesson.Quizzes.Select(q => q.Title).ToList() : new List<string>()
            };

            return Ok(lessonDto);
        }

        /// <summary>
        /// It updates a Lesson
        /// </summary>
        /// <param name="id">The ID of Lesson which we want to update</param>
        /// <param name="LessonDto">The required information to update the Lesson</param>
        /// <returns>
        /// 400 Bad Request
        /// or
        /// 404 Not Found
        /// or
        /// 204 No Content
        /// </returns>       
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto updatelessonDto)
        {
            if (id != updatelessonDto.LessonId)
            {
                return BadRequest();
            }

            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            var teacher = await _context.Teachers.FindAsync(updatelessonDto.TeacherId);
            if (teacher == null)
            {
                return NotFound();
            }

            // Update only the necessary fields
            lesson.Title = updatelessonDto.Title;
            lesson.Description = updatelessonDto.Description;
            lesson.DateCreated = updatelessonDto.DateCreated;
            lesson.TeacherId = updatelessonDto.TeacherId;

            _context.Entry(lesson).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LessonExists(id))
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
        /// Adds a Lesson in the Lessons table
        /// </summary>
        /// <remarks>
        /// We add a Lesson as AddLessonDto which is the required information we input to add a lesson
        /// and LessonDto is the information about the Lesson displayed in the output
        /// </remarks>
        /// <param name="AddLessonDto">The required information to add the Lesson</param>
        /// <param name="LessonDto">The output information displayed about the Lesson we added</param>
        /// <returns>
        /// 201 Created
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Lessons/Add -> Add the Lesson 
        /// </example>
        [HttpPost(template:"Add")]
        public async Task<ActionResult<Lesson>> AddLesson(AddLessonDto addlessonDto)
        {
            var teacher = await _context.Teachers.FindAsync(addlessonDto.TeacherId);
            if (teacher == null)
            {
                return NotFound();  
            }

            Lesson lesson = new Lesson()
            {
                Title = addlessonDto.Title,
                Description = addlessonDto.Description,
                DateCreated = addlessonDto.DateCreated,
                TeacherId = addlessonDto.TeacherId
            };

            _context.Lessons.Add(lesson);
            await _context.SaveChangesAsync();

            
            LessonDto lessonDto = new LessonDto()
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Description = lesson.Description,
                DateCreated = lesson.DateCreated,
                Name = lesson.Teacher.Name
            };

            return CreatedAtAction("FindLesson", new { id = lesson.LessonId }, lessonDto);
        }

        /// <summary>
        /// Delete a Lesson specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Lesson we want to delete</param>
        /// <returns>
        /// 201 No Content
        /// or
        /// 404 Not Found
        /// </returns>
        /// <example>
        /// api/Lessons/Delete/{id} -> Deletes the lesson associated with {id}
        /// </example>
        [HttpDelete(template:"Delete/{id}")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var lesson = await _context.Lessons.FindAsync(id);
            if (lesson == null)
            {
                return NotFound();
            }

            _context.Lessons.Remove(lesson);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LessonExists(int id)
        {
            return _context.Lessons.Any(e => e.LessonId == id);
        }

        /// <summary>
        /// Returns a list of Quiz names linked to a specific Lesson.
        /// </summary>
        /// <param name="id">Lesson ID</param>
        /// <returns>
        /// 200 Ok - List of quiz names associated with the lesson.
        /// or
        /// 404 Not Found - If the lesson does not exist.
        /// </returns>
        /// <example>
        /// GET: api/Lessons/ListOfQuizzes/1 -> Returns all quiz names associated with Lesson ID 1.
        /// </example>
        [HttpGet("ListOfQuizzes/{id}")]
        public async Task<ActionResult<IEnumerable<string>>> ListOfQuizzes(int id)
        {
            var lesson = await _context.Lessons
                .Include(l => l.Quizzes)  // Load related quizzes
                .FirstOrDefaultAsync(l => l.LessonId == id);

            if (lesson == null)
            {
                return NotFound($"Lesson with ID {id} not found.");
            }

            if (lesson.Quizzes == null || !lesson.Quizzes.Any())
            {
                return NotFound($"No quizzes found for Lesson ID {id}.");
            }

            // Return only the quiz titles (names)
            var quizNames = lesson.Quizzes.Select(q => q.Title).ToList();

            return Ok(quizNames);
        }



    }
}
