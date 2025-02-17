using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using QuizMatics.Interfaces;
using QuizMatics.Models;
using QuizMatics.Models.ViewModels;
using QuizMatics.Services;

namespace QuizMatics.Controllers
{
    public class QuizPageController : Controller
    {
        private readonly IQuizService _quizService;
        private readonly ILessonService _lessonService;
        private readonly ITeacherService _teacherService;

        // Dependency injection of service interfaces
        public QuizPageController(IQuizService quizService, ILessonService lessonService, ITeacherService teacherService)
        {
            _quizService = quizService;
            _lessonService = lessonService;
            _teacherService = teacherService;
        }

        // Show List of Quizzes on Index page
        public IActionResult Index()
        {
            return RedirectToAction("List");
        }

        // GET: QuizPage/ListQuizzes
        [HttpGet("ListQuizzes")]
        public async Task<IActionResult> List()
        {
            IEnumerable<QuizDto> quizDtos = await _quizService.ListQuizzes();
            return View(quizDtos);
        }

        [HttpGet("QuizDetails/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            
            QuizDto? quizDto = await _quizService.FindQuiz(id);
            
            IEnumerable<ListLessonDto> lessons = await _quizService.ListOfLessons(id);

            if (quizDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = new List<string> { "Could not find quiz" } });
            }

            var quizDetails = new QuizDetails
            {
                Quiz = quizDto,
                Lessons = lessons.ToList() 
            };

            return View(quizDetails);
        }




        // GET: QuizPage/AddQuiz
        [HttpGet("AddQuiz")]
        public async Task<IActionResult> Add()
        {
            var lessons = await _lessonService.ListLessons();

            if (lessons == null || !lessons.Any())
            {
                return View("Error", new ErrorViewModel() { Errors = new List<string> { "No lessons found." } });
            }

            ViewBag.Lessons = lessons.Select(l => new SelectListItem
            {
                Value = l.LessonId.ToString(),
                Text = l.Title
            }).ToList();

            return View();
        }





        // POST: QuizPage/AddQuiz
        [HttpPost("AddQuiz")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(AddQuizDto addQuizDto)
        {
            if (ModelState.IsValid)
            {
                ServiceResponse response = await _quizService.AddQuiz(addQuizDto);
                if (response.Status == ServiceResponse.ServiceStatus.Created)
                {
                    return RedirectToAction("List");
                }
                else
                {
                    return View("Error", new ErrorViewModel() { Errors = response.Messages });
                }
            }

            return View(addQuizDto);
        }




        // GET: QuizPage/EditQuiz/{id}
        [HttpGet("EditQuiz/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            QuizDto? quizDto = await _quizService.FindQuiz(id);

            if (quizDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Quiz not found"] });
            }

            var updateQuizDto = new UpdateQuizDto
            {
                QuizId = quizDto.QuizId,
                Title = quizDto.Title,
                Description = quizDto.Description,
                MaxMinsAlotted = quizDto.MaxMinsAlotted,
                Grade = quizDto.Grade,
                DifficultyLevel = quizDto.DifficultyLevel
            };

            var lessons = await _lessonService.ListLessons();
            ViewBag.Lessons = lessons;

            return View(updateQuizDto);
        }





        // POST: QuizPage/EditQuiz/{id}
        [HttpPost("EditQuiz/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateQuizDto updateQuizDto)
        {
            if (id != updateQuizDto.QuizId)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Quiz ID mismatch"] });
            }

            if (ModelState.IsValid)
            {
                var serviceResponse = await _quizService.UpdateQuiz(id, updateQuizDto);

                if (serviceResponse.Status == ServiceResponse.ServiceStatus.Error)
                {
                    return View("Error", new ErrorViewModel() { Errors = serviceResponse.Messages });
                }

                return RedirectToAction("Details", new { id });
            }

            return View(updateQuizDto);
        }





        // GET: QuizPage/DeleteQuiz/{id}
        [HttpGet("DeleteQuiz/{id}")]
        public async Task<IActionResult> ConfirmDelete(int id)
        {
            QuizDto? quizDto = await _quizService.FindQuiz(id);

            if (quizDto == null)
            {
                return View("Error", new ErrorViewModel() { Errors = ["Quiz not found"] });
            }

            return View(quizDto);
        }

        // POST: QuizPage/DeleteQuiz/{id}
        [HttpPost("DeleteQuiz/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            ServiceResponse response = await _quizService.DeleteQuiz(id);

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
