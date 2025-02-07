# QuizMatics

**QuizMatics** is an interactive mathematics resource platform designed for teachers and students. It provides tools for teacher, lesson and quiz management. 

## Features

- **User Authentication** *(To be executed in future)*
  - Secure login and role-based access for teachers and admins.
- **Teacher Dashboard**
  - Create, update, and delete teachers.
  - Manage lessons and quizzes.
- **Lesson Management**
  - Create, update, and delete lessons.
  - Associate lessons with quizzes.
- **Quiz System**
  - Create, update, and delete quizzes.
  - Associate quizzes with lessons.
- **Entity Relationships**
  - One-to-Many: Teachers → Lessons.
  - Many-to-Many: Lessons ↔ Quizzes (via LessonQuiz table).
- **Database Integration**
  - Built with ASP.NET Core, C#, and Entity Framework.
- **CRUD Functionality**
  - Full Create, Read, Update, and Delete (CRUD) operations for Teachers, Lessons, and Quizzes.
  - Fetch a list of lessons by quiz ID.
  - Fetch a list of quizzes by lesson ID.
  - Link and unlink lessons and quizzes.

## Technologies Used

- **Backend**: ASP.NET Core, C#
- **Frontend**: Razor Pages / MVC *(To be executed in future)*
- **Database**: Microsoft SQL Server, Entity Framework
- **Authentication**: Identity Framework *(To be executed in future)*

## Setup Instructions

1. Clone the repository:
   ```sh
   git clone https://github.com/Himani1609/QuizMatics.git
   cd QuizMatics
   ```
2. Install dependencies:
   ```sh
   dotnet restore
   ```
3. Configure the database:
   - Update `appsettings.json` with your database connection string.
   - Run migrations:
     ```sh
     dotnet ef database update
     ```
4. Run the application:
   ```sh
   dotnet run
   ```

## Future Enhancements

- Implement authentication for teachers and admins.
- Make the UI for a more interactive experience.
- Add detailed analytics for student performance.
- Advanced Quiz Customization by adding questions in a quiz and then having CRUD functionality on quiz questions.
- Search Functionality in Lesson by Topic.
- Search Functionality in Quiz by Title.
- Sort & Filter Quiz by Difficulty or Grade level.


## Contact

For any queries or suggestions, reach out to [Himani Bansal](https://github.com/Himani1609).

