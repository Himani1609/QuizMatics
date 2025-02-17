namespace QuizMatics.Models
{
    public class ServiceResponse
    {
        // A generic response wrapper that ensures consistency in API responses.

        public enum ServiceStatus { NotFound, Created, Updated, Deleted, Error, AlreadyExists, NotLinked }

        public ServiceStatus Status { get; set; }

        public int CreatedId { get; set; }

        public List<string> Messages { get; set; } = new List<string>();

    }
}

