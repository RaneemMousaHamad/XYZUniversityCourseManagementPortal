# README for XYZUniversityCourseManagementPortal

# XYZ University Course Management Portal

## 📚 Overview

**XYZ University Course Management Portal** is a comprehensive web application built with **ASP.NET Core MVC** that enables students, instructors, and administrators to manage university courses, enrollments, and academic records efficiently.

This project implements a full-stack solution with a **RESTful WebAPI** backend and a responsive web portal frontend.

---

## 🎯 Features

### For Students
- ✅ Browse available courses
- ✅ Enroll in courses
- ✅ View enrollment status
- ✅ Access student dashboard
- ✅ Track course progress

### For Instructors
- ✅ Manage course details
- ✅ View enrolled students
- ✅ Track enrollment statistics
- ✅ Update course information

### For Administrators
- ✅ Manage all courses and students
- ✅ Oversee instructor accounts
- ✅ Generate reports
- ✅ System configuration

---

## 🏗️ Project Structure

```
XYZUniversityCourseManagementPortal/
├── WebAPI/                          # RESTful WebAPI endpoints
│   ├── Controllers/                 # API controllers for CRUD operations
│   ├── Models/                      # Data models (Course, Student, Instructor)
│   ├── Services/                    # Business logic layer
│   ├── Data/                        # Database context and repositories
│   └── Migrations/                  # Entity Framework migrations
│
├── XYZUniversityCourseManagementPortal/  # MVC Web Portal
│   ├── Controllers/                 # MVC controllers
│   ├── Views/                       # Razor views (HTML templates)
│   ├── Models/                      # View models
│   ├── Services/                    # Business logic
│   ├── wwwroot/
│   │   ├── css/                     # Responsive styling
│   │   ├── js/                      # Client-side scripts
│   │   └── lib/                     # jQuery, Bootstrap, validation libraries
│   └── appsettings.json             # Configuration settings
│
├── README.md
├── LICENSE
└── .gitignore
```

---

## 🛠️ Technology Stack

| Component | Technology |
|-----------|-----------|
| **Backend** | ASP.NET Core MVC |
| **Language** | C# (61.8%) |
| **Frontend** | HTML (33.7%), CSS (2.9%), JavaScript (0.1%) |
| **Database** | SQL Server (TSQL) |
| **UI Framework** | Bootstrap 5 |
| **Validation** | jQuery Validation Unobtrusive |
| **ORM** | Entity Framework Core |

---

## 📋 Language Composition

- **C#**: 61.8% (Backend logic, APIs, business services)
- **HTML**: 33.7% (Razor views, user interface templates)
- **CSS**: 2.9% (Responsive styling and design)
- **TSQL**: 1.5% (Database schema and queries)
- **JavaScript**: 0.1% (Client-side interactivity)

---

## 🚀 Getting Started

### Prerequisites
- .NET 6.0 or higher
- SQL Server 2019 or higher
- Visual Studio 2022 or VS Code
- Git

### Installation

1. **Clone the repository:**
```bash
git clone https://github.com/RaneemMousaHamad/XYZUniversityCourseManagementPortal.git
cd XYZUniversityCourseManagementPortal
```

2. **Install dependencies:**
```bash
dotnet restore
```

3. **Configure database connection:**
   - Update `appsettings.json` with your SQL Server connection string:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=XYZUniversityCourseDB;Trusted_Connection=true;"
  }
}
```

4. **Apply database migrations:**
```bash
dotnet ef database update
```

5. **Build the project:**
```bash
dotnet build
```

6. **Run the application:**
```bash
dotnet run
```

7. **Access the portal:**
   - Open your browser and navigate to: `https://localhost:5001`

---

## 📡 API Endpoints

The WebAPI provides RESTful endpoints for course management:

### Courses
```
GET    /api/courses              - Get all courses
GET    /api/courses/{id}         - Get course by ID
POST   /api/courses              - Create new course
PUT    /api/courses/{id}         - Update course
DELETE /api/courses/{id}         - Delete course
```

### Students
```
GET    /api/students             - Get all students
GET    /api/students/{id}        - Get student by ID
POST   /api/students             - Create new student
PUT    /api/students/{id}        - Update student
DELETE /api/students/{id}        - Delete student
```

### Enrollments
```
GET    /api/enrollments          - Get all enrollments
POST   /api/enrollments          - Enroll student in course
DELETE /api/enrollments/{id}     - Remove enrollment
```

---

## 💾 Database Schema

### Main Tables

**Courses**
- CourseID (PK)
- CourseCode
- CourseName
- Description
- InstructorID (FK)
- Credits
- Capacity

**Students**
- StudentID (PK)
- FirstName
- LastName
- Email
- EnrollmentDate

**Instructors**
- InstructorID (PK)
- FirstName
- LastName
- Email
- Department

**Enrollments**
- EnrollmentID (PK)
- StudentID (FK)
- CourseID (FK)
- EnrollmentDate
- Grade

---

## 🔐 Security Features

- ✅ User authentication and authorization
- ✅ Input validation on client and server
- ✅ CSRF protection
- ✅ SQL injection prevention (Entity Framework)
- ✅ Secure password handling
- ✅ Role-based access control (RBAC)

---

## 📊 Project Commits

The project follows **Conventional Commits** format for clear version history:

1. `chore:` initialize ASP.NET Core MVC project structure
2. `feat:` add domain models for courses, students, instructors
3. `feat:` add MVC controllers for HTTP request handling
4. `feat:` add Razor views for user interface
5. `feat:` add service layer for business logic
6. `feat:` implement RESTful WebAPI endpoints
7. `chore:` add TSQL database schema
8. `style:` add responsive CSS styling
9. `chore:` configure ASP.NET Core dependencies

---

## 🧪 Testing

```bash
# Run unit tests
dotnet test

# Run integration tests
dotnet test --filter "Category=Integration"
```

---

## 📝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/your-feature`
3. Commit changes: `git commit -m "feat: add your feature"`
4. Push to branch: `git push origin feature/your-feature`
5. Submit a Pull Request

---

## 📄 License

This project is licensed under the **MIT License** - see the [LICENSE](LICENSE) file for details.

---

## 👨‍💻 Author

**Raneem Mousa Hamad**  
GitHub: [@RaneemMousaHamad](https://github.com/RaneemMousaHamad)

---

## 📞 Support & Contact

For questions or issues, please:
- Open an [Issue](https://github.com/RaneemMousaHamad/XYZUniversityCourseManagementPortal/issues)
- Email: raneemhamad71@gmail.com

---

## 🎓 Learning Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/en-us/aspnet/core/)
- [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/)
- [RESTful API Design](https://restfulapi.net/)
- [Bootstrap Documentation](https://getbootstrap.com/docs/5.0/)


