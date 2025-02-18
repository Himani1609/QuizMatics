# QuizMatics

**QuizMatics** is an interactive mathematics resource platform designed for teachers and students. It provides tools for teacher, lesson, and quiz management.

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
- **Functionality**
  - Full Create, Read, Update, and Delete (CRUD) operations for Teachers, Lessons, and Quizzes.
  - Fetch a list of lessons by quiz ID.
  - Fetch a list of quizzes by lesson ID.
  - Fetch a list of lessons by teacher ID.
  - Link and unlink lessons and quizzes.

## Technologies Used

- **Backend**: ASP.NET Core, C#
- **Frontend**: Razor Pages / MVC *(To be executed in future)*
- **Database**: Microsoft SQL Server, Entity Framework
- **Authentication**: Identity Framework *(To be executed in future)*

## Design and Architecture

- **Service Layer & Interfaces**: 
  - Implemented a service layer to handle business logic and data operations, with interfaces for flexibility and easier testing.
  
- **Razor Views**: 
  - Used to render dynamic, data-driven pages, ensuring an interactive experience for users.

## API, Interface, and Service Layer

In **QuizMatics**, the architecture follows a layered design to separate concerns and improve maintainability:

- **API (Controller Layer)**:
  - The **API** layer (Controllers) handles incoming HTTP requests from the client. It processes these requests and returns appropriate responses.
  - The controllers act as a middleman between the user interface and the underlying business logic.

- **Interface**:
  - The **Interface** defines the contract for the services. It declares the methods that the service layer will implement, ensuring consistency and flexibility.
  - Interfaces allow for easier swapping of service implementations and enhance testability by enabling the use of mock services during unit testing.

- **Service Layer**:
  - The **Service** layer contains the actual business logic and data operations. Services implement the methods defined in the interfaces.
  - By using interfaces and dependency injection, the API layer can communicate with the service layer without knowing the details of the implementation. This promotes **Inversion of Control (IoC)**, where dependencies are injected rather than directly instantiated in the controllers.

### Flow of Execution:

1. **API Controller** receives a request.
2. **Controller** calls methods defined in the **Service** through the **Interface**.
3. **Service** executes the business logic and returns the result to the **Controller**.
4. **Controller** sends the response back to the client.

This architecture allows for a flexible, scalable, and testable application design.

## Page Controllers, View Models, and Views

In **QuizMatics**, I utilized **Page Controllers**, **View Models**, and **Views** to structure the application and separate concerns for a cleaner, more maintainable codebase:

- **Page Controllers**:
  - **Page Controllers** handle HTTP requests for specific pages. They interact with the **View Models** to prepare data and pass it to the **Views** for rendering.
  - These controllers focus on processing user requests, interacting with the service layer, and preparing data for display.

- **View Models**:
  - **View Models** hold data specific to a view. They provide a clear structure of data required by the view, ensuring that the controller doesn't directly pass data models to the view.
  - By using **View Models**, I decoupled the presentation logic from the domain models, making it easier to adjust the user interface without affecting the underlying business logic.
  - **View Models** are also used for validation purposes, ensuring that only the necessary data is passed to the view and the correct formatting is applied.

- **Views**:
  - **Views** in **QuizMatics** are responsible for rendering the UI and displaying the data passed from the **Page Controllers** through the **View Models**.
  - I used **Razor Views** to dynamically render HTML and embed C# code where necessary. The views are tightly connected to the **View Models** to ensure that the correct data is presented to the user.

### Flow of Execution:

1. **Page Controller** handles an HTTP request.
2. The **Page Controller** prepares and passes the data via a **View Model**.
3. The **View Model** holds the necessary data and structure for the view.
4. The **View** renders the data and presents it to the user.

This design ensures that each part of the application has a clear responsibility, improving maintainability, flexibility, and testability.

## Setup Instructions
Follow the instructions below to set up **QuizMatics** on your local machine or server.

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

## Contact

For any queries or suggestions, reach out to [Himani Bansal](https://github.com/Himani1609).

