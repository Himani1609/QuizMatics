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
using QuizMatics.Interfaces;
using QuizMatics.Services;

namespace QuizMatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizservice;

        public QuizzesController(IQuizService context)
        {
            _quizservice = context;
        }

        /// <summary>
        /// Returns a list of Quizzes, each represented by a QuizDto along with it's Lessons count.
        /// </summary>
        /// <param name="QuizDto">It includes Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// List of quizzes with it's Id, Title, Description, Difficulty level, Max Minutes alloted for the quiz, Grade level, Total Lessons in which this quiz is a part of.
        /// </returns>
        /// <example>
        /// GET: api/Quizzes/List -> [{QuizId:1, Title:"Algebra Basics Quiz", Difficulty Level:0, Grade:9, MaxMinsAlotted:60, TotalLessons:2 },{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<QuizDto>>> ListQuizzes()
        {

            IEnumerable<QuizDto> QuizDtos = await _quizservice.ListQuizzes();

            // return 200 OK with TeacherDtos
            return Ok(QuizDtos);
        }

        /// <summary>
        /// Returns a Quiz specified by its {id}, represented by an QuizDto along with it's lesson count.
        /// </summary>
        /// <param name="id">Quiz id</param>
        /// <param name="QuizDto">It includes Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// QuizDto : Quiz with it's given Id, Title, Description, Max Minutes Alotted for the quiz, Grade level, Difficulty level and Number of lessons it is a part of.
        /// or
        /// 404 Not Found: $"Quiz with ID {id} not found.": when there is no Quiz of that id
        /// </returns>
        /// <example>
        /// GET: api/Quizzes/Find/{id} -> {QuizDto} represented as {QuizId:1, Title:"Algebra Basics Quiz", Difficulty Level:0, Grade:9, MaxMinsAlotted:60, TotalLessons:2 }
        /// </example>
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<QuizDto>> FindQuiz(int id)
        {
            var quiz = await _quizservice.FindQuiz(id);

            if (quiz == null)
            {
                return NotFound($"Quiz with ID {id} not found.");
            }
            else
            {
                return Ok(quiz);
            }
        }

        /// <summary>
        /// It updates a Quiz
        /// </summary>
        /// <param name="id">The ID of Quiz which we want to update</param>
        /// <param name="UpdateQuizDto">The required information to update the Quiz</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 400 Bad Request:"Quiz ID mismatch.": If the ID in the route does not match the Quiz ID.
        /// or
        /// 404 Not Found: "Quiz not found.": If the Quiz does not exist.
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while updating the quiz.": If there is an error updating the quiz.
        /// or
        /// 200 OK: $"Quiz with ID {id} updated successfully.": If the update is successful, returns a success message.
        /// </returns>       
        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateQuiz(int id, UpdateQuizDto updatequizDto)
        {
            if (id != updatequizDto.QuizId)
            {
                return BadRequest(new { message = "Quiz ID mismatch." });
            }

            ServiceResponse response = await _quizservice.UpdateQuiz(id, updatequizDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Quiz not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the quiz." });
            }

            return Ok(new { message = $"Quiz with ID {id} updated successfully." });
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
        /// HTTP RESPONSE
        /// 201 Created:"Quiz added successfully with ID {response.CreatedId}": If the quiz is successfully added.
        /// or
        /// 404 Not Found: "Lesson not found. Cannot add quiz.": If the lesson for LessonId added in request body is not found
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while adding the quiz.": If there is an error adding teh quiz.
        /// </returns>
        /// <example>
        /// api/Quizzes/Add -> Add the Quiz in the Quizzes table
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<IActionResult> AddQuiz(AddQuizDto addquizDto)
        {
            ServiceResponse response = await _quizservice.AddQuiz(addquizDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Lesson not found. Cannot add quiz." });
            }

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the quiz." });
            }

            return CreatedAtAction("FindTeacher", new { id = response.CreatedId }, new
            {
                message = $"Quiz added successfully with ID {response.CreatedId}",
                quizId = response.CreatedId
            });
        }


        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 OK: "Quiz with ID {id} deleted successfully.": If deletion is successful, returns a success message.
        /// or
        /// 404 Not Found:  "Quiz not found.": When quiz of that {id} is not found.
        /// or
        /// 505 Internal server Error: "An unexpected error occurred while deleting the quiz.": if there is an error deleting the quiz.
        /// </returns>
        /// <example>
        /// api/Quizzes/Delete/{id} -> Deletes the quiz associated with {id}
        /// </example>
        [HttpDelete(template: "Delete/{id}")]
        public async Task<IActionResult> DeleteQuiz(int id)
        {
            ServiceResponse response = await _quizservice.DeleteQuiz(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Quiz not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the quiz." });
            }

            return Ok(new { message = $"Quiz with ID {id} deleted successfully." });
        }

        /// <summary>
        /// Retrieves a list of lessons associated with a specific quiz identified by its ID.
        /// Each lesson is mapped to a <see cref="ListLessonDto"/> containing lesson details
        /// and the name of the teacher associated with the lesson.
        /// </summary>
        /// <param name="id">The ID of the quiz to retrieve lessons for.</param>
        /// <returns>
        /// HTTP RESPONSE
        /// ListLessonDto: A list of lessons containing lesson details and teacher names.
        /// or
        /// 404 Not Found: "No lessons found for Quiz ID {id}.": If the quiz does not exist for that {id} or if no lessons are found for the quiz.
        /// </returns>
        /// <example>
        /// GET /ListOfLessons/1 ->
        /// Returns:
        /// [{
        ///  "lessonId": 1,
        /// "title": "Algebra Fundamentals",
        /// "description": "Introduction to Algebra basics",
        /// "dateCreated": "2025-01-01",
        /// "name": "Apurva"
        ///  },
        ///{
        ///  "lessonId": 2,
        ///  "title": "Linear Equations",
        ///  "description": "Solving Linear Equations",
        ///  "dateCreated": "2025-02-02",
        ///  "name": "Apurva"
        ///}
        /// ]
        /// </example>


        [HttpGet("ListOfLessons/{id}")]
        public async Task<IActionResult> ListOfLessons(int id)
        {
            var response = await _quizservice.ListOfLessons(id);

            if (response == null)
            {
                return NotFound($"No lessons found for Quiz ID {id}.");
            }

            return Ok(response);
        }



        /// <summary>
        /// Links an existing Quiz to a Lesson.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// 200 Ok:"Quiz with {quizId} linked to Lesson with {lessonId} successfully.": if the Quiz is linked to the Lesson.
        /// or
        /// 505 Interval Server Error: "An unexpected error occurred while linking the quiz to a lesson.": If there is an error linking quiz to a lesson
        /// or
        /// 404 Not Found: "Lesson or Quiz not found.":  if the Lesson or Quiz is not found.
        /// or
        /// 409 Conflict: "Quiz is already linked to this lesson." : If quiz is already linked with the lesson
        /// </returns>
        /// <example>
        /// api/Quizzes/LinkQuiz?linkId=1^&quizId=2 -> Quiz 2 linked to Lesson 1.
        /// </example>
        [HttpPost("LinkQuiz")]
        public async Task<IActionResult> LinkQuizToLesson(int lessonId, int quizId)
        {
            var response = await _quizservice.LinkQuizToLesson(lessonId, quizId);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Lesson or Quiz not found." });
            }
            if (response.Status == ServiceResponse.ServiceStatus.AlreadyExists)
            {
                return Conflict(new { conflict = "Quiz is already linked to this lesson." });
            }
            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while linking the quiz to a lesson." });
            }

            return Ok(new { message = $"Quiz with {quizId} linked to Lesson with {lessonId} successfully." });
        }



        /// <summary>
        /// Unlinks an existing Quiz from a Lesson without deleting the Quiz and by removing the association in the junction table.
        /// </summary>
        /// <param name="lessonId">The ID of the Lesson.</param>
        /// <param name="quizId">The ID of the Quiz.</param>
        /// <returns>
        /// 200 Ok:"Quiz with {quizId} unlinked to Lesson with {lessonId} successfully.": if the Quiz is unlinked to the Lesson.
        /// or
        /// 505 Interval Server Error: "An unexpected error occurred while unlinking the quiz to a lesson.": If there is an error unlinking quiz to a lesson
        /// or
        /// 404 Not Found: "Lesson or Quiz not found.":  if the Lesson or Quiz is not found.
        /// or
        /// 409 Conflict: "Quiz is not linked to this lesson." : If quiz is not linked with the lesson
        /// </returns>
        /// <example>
        /// api/Quizzes/UnlinkQuiz?linkId=1^&quizId=2 -> Quiz 2 unlinked from Lesson 1.
        /// </example>
        [HttpPost("UnlinkQuiz")]
        public async Task<IActionResult> UnlinkQuizFromLesson(int lessonId, int quizId)
        {
            var response = await _quizservice.UnlinkQuizFromLesson(lessonId, quizId);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Lesson or Quiz not found." });
            }
            if (response.Status == ServiceResponse.ServiceStatus.NotLinked)
            {
                return Conflict(new { conflict = "Quiz is not linked to this lesson." });
            }
            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while unlinking the quiz to a lesson." });
            }

            return Ok(new { message = $"Quiz with {quizId} unlinked to Lesson with {lessonId} successfully." });

        }
    }
}
