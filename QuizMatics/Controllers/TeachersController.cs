using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using QuizMatics.Models;

namespace QuizMatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TeachersController(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Returns a list of Teachers including the Number of Teachers they created.
        /// </summary>
        /// <param name="TeacherDto">It includes ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// 200 Ok
        /// List of Teachers including ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// </returns>
        /// <example>
        /// GET: api/Teachers/List -> [{TeacherId: 1, Name: "Apurva", TotalTeachers: 2, TotalQuizzes:2},{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<TeacherDto>>> ListTeachers()
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
                    TeacherId =Teacher.TeacherId,
                    Name = Teacher.Name,
                    TotalLessons = Teacher.Lessons?.Count() ?? 0,
                    TotalQuizzes = Teacher.Lessons != null ? Teacher.Lessons.SelectMany(l => l.Quizzes).Count() : 0
                });

            }
            // return 200 OK with TeacherDtos
            return Ok(TeacherDtos);
        }


        /// <summary>
        /// Return a Teacher specified by it's {id}
        /// </summary>
        /// /// <param name="id">Teacher's id</param>
        /// <param name="TeacherDto">It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// 200 Ok
        /// TeacherDto : It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// or
        /// 404 Not Found when there is no Teacher for that {id}
        /// </returns>
        /// <example>
        /// GET: api/Teachers/Find/{id} -> {TeacherId: 1, Name: "Apurva", TotalTeachers: 2, TotalQuizzes:2}
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<TeacherDto>> FindTeacher(int id)
        {
            var teacher = await _context.Teachers
                .Include(t => t.Lessons)
                .ThenInclude(t => t.Quizzes)
                .FirstOrDefaultAsync(l => l.TeacherId == id);

            if (teacher == null)
            {
                return NotFound();
            }

            TeacherDto teacherDto = new TeacherDto()
            {
                TeacherId = teacher.TeacherId,
                Name = teacher.Name,
                TotalLessons = teacher.Lessons?.Count() ?? 0,
                TotalQuizzes = teacher.Lessons != null ? teacher.Lessons.SelectMany(l => l.Quizzes).Count() : 0
            };

            return Ok(teacherDto);
        }

        /// <summary>
        /// It updates a Teacher
        /// </summary>
        /// <param name="id">The ID of Teacher which we want to update</param>
        /// <param name="UpdateTeacherDto">The required information to update the Teacher</param>
        /// <returns>
        /// 400 Bad Request - If the ID in the route does not match the Teacher ID.
        /// or
        /// 404 Not Found - If the Teacher does not exist.
        /// or
        /// 200 OK - If the update is successful, returns a success message.
        /// </returns>       
        [HttpPut(template:"Update/{id}")]
        public async Task<IActionResult> UpdateTeacher(int id, UpdateTeacherDto updateteacherDto)
        {
            if (id != updateteacherDto.TeacherId)
            {
                return BadRequest(new { message = "Teacher ID mismatch." });
            }

            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found." });
            }


            // Update only the necessary fields
            teacher.Name = updateteacherDto.Name;
            teacher.Email = updateteacherDto.Email;

            _context.Entry(teacher).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TeacherExists(id))
                {
                    return NotFound(new { message = "Teacher not found after concurrency check." });
                }
                else
                {
                    throw;
                }
            }

            return Ok(new { message = $"Teacher {id} updated successfully." });
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
        /// 201 Created - If the teacher is successfully added.
        /// </returns>
        /// <example>
        /// api/Teachers/Add -> Add the Teacher 
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<ActionResult<Teacher>> AddTeacher(AddTeacherDto addteacherDto)
        {

            Teacher teacher = new Teacher()
            {
                Name = addteacherDto.Name,
                Email = addteacherDto.Email
            };

            _context.Teachers.Add(teacher);
            await _context.SaveChangesAsync();


            AddTeacherDto teacherDto = new AddTeacherDto()
            {
                Name = teacher.Name,
                Email = teacher.Email
            };

            return CreatedAtAction(nameof(FindTeacher), new { id = teacher.TeacherId },
            new { message = $"Teacher {teacher.TeacherId} added successfully.", teacherId = teacher.TeacherId });
        }

        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// 2200 OK - If deletion is successful, returns a success message.
        /// or
        /// 404 Not Found - If the teacher does not exist.
        /// </returns>
        /// <example>
        /// api/Quizzes/Delete/{id} -> Deletes the quiz associated with {id}
        /// </example>
        [HttpDelete(template:"Delete/{id}")]
        public async Task<IActionResult> DeleteTeacher(int id)
        {
            var teacher = await _context.Teachers.FindAsync(id);
            if (teacher == null)
            {
                return NotFound(new { message = "Teacher not found." });
            }

            _context.Teachers.Remove(teacher);
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Teacher {id} deleted successfully." });
        }

        private bool TeacherExists(int id)
        {
            return _context.Teachers.Any(e => e.TeacherId == id);
        }
    }
}
