using System.ComponentModel.DataAnnotations;

namespace QuizMatics.Models
{
    public enum Difficulty
    {
        Easy = 0,
        Medium = 1,
        Hard = 2
    }
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }


        public DateOnly DateCreated { get; set; }

        public int MaxMinsAlotted { get; set; }

        public int Grade { get; set; }

        [Required]
        public Difficulty DifficultyLevel { get; set; }

        //one quiz can be in many lessons
        public ICollection<Lesson>? Lessons { get; set; }
    }


    public class QuizDto
    {
        [Key]
        public int QuizId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

        public int MaxMinsAlotted { get; set; }

        public int Grade { get; set; }

        [Required]
        public Difficulty DifficultyLevel { get; set; }

        public int TotalLessons { get; set; }

        public List<string> LessonNames { get; set; }


    }


    public class UpdateQuizDto
    {

        [Key]
        public int QuizId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

        public DateOnly DateCreated { get; set; }


        public int MaxMinsAlotted { get; set; }

        public int Grade { get; set; }

        [Required]
        public Difficulty DifficultyLevel { get; set; }

    }


    public class AddQuizDto
    {

        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

        public DateOnly DateCreated { get; set; }


        public int MaxMinsAlotted { get; set; }

        public int Grade { get; set; }

        [Required]
        public Difficulty DifficultyLevel { get; set; }

        [Required] 
        public int LessonId { get; set; }

    }

}
