# Employee Recognition & Feedback System

## 📌 Project Overview

Employee Recognition & Feedback System is a web-based application designed to improve employee engagement, motivation, and workplace communication.

The system allows employees to recognize colleagues for their achievements, submit feedback, and track recognition activities. Managers can monitor employee performance, review feedback, and generate insights, while administrators manage the overall platform.

---

## 🚀 Features

### 👨‍💼 Employee Module
- Employee Registration & Login
- Secure Authentication
- Dashboard Overview
- Give Recognition to Colleagues
- Submit Feedback
- View Recognition Feed
- View Personal Profile
- Track Recognition History

### 👨‍💻 Manager Module
- Manager Dashboard
- View Team Members
- Monitor Employee Feedback
- Review Recognition Activities
- Employee Performance Insights
- Approve/Manage Recognition Records

### 🔧 Admin Module
- User Management
- Employee Management
- Manager Management
- Role Management
- Dashboard Analytics
- System Configuration

---

## 🏗️ System Architecture

Frontend (Angular 19)
        │
        ▼
ASP.NET Core Web API
        │
        ▼
Entity Framework Core
        │
        ▼
SQL Server Database

---

## 🛠️ Technology Stack

### Frontend
- Angular 19
- TypeScript
- HTML5
- CSS3
- Bootstrap
- Angular Reactive Forms
- Angular Router
- RxJS

### Backend
- ASP.NET Core Web API
- Entity Framework Core
- C#
- RESTful APIs
- JWT Authentication

### Database
- SQL Server

### Tools
- Visual Studio 2022
- Visual Studio Code
- SQL Server Management Studio (SSMS)
- Git & GitHub
- Postman

---

## 📂 Project Structure

### Frontend (Angular)

src/
│
├── app/
│ ├── employee/
│ ├── manager/
│ ├── admin/
│ ├── shared/
│ ├── services/
│ ├── guards/
│ ├── interceptors/
│ └── models/
│
├── assets/
└── environments/

### Backend (.NET)

EmployeeRecognitionAPI/
│
├── Controllers/
├── Services/
├── Repositories/
├── Data/
├── Models/
├── DTOs/
├── Interfaces/
├── Middleware/
├── Helpers/
└── Migrations/

---

## 🔐 Authentication & Authorization

The application uses JWT (JSON Web Token) Authentication.

### Roles

- Employee
- Manager
- Admin

### Security Features

- JWT Token Authentication
- Role-Based Authorization
- Protected Routes
- HTTP Interceptor
- Password Encryption

---

## 📊 Database Modules

### Employee Table
- EmployeeId
- FullName
- Email
- Department
- Designation
- PasswordHash

### Recognition Table
- RecognitionId
- SenderId
- ReceiverId
- Message
- RecognitionDate

### Feedback Table
- FeedbackId
- EmployeeId
- FeedbackText
- Rating
- CreatedDate

### Manager Table
- ManagerId
- Name
- Email

### Admin Table
- AdminId
- Name
- Email

---

## ⚙️ Installation

### Clone Repository

```bash
git clone https://github.com/yourusername/employee-recognition-system.git
```

### Frontend Setup

```bash
cd frontend
npm install
ng serve
```

Application runs on:

```bash
http://localhost:4200
```

### Backend Setup

```bash
cd EmployeeRecognitionAPI
```

Restore Packages:

```bash
dotnet restore
```

Run Migration:

```bash
Update-Database
```

Run API:

```bash
dotnet run
```

API runs on:

```bash
https://localhost:5001
```

---

## 📡 API Endpoints

### Authentication

| Method | Endpoint |
|----------|-------------|
| POST | /api/auth/register |
| POST | /api/auth/login |

### Employee

| Method | Endpoint |
|----------|-------------|
| GET | /api/employees |
| GET | /api/employees/{id} |
| PUT | /api/employees/{id} |

### Recognition

| Method | Endpoint |
|----------|-------------|
| POST | /api/recognition |
| GET | /api/recognition/feed |

### Feedback

| Method | Endpoint |
|----------|-------------|
| POST | /api/feedback |
| GET | /api/feedback |

---

## 🎯 Business Problem Solved

Many organizations struggle with:

- Lack of employee motivation
- Poor appreciation culture
- Limited communication between employees and managers
- Difficulty tracking employee achievements
- Manual feedback processes

This system provides a centralized platform for employee recognition and feedback management.

---

## 📈 Future Enhancements

- Email Notifications
- Real-Time Chat
- Recognition Points System
- Reward Redemption
- Performance Analytics Dashboard
- AI-Based Feedback Analysis
- Mobile Application

---

## 👨‍💻 Developed By

Tejas Thorat

### Tech Stack
Angular 19 | ASP.NET Core Web API | Entity Framework Core | SQL Server

---

## 📄 License

This project is developed for educational and learning purposes.
