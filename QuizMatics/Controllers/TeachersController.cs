using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuizMatics.Data;
using QuizMatics.Models;
using QuizMatics.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace QuizMatics.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TeachersController : ControllerBase
    {
        private readonly ITeacherService _teacherservice;

        public TeachersController(ITeacherService context)
        {
            _teacherservice = context;
        }

        // Controllers handle HTTP requests and call the service layer to process business logic.

        /// <summary>
        /// Returns a list of Teachers including the Number of Teachers they created.
        /// </summary>
        /// <param name="TeacherDto">It includes ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// List of Teachers including ID, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// </returns>
        /// <example>
        /// GET: api/Teachers/List -> [{TeacherId: 1, Name: "Apurva", TotalTeachers: 2, TotalQuizzes:2},{....},{....}]
        /// </example>
        [HttpGet(template: "List")]
        public async Task<ActionResult<IEnumerable<TeacherDto>>> ListTeachers()
        {
            // controller is using ITeacherService to call for the List of Teachers
            // and the ITeacherService is calling TeacherService to implement this
            IEnumerable<TeacherDto> TeacherDtos = await _teacherservice.ListTeachers();

            // return 200 OK with TeacherDtos
            return Ok(TeacherDtos);
        }


        /// <summary>
        /// Return a Teacher specified by it's {id}
        /// </summary>
        /// /// <param name="id">Teacher's id</param>
        /// <param name="TeacherDto">It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 Ok
        /// TeacherDto : It includes Teacher's id, Name, Total no. of Teachers they created, Total no. of Quizzes they created
        /// or
        /// 404 Not Found: "Teacher with {id} doesn't exist" when there is no Teacher for that {id}
        /// </returns>
        /// <example>
        /// GET: api/Teachers/Find/{id} -> {TeacherId: 1, Name: "Apurva", TotalTeachers: 2, TotalQuizzes:2}
        [HttpGet(template: "Find/{id}")]
        public async Task<ActionResult<TeacherDto>> FindTeacher(int id)
        {
            var teacher = await _teacherservice.FindTeacher(id);

            if (teacher == null)
            {
                return NotFound($"Teacher with {id} doesn't exist");
            }
            else
            {
                return Ok(teacher);
            }

        }

        /// <summary>
        /// It updates a Teacher
        /// </summary>
        /// <param name="id">The ID of Teacher which we want to update</param>
        /// <param name="UpdateTeacherDto">The required information to update the Teacher</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 400 Bad Request:"Teacher ID mismatch.": If the ID in the route does not match the Teacher ID.
        /// or
        /// 404 Not Found: "Teacher not found.": If the Teacher does not exist.
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while updating the teacher.": If there is an error updating the Teacher.
        /// or
        /// 200 OK:"Teacher with ID {id} updated successfully.": If the update is successful, returns a success message "Teacher Updated Successfully"
        /// </returns>       
        [HttpPut(template: "Update/{id}")]
        [Authorize]
        public async Task<ActionResult> UpdateTeacher(int id, UpdateTeacherDto updateteacherDto)
        {

            if (id != updateteacherDto.TeacherId)
            {
                return BadRequest(new { message = "Teacher ID mismatch." });
            }

            ServiceResponse response = await _teacherservice.UpdateTeacher(id, updateteacherDto);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Teacher not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while updating the teacher." });
            }

            return Ok(new { message = $"Teacher with ID {id} updated successfully." });
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
        /// HTTP RESPONSE
        /// 201 Created: "Teacher added successfully with ID {response.CreatedId}": If the teacher is successfully added.
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while updating the teacher.": If there is an error updating the teacher.
        /// </returns>
        /// <example>
        /// api/Teachers/Add -> Add the Teacher 
        /// </example>
        [HttpPost(template: "Add")]
        public async Task<ActionResult> AddTeacher(AddTeacherDto addteacherDto)
        {

            ServiceResponse response = await _teacherservice.AddTeacher(addteacherDto);

            if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while adding the teacher." });
            }

            return CreatedAtAction("FindTeacher", new { id = response.CreatedId }, new
            {
                message = $"Teacher added successfully with ID {response.CreatedId}",
                teacherId = response.CreatedId
            });
        }

        /// <summary>
        /// Delete a Quiz specified by it's {id}
        /// </summary>
        /// <param name="id">The id of the Quiz we want to delete</param>
        /// <returns>
        /// HTTP RESPONSE
        /// 200 OK: "Teacher with ID {id} deleted successfully.": If deletion is successful, returns a success message.
        /// or
        /// 404 Not Found: "Teacher not found.": If the teacher does not exist.
        /// or
        /// 500 Internal Server Error: "An unexpected error occurred while deleting the teacher.": If there is an error deleting the teacher.
        /// </returns>
        /// <example>
        /// api/Quizzes/Delete/{id} -> Deletes the quiz associated with {id}
        /// </example>
        [HttpDelete(template: "Delete/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteTeacher(int id)
        {
            ServiceResponse response = await _teacherservice.DeleteTeacher(id);

            if (response.Status == ServiceResponse.ServiceStatus.NotFound)
            {
                return NotFound(new { error = "Teacher not found." });
            }
            else if (response.Status == ServiceResponse.ServiceStatus.Error)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while deleting the teacher." });
            }

            return Ok(new { message = $"Teacher with ID {id} deleted successfully." });
        }


    }
}
