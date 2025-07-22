# Polaris Learning System - Backend API

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square&logo=dotnet)]()
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-Core-512BD4?style=flat-square)]()
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Express-CC2927?style=flat-square&logo=microsoft-sql-server)]()

The Polaris Learning System backend is a comprehensive RESTful API built with ASP.NET Core 8.0, designed to support cryptography education and assessment management. This system provides secure authentication, role-based access control, and comprehensive educational tools for students, instructors, and administrators.

## 🏗️ Architecture Overview

The backend follows a clean architecture pattern with the following layers:

- **Controllers**: API endpoints and request handling
- **Services**: Business logic and application services
- **Repositories**: Data access layer with Entity Framework Core
- **Models**: Domain entities and data models
- **DTOs**: Data Transfer Objects for API communication
- **Middleware**: Authentication, authorization, and monitoring

## 🚀 Features

### Core Functionality
- **Multi-role Authentication**: Student, Instructor, and Admin roles with JWT-based security
- **Assessment Management**: Create, manage, and grade cryptographic assessments
- **Real-time Feedback**: Comprehensive feedback system for student-instructor communication
- **Cryptographic Tools**: Interactive learning modules for cryptography education
- **Section Management**: Course section organization and visibility controls
- **User Management**: Complete user lifecycle management with password reset capabilities

### Technical Features
- **RESTful API Design**: Clean, consistent API endpoints
- **Entity Framework Core**: Code-first database approach with migrations
- **JWT Authentication**: Secure token-based authentication with refresh tokens
- **Role-based Authorization**: Granular permissions system
- **Password Security**: Secure password hashing and reset functionality
- **Monitoring**: Prometheus metrics integration for performance monitoring
- **API Documentation**: Swagger/OpenAPI documentation

## 📋 Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or SQL Server LocalDB
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) (optional)

## 🛠️ Installation & Setup

### 1. Clone the Repository
```bash
git clone <repository-url>
cd polaris/backend
```

### 2. Configure Database Connection
Update the connection string in `appsettings.json` or `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=PolarisDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Install Dependencies
```bash
dotnet restore
```

### 4. Run Database Migrations
```bash
dotnet ef database update
```

### 5. Configure JWT Settings
Update JWT configuration in `appsettings.json`:

```json
{
  "JWT": {
    "Issuer": "https://localhost:7040",
    "Audience": "https://localhost:7040",
    "SigningKey": "your-256-bit-secret-key-here"
  }
}
```

### 6. Run the Application
```bash
dotnet run
```

The API will be available at:
- **HTTPS**: `https://localhost:7040`
- **HTTP**: `http://localhost:5040`
- **Swagger UI**: `https://localhost:7040/swagger`

## 📚 API Documentation

### Authentication Endpoints
- `POST /api/auth/register` - User registration
- `POST /api/auth/login` - User login
- `POST /api/auth/refresh` - Refresh JWT token
- `POST /api/auth/reset-password-request` - Request password reset
- `POST /api/auth/reset-password` - Complete password reset

### Student Endpoints
- `GET /api/student/profile` - Get student profile
- `PUT /api/student/profile` - Update student profile
- `GET /api/student/assessments` - Get available assessments
- `GET /api/student/results` - Get assessment results
- `POST /api/student/feedback` - Submit feedback

### Instructor Endpoints
- `GET /api/instructor/sections` - Get managed sections
- `POST /api/instructor/assessments` - Create assessments
- `GET /api/instructor/students` - Get students in sections
- `GET /api/instructor/feedback` - Get student feedback
- `PUT /api/instructor/assessment-visibility` - Manage assessment visibility

### Admin Endpoints
- `GET /api/admin/users` - Get all users
- `POST /api/admin/users` - Create users
- `PUT /api/admin/users/{id}` - Update user
- `DELETE /api/admin/users/{id}` - Delete user
- `GET /api/admin/system-metrics` - Get system metrics

### Assessment Endpoints
- `GET /api/assessment/{id}` - Get specific assessment
- `POST /api/assessment/{id}/submit` - Submit assessment
- `GET /api/assessment/{id}/results` - Get assessment results

### Feedback Endpoints
- `GET /api/feedback` - Get feedback (role-based)
- `POST /api/feedback` - Submit feedback
- `PUT /api/feedback/{id}` - Update feedback
- `DELETE /api/feedback/{id}` - Delete feedback

## 🗄️ Database Schema

### Core Entities

#### ApplicationUser
- Identity-based user management
- Role assignment (Student, Instructor, Admin)
- Password reset token management

#### Student
- Student-specific profile information
- Section enrollments
- Assessment results

#### Instructor
- Instructor profile and credentials
- Section management
- Assessment creation capabilities

#### Assessment
- Assessment metadata and content
- Due dates and scoring information
- Visibility controls

#### Section
- Course section organization
- Student-instructor relationships
- Assessment visibility management

#### Feedback
- Student-instructor communication
- Assessment-specific feedback
- Response tracking

#### Result
- Assessment submission results
- Scoring and grading information
- Attempt tracking

## 🔧 Configuration

### Environment Variables
```bash
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection="your-connection-string"
JWT__SigningKey="your-jwt-signing-key"
JWT__Issuer="your-issuer"
JWT__Audience="your-audience"
```

### appsettings.json Structure
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": {
    "DefaultConnection": "connection-string-here"
  },
  "JWT": {
    "Issuer": "issuer-url",
    "Audience": "audience-url",
    "SigningKey": "signing-key"
  },
  "AllowedHosts": "*"
}
```

## 🧪 Testing

### Run Unit Tests
```bash
dotnet test
```

### API Testing
Use the included `backend.http` file with Visual Studio Code REST Client extension for API testing, or access Swagger UI at `https://localhost:7040/swagger`.

## 📊 Monitoring

The application includes Prometheus metrics integration. Metrics are available at `/metrics` endpoint for monitoring:

- Request duration and counts
- Authentication success/failure rates
- Database query performance
- Custom business metrics

## 🔒 Security Features

- **JWT Authentication**: Secure token-based authentication
- **Role-based Authorization**: Granular permission system
- **Password Hashing**: Secure password storage with Identity
- **CORS Configuration**: Proper cross-origin resource sharing setup
- **Input Validation**: Comprehensive request validation
- **SQL Injection Protection**: Entity Framework Core parameterization

## 🚀 Deployment

### Development Deployment
```bash
dotnet publish -c Development
```

### Production Deployment
```bash
dotnet publish -c Release -o ./publish
```

### Docker Support
Create a `Dockerfile` in the backend directory:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["backend.csproj", "."]
RUN dotnet restore "./backend.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "backend.dll"]
```

## 📝 Development Guidelines

### Code Style
- Follow Microsoft C# coding conventions
- Use meaningful variable and method names
- Implement proper error handling and logging
- Write unit tests for business logic

### Adding New Features
1. Create appropriate DTOs in the `Dtos` folder
2. Update database models if needed
3. Create/update repositories for data access
4. Implement business logic in services
5. Create controller endpoints
6. Update API documentation
7. Write unit tests

### Database Migrations
```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## 🤝 Contributing

1. Follow the established coding standards
2. Write comprehensive unit tests
3. Update API documentation for new endpoints
4. Ensure proper error handling and logging
5. Test with different user roles

## 📞 Support

For technical support or questions:
- Check the API documentation at `/swagger`
- Review the application logs for error details
- Ensure database connectivity and migrations are up to date
- Verify JWT configuration for authentication issues

## 📄 License

This project is part of the Polaris Learning System for cryptography education.

---

**Built with ❤️ for cryptography education**
