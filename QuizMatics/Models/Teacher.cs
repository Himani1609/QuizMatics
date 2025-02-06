using System.ComponentModel.DataAnnotations;

namespace QuizMatics.Models
{
    public class Teacher
    {
        [Key]
        public int TeacherId { get; set; }


        [Required]
        [MaxLength(100)]
        public string Name { get; set; }


        [Required]
        [EmailAddress]
        public string Email { get; set; }


        // one teacher can have many lessons
        public ICollection<Lesson>? Lessons { get; set; }
    }

    public class TeacherDto
    {
        [Key]
        public int TeacherId { get; set; }


        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public int TotalLessons { get; set; }

        public int TotalQuizzes { get; set; }
    }



    public class UpdateTeacherDto
    {
        [Key]
        public int TeacherId { get; set; }


        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }


    public class AddTeacherDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
