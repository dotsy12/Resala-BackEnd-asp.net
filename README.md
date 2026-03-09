# Back-end-Clean-Architecture-Template

A clean and professional ASP.NET Core 8 backend API built with Clean Architecture, CQRS, and modern best practices.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [API Endpoints](#api-endpoints)
- [Authentication & Authorization](#authentication--authorization)
- [Project Architecture](#project-architecture)
- [Contributing](#contributing)
- [License](#license)

## 🎯 Overview

This project is a production-ready backend API that implements the latest architectural patterns and best practices. It provides user authentication, product management, and employee management features with a secure, scalable, and maintainable codebase.

## ✨ Features

- **User Authentication & Authorization**
  - JWT-based authentication
  - Refresh token mechanism
  - Role-based access control (Admin, Employee)
  - Secure password reset functionality

- **Product Management To Test**
  - Create, Read, Update, Delete (CRUD) operations
  - Advanced search and filtering
  - Pagination support
  - Soft delete mechanism

- **Employee Management**
  - Employee registration with age validation (18-60 years)
  - Employee profile management
  - User-employee relationship tracking

- **Clean Code Architecture**
  - CQRS pattern implementation
  - Fluent Validation integration
  - Unit of Work pattern
  - Generic Repository pattern
  - AutoMapper for DTO mapping

- **Security**
  - JWT token-based authentication
  - Password encryption
  - Secure cookie handling
  - Role-based authorization

## 🛠 Tech Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity + JWT
- **CQRS**: MediatR
- **Validation**: FluentValidation
- **Mapping**: AutoMapper
- **API Documentation**: Swagger/OpenAPI

## 📁 Project Structure

```
BackEndProject/
├── BackEnd.Application/          # Application Layer
│   ├── Abstractions/             # Interfaces and abstractions
│   │   ├── Messaging/            # ICommand, IQuery interfaces
│   │   ├── Persistence/          # Repository interfaces
│   │   └── Queries/              # Query interfaces
│   ├── Features/                 # Feature-based organization
│   │   ├── LoginAndTokens/       # Authentication features
│   │   ├── Password/             # Password management
│   │   ├── Products/             # Product management
│   │   └── RegisterUser/         # User registration
│   ├── Behaviors/                # MediatR pipeline behaviors
│   ├── Common/                   # Common utilities
│   ├── Dependencies/             # Dependency injection setup
│   ├── Interfaces/               # Service interfaces
│   └── Mappings/                 # AutoMapper profiles
│
├── BackEnd.Domain/               # Domain Layer
│   ├── Entities/                 # Domain entities
│   │   ├── Identity/             # User and role entities
│   │   └── Product.cs
│   ├── Common/                   # Base entities
│   └── Interfaces/               # Domain interfaces
│
├── BackEnd.Infrastructure/       # Infrastructure Layer
│   ├── DependencyInjection/      # DI configuration
│   ├── Persistence/
│   │   ├── DbContext/            # EF Core DbContext
│   │   ├── Repositories/         # Repository implementations
│   │   ├── Configurations/       # Entity configurations
│   │   └── UnitOfWork.cs
│   ├── Migrations/               # EF Core migrations
│   ├── Services/                 # Service implementations
│   └── obj/                      # Build output
│
└── BackEndApi/                   # Presentation Layer
    ├── Controllers/              # API controllers
    ├── Filters/                  # Action filters
    ├── Common/                   # Controller base classes
    └── Program.cs                # Application startup
```

## 📦 Prerequisites

- **.NET SDK 8.0** or higher
- **SQL Server** 2019 or higher
- **Visual Studio** 2022 or **Visual Studio Code**
- **Git**

## 🚀 Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/BackEndProject.git
cd BackEndProject
```

### 2. Install Dependencies

```bash
dotnet restore
```

### 3. Update Connection String

Edit `appsettings.json` in the `BackEndApi` project:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BackEndDb;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=true;"
  },
  "jwtSittings": {
    "Key": "your-secret-key-min-32-characters-long!",
    "Issuer": "YourIssuer",
    "Audience": "YourAudience",
    "DurationInHours": 1
  }
}
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project BackEnd.Infrastructure
```

### 5. Run the Application

```bash
cd BackEndApi
dotnet run
```

The API will be available at `https://localhost:7000` (or as configured).

## ⚙️ Configuration

### appsettings.json Example

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=BackEndDb;Integrated Security=true;TrustServerCertificate=true;"
  },
  "jwtSittings": {
    "Key": "your-very-secret-key-that-is-at-least-32-characters-long",
    "Issuer": "BackEndApp",
    "Audience": "BackEndAppUsers",
    "DurationInHours": 1
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  }
}
```

### Key Configuration Points

- **JWT Settings**: Configure in `appsettings.json` for token generation
- **Database**: Update connection string for your SQL Server instance
- **CORS**: Configure in `Program.cs` if needed for frontend integration

## 🗄️ Database Setup

### Initial Setup

```bash
# Create migration
dotnet ef migrations add InitialCreate --project BackEnd.Infrastructure

# Apply migration
dotnet ef database update --project BackEnd.Infrastructure
```

### Seeded Data

The database comes with default admin user:

```
Email: admin@admin.com
Password: Admin@123 (hashed)
Role: Admin
```

⚠️ **Change this password in production!**

## 📡 API Endpoints

### Authentication Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/authentication/Register-Admin` | Register new admin |
| POST | `/api/authentication/Register-Employee` | Register new employee |
| POST | `/api/authentication/Login` | User login |
| POST | `/api/authentication/Generate-New-token-From-RefreshToken` | Refresh JWT token |
| POST | `/api/authentication/Forget-Password` | Request password reset token |
| POST | `/api/authentication/Rest-Password` | Reset password |
| POST | `/api/authentication/Logout` | User logout |

### Product Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/products/Create-Product` | Create new product |
| PUT | `/api/products/Update-Product` | Update product |
| PUT | `/api/products/Delete-Product` | Delete product (soft delete) |
| GET | `/api/products/Get-Product-ById?id=1` | Get product by ID |
| POST | `/api/products/Product-Search` | Search products with filters |

## 🔐 Authentication & Authorization

### JWT Token Flow

1. **Login**: User credentials → JWT token + Refresh token
2. **Access**: Include token in `Authorization: Bearer <token>` header
3. **Refresh**: Use refresh token to get new JWT token
4. **Logout**: Invalidate refresh token

### Roles

- **Admin**: Full access to all endpoints
- **Employee**: Limited access based on permissions

### Protected Endpoints

Most endpoints require JWT authentication. Include the token in the request header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

## 🏗️ Project Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│   Presentation Layer (API)          │  Controllers, Filters, Response Models
├─────────────────────────────────────┤
│   Application Layer                 │  CQRS, Commands, Queries, Validators
├─────────────────────────────────────┤
│   Domain Layer                      │  Entities, Value Objects, Interfaces
├─────────────────────────────────────┤
│   Infrastructure Layer              │  Repositories, Services, DbContext
└─────────────────────────────────────┘
```

### Design Patterns Implemented

- **CQRS**: Separates read (Query) and write (Command) operations
- **Repository Pattern**: Data access abstraction
- **Unit of Work**: Transaction management
- **Dependency Injection**: Loose coupling via DI container
- **Validation Pipeline**: FluentValidation integration via MediatR behavior
- **DTO Pattern**: Data transfer objects for API responses

## 🔧 Development Workflow

### Adding a New Feature

1. **Create Domain Entity** in `BackEnd.Domain/Entities/`
2. **Create DTOs** in `BackEnd.Application/Features/YourFeature/Dto/`
3. **Create Handler** (Command/Query) in `BackEnd.Application/Features/YourFeature/`
4. **Create Validator** in `BackEnd.Application/Features/YourFeature/Validator/`
5. **Create Repository** in `BackEnd.Infrastructure/Persistence/Repositories/`
6. **Create Controller** in `BackEndApi/Controllers/`
7. **Create EF Configuration** in `BackEnd.Infrastructure/Persistence/Configurations/`
8. **Create Migration**:
   ```bash
   dotnet ef migrations add YourFeatureName --project BackEnd.Infrastructure
   dotnet ef database update --project BackEnd.Infrastructure
   ```

### Running Tests

```bash
dotnet test
```

## 📝 Code Examples

### Creating a Product

```http
POST /api/products/Create-Product
Content-Type: application/json
Authorization: Bearer {token}

{
  "name": "Laptop",
  "description": "High-performance laptop",
  "price": 999.99
}
```

### Searching Products

```http
POST /api/products/Product-Search
Content-Type: application/json
Authorization: Bearer {token}

{
  "search": "laptop",
  "minPrice": 500,
  "maxPrice": 2000,
  "pageIndex": 1,
  "pageSize": 10
}
```

### User Login

```http
POST /api/authentication/Login
Content-Type: application/x-www-form-urlencoded

email=user@example.com&password=YourPassword&rememberMe=true
```

## 🐛 Troubleshooting

### Common Issues

**Issue**: Database connection failed
- **Solution**: Check connection string in `appsettings.json`
- Verify SQL Server is running
- Ensure database credentials are correct

**Issue**: JWT token validation fails
- **Solution**: Verify `jwtSittings` in `appsettings.json`
- Check token hasn't expired
- Ensure secret key matches

**Issue**: Migration errors
- **Solution**: Delete previous migrations and start fresh
- Run `dotnet ef database update` again

## 📚 Additional Resources

- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit changes (`git commit -m 'Add AmazingFeature'`)
4. Push to branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

### Coding Standards

- Follow C# naming conventions (PascalCase for classes, camelCase for variables)
- Write meaningful commit messages
- Include XML comments for public methods
- Keep methods focused and small
- Use dependency injection over static references

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👨‍💻 Author

**Ahmed Sabry**

---

**Last Updated**: February 2026

Happy coding! 🚀
"# Resala-BackEnd-asp.net" 
