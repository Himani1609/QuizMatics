using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuizMatics.Interfaces;
using QuizMatics.Models;
using QuizMatics.Models.ViewModels;
using QuizMatics.Services;

namespace QuizMatics.Controllers
{
    public class TeacherPageController : Controller
    {
        private readonly ITeacherService _teacherService;
        private readonly ILessonService _lessonService;
        private readonly IQuizService _quizService;

        // dependency injection of service interface
        public TeacherPageController(ITeacherService TeacherService, ILessonService LessonService, IQuizService QuizService)
        {
            _teacherService = TeacherService;
            _lessonService = LessonService;
            _quizService = QuizService;
        }


        // Show List of Teachers on Index page 
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }


        // GET: TeacherPage/ListTeachers
        [HttpGet("ListTeachers")]
        public async Task<IActionResult> List()
        {
            IEnumerable<TeacherDto?> TeacherDtos = await _teacherService.ListTeachers();
            return View(TeacherDtos);
        }

        [HttpGet("TeacherDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            TeacherDto? TeacherDto = await _teacherService.FindTeacher(id);

            if (TeacherDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Could not find teacher"] });
            }
            else
            {
                var lessons = await _lessonService.ListLessonsByTeacherId(id); 

                
                TeacherDetails TeacherInfo = new TeacherDetails()
                {
                    Teacher = TeacherDto,
                    Lessons = lessons.ToList() 
                };

                return View(TeacherInfo);
            }
        }


        // GET: TeacherPage/AddTeacher
        [HttpGet("AddTeacher")]
        public IActionResult Add()
        {
            return View();
        }

        // POST: TeacherPage/AddTeacher
        [HttpPost("AddTeacher")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddTeacherDto teacherDto)
        {
            if (ModelState.IsValid)
            {
                await _teacherService.AddTeacher(teacherDto);
                return RedirectToAction("List");
            }

            return View(teacherDto);
        }

        // GET: TeacherPage/EditTeacher/{id}
        [HttpGet("EditTeacher/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            TeacherDto? teacherDto = await _teacherService.FindTeacher(id);

            if (teacherDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Teacher not found"] });
            }

            var updateTeacherDto = new UpdateTeacherDto
            {
                TeacherId = teacherDto.TeacherId,
                Name = teacherDto.Name
            };

            return View(updateTeacherDto);
        }


        // POST: TeacherPage/EditTeacher/{id}
        [HttpPost("EditTeacher/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdateTeacherDto updateteacherDto)
        {
            if (id != updateteacherDto.TeacherId)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Invalid teacher ID"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _teacherService.UpdateTeacher(id, updateteacherDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updateteacherDto);
        }

        // GET: TeacherPage/DeleteTeacher/{id}
        [HttpGet("DeleteTeacher/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            TeacherDto? teacherDto = await _teacherService.FindTeacher(id);

            if (teacherDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Teacher not found"] });
            }

            return View(teacherDto);
        }


        // POST: TeacherPage/DeleteTeacher/{id}
        [HttpPost("DeleteTeacher/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _teacherService.DeleteTeacher(id);

            if (response.Status == ServiceResponse.ServiceStatus.Deleted)
            {
                return RedirectToAction("List", "TeacherPage");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }
    }

}
