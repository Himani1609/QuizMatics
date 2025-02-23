using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using QuizMatics.Data.Migrations;
using QuizMatics.Interfaces;
using QuizMatics.Models;
using QuizMatics.Services;

namespace QuizMatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LessonsController : ControllerBase
    {
        private readonly ILessonService _lessonservice;

        public LessonsController(ILessonService context)
        {
            _lessonservice = context;
        }

        /// <summary>
        /// Returns a list of Lessons, each represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// <param name="LessonDto">It includes Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// List of lessons with it's Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// </returns>
        /// <example>
        /// GET: api/Lessons/List -> [{LessonId:1, Title:"Algebra Basics", DateCreated:2025-01-01, Name: "Apurva",TotalQuizzes: 1},{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<LessonDto>>> ListLessons()
        {
            IEnumerable<LessonDto> LessonDtos = await _lessonservice.ListLessons();

            // return 200 OK with TeacherDtos
            return Ok(LessonDtos);
        }

        /// <summary>
        /// Returns a Lesson specified by its {id}, represented by an LessonDto with it's associated Teacher and Quizzes.
        /// </summary>
        /// <param name="id">Lesson id</param>
        /// <param name="LessonDto">It includes Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// LessonDto : Lesson with it's given Id, Title, Date Created, Teacher Name who created the lesson, and Number of Quizzes in a Lesson
        /// or
        /// 404 Not Found: "No lesson found for that ID {id}":  when there is no Lesson of that id
        /// </returns>
        /// <example>
        /// GET: api/Lessons/Find/1 -> {LessonDto} represented as {LessonId:1, Title:"Algebra Basics", DateCreated:2025-01-01, Name: "Apurva",TotalQuizzes: 1}
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<LessonDto>> FindLesson(int id)
        {
            var lesson = await _lessonservice.FindLesson(id);

            if (lesson == null)
            {
                return NotFound($"No lesson found for that ID {id}");
            }
            else
            {
                return Ok(lesson);
            }
        }

        /// <summary>
        /// It updates a Lesson
        /// </summary>
        /// <param name="id">The ID of Lesson which we want to update</param>
        /// <param name="UpdateLessonDto">The required information to update the Lesson</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 400 Bad Request - If the ID in the route does not match the Lesson ID.
        /// or
        /// 404 Not Found - If the Lesson or Teacher does not exist.
        /// or
        /// 204 No Content - If the update is successful, returns a success message.
        /// </returns>       
        [HttpPut("Update/{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateLesson(int id, UpdateLessonDto updatelessonDto)
        {
            if (id != updatelessonDto.LessonId)
            {
                return BadRequest(new { message = "Lesson ID mismatch." });
            }

            ServiceResponse response = await _lessonservice.UpdateLesson(id, updatelessonDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Lesson not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the lesson." });
            }

            return Ok(new { message = $"Lesson with ID {id} updated successfully." });
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
        /// HTTP RESPONSE
        /// 201 Created: "Lesson added successfully with ID {response.CreatedId}": If the Lesson is successfully added.
        /// or
        /// 404 Not Found: "Teacher not found. Cannot add lesson.": If the teacher for TeacherId added in request body is not found
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while adding the lesson." : If there is an error adding lesson.
        /// </returns>
        /// <example>
        /// api/Lessons/Add -> Add the Lesson 
        /// </example>
        [HttpPost(template:"Add")]
        public async Task<ActionResult> AddLesson(AddLessonDto addlessonDto)
        {
            ServiceResponse response = await _lessonservice.AddLesson(addlessonDto);
            
            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Teacher not found. Cannot add lesson." }); 
            }

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the lesson." });
            }

            return CreatedAtAction("FindLesson", new { id = response.CreatedId }, new
            {
                message = $"Lesson added successfully with ID {response.CreatedId}",
                lessonId = response.CreatedId
            });
        }

        /// <summary>
        /// Delete a Lesson specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Lesson we want to delete</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 OK: "Lesson with ID {id} deleted successfully.": If deletion is successful, returns a success message.
        /// or
        /// 404 Not Found: "Teacher not found.": If the teacher does not exist.
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while deleting the lesson.": If there is an error deleting the lesson.
        /// </returns>
        /// <example>
        /// api/Lessons/Delete/{id} -> Deletes the lesson associated with {id}
        /// </example>
        [HttpDelete(template:"Delete/{id}")]
        [Authorize]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            ServiceResponse response = await _lessonservice.DeleteLesson(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Lesson not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the teacher." });
            }

            return Ok(new { message = $"Lesson with ID {id} deleted successfully." });
        }


        /// <summary>
        /// Returns a list of quizzes associated with a specific Lesson, including quiz details like title, grade, and difficulty.
        /// </summary>
        /// <param name="id">The ID of the lesson to retrieve quizzes for.</param>
        /// <returns>
        /// HTTP RESPONSE
        /// ListQuizDto - A list of quizzes (with their title, grade, and difficulty level) associated with the lesson.
        /// or
        /// 404 Not Found: "No quizzes found for Lesson ID {id}.": If the lesson does not exist for that {id} or if no quizzes are found for the lesson.
        /// </returns>
        /// <example>
        /// api/Lessons/ListOfQuizzes/9 ->
        /// [
        ///{
        ///  "quizId": 10,
        ///  "title": "Geometry Quiz Challenge",
        ///  "grade": 7,
        ///  "difficultyLevel": 1
        ///},
        /// {
        ///  "quizId": 11,
        ///  "title": "Geometry Quiz",
        ///  "grade": 5,
        ///  "difficultyLevel": 0
        /// }
        ///]
        /// </example>

        [HttpGet("ListOfQuizzes/{id}")]
        public async Task<ActionResult<IEnumerable<ListQuizDto>>> ListOfQuizzes(int id)
        {
            var response = await _lessonservice.ListOfQuizzes(id);

            if (response == null)
            {
                return NotFound($"No quizzes found for Lesson ID {id}.");
            }

            return Ok(response);
        }

        // GET: api/Lesson/ListLessonsByTeacher/{id}
        [HttpGet("ListLessonsByTeacher/{id}")]
        public async Task<IActionResult> ListLessonsByTeacher(int id)
        {
            var lessons = await _lessonservice.ListLessonsByTeacherId(id);

            if (lessons == null || !lessons.Any())
            {
                return NotFound(new { message = "No lessons found for this teacher." });
            }

            return Ok(lessons);
        }
    }
}
