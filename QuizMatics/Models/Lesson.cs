using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace QuizMatics.Models
{
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }


        public DateOnly DateCreated { get; set; }


        // one lesson is made by one teacher
        [ForeignKey("Teachers")]
        public int TeacherId { get; set; }
        public virtual Teacher Teacher { get; set; }


        //one lesson can have many quizzes
        public ICollection<Quiz>? Quizzes { get; set; } 
        
    }

    public class LessonDto
    {
        [Key]
        public int LessonId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateOnly DateCreated { get; set; }

        public string? Name { get; set; }

        public int? TotalQuizzes { get; set; }

        public List<string>? QuizNames { get; set; }

    }


    public class UpdateLessonDto
    {
        [Key]
        public int LessonId { get; set; }


        [Required]
        [StringLength(100)]
        public string Title { get; set; }


        [StringLength(500)]
        public string Description { get; set; }

        public int TeacherId { get; set; }
    }


    public class AddLessonDto
    {
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        public DateOnly DateCreated { get; set; }

        [Required]
        public int TeacherId { get; set; }  
    }

}
