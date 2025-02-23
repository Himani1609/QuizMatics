using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuizMatics.Interfaces;
using QuizMatics.Models;
using QuizMatics.Models.ViewModels;
using QuizMatics.Services;

namespace QuizMatics.Controllers
{
    public class LessonPageController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly ITeacherService _teacherService;
        private readonly IQuizService _quizService;


        // dependency injection of service interfaces
        public LessonPageController(ILessonService lessonService, ITeacherService teacherService, IQuizService quizService)
        {
            _lessonService = lessonService;
            _teacherService = teacherService;
            _quizService = quizService;
        }

        // Show List of Lessons on Index page 
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: LessonPage/ListLessons
        [HttpGet("ListLessons")]
        public async Task<IActionResult> List()
        {
            IEnumerable<LessonDto> lessonDtos = await _lessonService.ListLessons();
            return View(lessonDtos);
        }



        // GET: LessonPage/LessonDetails/{id}
        [HttpGet("LessonDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
           
            LessonDto? lessonDto = await _lessonService.FindLesson(id);
            if (lessonDto == null)
            {
                return View("Error", new ErrorViewModel { Errors = ["Could not find lesson"] });
            }

   
            IEnumerable<ListQuizDto> linkedQuizzes = await _lessonService.ListOfQuizzes(id);

            IEnumerable<ListQuizDto> allQuizzes = (await _quizService.ListQuizzes())
                .Select(q => new ListQuizDto
                {
                    QuizId = q.QuizId,
                    Title = q.Title
                });

            
            IEnumerable<ListQuizDto> availableQuizzes = allQuizzes
                .Where(q => !linkedQuizzes.Any(lq => lq.QuizId == q.QuizId));

           
            var lessonDetails = new LessonDetails
            {
                Lesson = lessonDto,
                ListQuizzes = linkedQuizzes,
                AvailableQuizzes = availableQuizzes
            };

            
            ViewBag.QuizDropdown = new SelectList(availableQuizzes, "QuizId", "Title");

            return View(lessonDetails);
        }



        // POST: LessonPage/LinkQuiz
        [HttpPost("LinkQuiz")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> LinkQuiz(int lessonId, int quizId)
        {
            var response = await _quizService.LinkQuizToLesson(lessonId, quizId);

            if (response.Status == ServiceResponse.ServiceStatus.Updated)
            {
                return RedirectToAction("Details", new { id = lessonId });
            }

            return View("Error", new ErrorViewModel { Errors = response.Messages });
        }

        // POST: LessonPage/UnlinkQuiz
        [HttpPost("UnlinkQuiz")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnlinkQuiz(int lessonId, int quizId)
        {
            var response = await _quizService.UnlinkQuizFromLesson(lessonId, quizId);

            if (response.Status == ServiceResponse.ServiceStatus.Updated)
            {
                return RedirectToAction("Details", new { id = lessonId });
            }

            return View("Error", new ErrorViewModel { Errors = response.Messages });
        }



        // GET: LessonPage/AddLesson
        [HttpGet("AddLesson")]
        public async Task<IActionResult> Add()
        {
            var teachers = await _teacherService.ListTeachers(); 

            
            if (teachers == null || !teachers.Any())
            {
                
                return View("Error", new ErrorViewModel() { Errors = new List<string> { "No teachers found." } });
            }

            ViewBag.Teachers = teachers.Select(t => new SelectListItem
            {
                Value = t.TeacherId.ToString(),
                Text = t.Name
            }).ToList();

            return View();
        }



        // POST: LessonPage/AddLesson
        [HttpPost("AddLesson")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddLessonDto addLessonDto)
        {
            if (ModelState.IsValid)
            {
                if (addLessonDto.DateCreated == default)
                {
                    addLessonDto.DateCreated = DateOnly.FromDateTime(DateTime.Now);
                }

                ServiceResponse response = await _lessonService.AddLesson(addLessonDto);
                if (response.Status == ServiceResponse.ServiceStatus.Created)
                {
                    return RedirectToAction("List");
                }
                else
                {
                    return View("Error", new ErrorViewModel() { Errors = response.Messages });
                }
            }

            return View(addLessonDto);
        }

        // GET: LessonPage/EditLesson/{id}
        [HttpGet("EditLesson/{id}")]
        [Authorize]
        public async Task<IActionResult> Edit(int id)
        {
            LessonDto? lessonDto = await _lessonService.FindLesson(id);

            if (lessonDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Lesson not found"] });
            }


            var updateLessonDto = new UpdateLessonDto
            {
                LessonId = lessonDto.LessonId,
                Title = lessonDto.Title,
                Description = lessonDto.Description,
                DateCreated = lessonDto.DateCreated
            };

            var teachers = await _teacherService.ListTeachers();
            ViewBag.Teachers = teachers;  

            return View(updateLessonDto);
        }

        // POST: LessonPage/EditLesson/{id}
        [HttpPost("EditLesson/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, UpdateLessonDto updateLessonDto)
        {
            if (id != updateLessonDto.LessonId)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Lesson ID mismatch"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _lessonService.UpdateLesson(id, updateLessonDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updateLessonDto);
        }

        // GET: LessonPage/DeleteLesson/{id}
        [HttpGet("DeleteLesson/{id}")]
        [Authorize]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            LessonDto? lessonDto = await _lessonService.FindLesson(id);

            if (lessonDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Lesson not found"] });
            }

            return View(lessonDto);
        }

        // POST: LessonPage/DeleteLesson/{id}
        [HttpPost("DeleteLesson/{id}")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _lessonService.DeleteLesson(id);

            if (response.Status == ServiceResponse.ServiceStatus.Deleted)
            {
                return RedirectToAction("List");
            }
            else
            {
                return View("Error", new ErrorViewModel() { Errors = response.Messages });
            }
        }

    }
}
