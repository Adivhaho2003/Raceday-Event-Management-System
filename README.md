# 🏃 RaceDay Event Management System

## 📋 Project Overview

**RaceDay** is a full-stack web-based event management system designed specifically for the South African road running, walking, and cycling community. The platform allows Event Organisers to create and manage events, categories, and participant results, while Participants can browse upcoming events, enter events, track their personal performance history, and prepare for race day using live weather and route information.

### 🌍 South African Context

South Africa has a rich road events culture, from the iconic Comrades Marathon between Pietermaritzburg and Durban, to the Cape Town Cycle Tour, the Soweto Marathon, the Two Oceans, and hundreds of community walks, park runs, and charity cycling events held in towns and cities across the country every weekend. Despite the enormous participation these events attract, many are still managed through paper-based registration, spreadsheets, and disconnected communication channels, leaving organisers overwhelmed and participants underserved.

**RaceDay** solves this problem by providing a modern, integrated platform for event management.

---

## 🎯 Project Goals

- Build a complete event management system from planning to deployment
- Demonstrate full-stack development skills using modern technologies
- Implement role-based access control for Organisers and Participants
- Create a scalable, cloud-aware, containerised application
- Follow real-world software development practices used in the sports technology industry

---

## 🏗️ Architecture Overview

The project is built across three progressive parts:

| Part | Focus | Technologies |
|------|-------|--------------|
| **Part 1** | System Planning and Database | SQL Server, ERD Design, API Planning |
| **Part 2** | RESTful API Development | ASP.NET Core Web API, Entity Framework Core, Unit Testing, GitHub Actions |
| **Part 3** | MVC Frontend + Azure + Docker | ASP.NET Core MVC, Azure Blob Storage, Docker |

---

## 👥 System Roles

| Role | Responsibilities | Access Level |
|------|------------------|--------------|
| **Organiser** | Create and manage events, define categories, view enrolments, capture results | Full event management |
| **Participant** | Browse events, enrol in events, view personal results and history | Self-service access |
| **Admin** | Manage users, system-wide oversight | Full system access |

---

## 📁 Repository Structure
Raceday-Event-Management-System/
│
├── docs/
│ ├── ERD.png # Entity Relationship Diagram
│ ├── API_ENDPOINT_PLAN.pdf # Complete API endpoint specification
│ └── raceday_eventDB.sql # Full database schema with seed data
│
├── RaceDayAPI/ # Part 2 - ASP.NET Core Web API (Coming Soon)
│ ├── Controllers/
│ ├── Models/
│ ├── Data/
│ ├── Services/
│ ├── DTOs/
│ ├── Tests/
│ └── Program.cs
│
├── RaceDayMVC/ # Part 3 - ASP.NET Core MVC (Coming Soon)
│ ├── Controllers/
│ ├── Views/
│ ├── Models/
│ ├── Services/
│ └── wwwroot/
│
├── .github/workflows/
│ └── ci.yml # GitHub Actions CI/CD Pipeline
│
├── Dockerfile # Docker Container Configuration
├── README.md # Project Documentation
└── .gitignore # Git Ignore File


---

## 🗄️ Database Schema (Part 1)

### Database Name: `raceday_eventDB`

### Entities (7 Tables)

| # | Table Name | Description | Primary Key | Foreign Keys |
|---|------------|-------------|-------------|--------------|
| 1 | **Users** | System users (Organisers & Participants) | UserId | - |
| 2 | **Events** | Event details | EventId | OrganiserId → Users.UserId |
| 3 | **Categories** | Race categories | CategoryId | - |
| 4 | **EventCategories** | Junction table (Events ↔ Categories) | EventCategoryId | EventId → Events.EventId, CategoryId → Categories.CategoryId |
| 5 | **Enrolments** | Participant registrations | EnrolmentId | EventId → Events.EventId, ParticipantId → Users.UserId, EventCategoryId → EventCategories.EventCategoryId |
| 6 | **Results** | Race results | ResultId | EnrolmentId → Enrolments.EnrolmentId, ParticipantId → Users.UserId, EventId → Events.EventId, CategoryId → Categories.CategoryId |
| 7 | **WeatherData** | Weather forecasts | WeatherId | EventId → Events.EventId |

### Entity Relationship Diagram (ERD)

---

## 📡 API Endpoint Plan (Part 1)

The API follows RESTful principles with role-based access control.

### Resource Areas (7)

| # | Resource Area | Endpoints |
|---|---------------|-----------|
| 1 | **Authentication** | Register, Login, Logout, Profile Management |
| 2 | **User Profile** | View and manage user profiles |
| 3 | **Events** | Full CRUD operations for events |
| 4 | **Categories** | Manage race categories |
| 5 | **Event Categories** | Link events to categories (junction table) |
| 6 | **Enrolments** | Participant registrations for events |
| 7 | **Results** | Race results management |

### Example Endpoint

| HTTP Method | Route | Description | Role Required | Request Body | Expected Response |
|-------------|-------|-------------|---------------|--------------|-------------------|
| POST | /api/auth/register | Register a new user account | None (Public) | `{ "email": "string", "password": "string", "firstName": "string", "lastName": "string", "role": "string" }` | 201 Created - user object with token |


---

## 🚀 Setup Instructions

### Prerequisites

- SQL Server Management Studio (SSMS)
- SQL Server (Local or Azure)
- Git
- (For Part 2 & 3: .NET 7/8 SDK, Visual Studio 2022+, Docker Desktop)

### 1. Clone the Repository

```bash
git clone https://github.com/Adiwhaho2003/Raceday-Event-Management-System.git
cd Raceday-Event-Management-System

2. Create the Database
Open SQL Server Management Studio (SSMS)

Connect to your SQL Server instance

Open the file: docs/raceday_eventDB.sql

Execute the script (F5)

Verify the database raceday_eventDB is created with all tables and seed data

3. Verify Database Setup
Run these queries to verify everything is working:
USE raceday_eventDB;

-- Check all tables exist
SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES;

-- Check sample data
SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Enrolments;
SELECT * FROM Results;

4. Sample Data
The database includes realistic South African sample data:

Data Type	Sample Records
Organisers	Thabo Mokoena (RunnersHub), Linda Van Der Merwe (Cape Town Cycling)
Participants	Sipho Ndlovu, Johannes Smit
Events	Soweto Marathon, Cape Town Cycle Tour, Two Oceans Marathon
Categories	10 categories (Running, Walking, Cycling)
Enrolments	5 sample registrations (Confirmed & Pending)
Results	4 completed race results with finish times
Weather	6 weather forecast entries

📊 Part 1 Deliverables
Deliverable	File	Status
ERD Diagram	docs/ERD.png	✅ Completed
API Endpoint Plan	docs/API_ENDPOINT_PLAN.pdf	✅ Completed
SQL Database Script	docs/raceday_eventDB.sql	✅ Completed
GitHub Repository	https://github.com/Adiwhaho2003/Raceday-Event-Management-System	✅ Active
Video Presentation	[YouTube Link]	⏳ In Progress

Minimum Requirements Met
✅ Minimum 6 entities (7 implemented)
✅ All primary keys, foreign keys, and constraints defined
✅ SQL script matches ERD exactly
✅ 2 Organisers, 2 Participants, 3 Events seeded
✅ API Endpoint Plan covers all 7 resource areas
✅ All planning documents in /docs folder

🎥 Video Presentation
An unlisted YouTube video is available demonstrating:
ERD design decisions and explanations
API endpoint plan walkthrough
SQL script execution in SSMS
Database verification queries
Link to YouTube Video: https://youtu.be/SqR719-4qcc

🔄 CI/CD Pipeline
A GitHub Actions workflow will be configured to:
Build the API and MVC projects
Run all unit tests
Report build status with a green badge
Automate deployment workflows

<img width="655" height="288" alt="image" src="https://github.com/user-attachments/assets/90cf1b19-9c61-4a01-9a02-f2990fafae89" />

🛠️ Technologies Used
Technology	Purpose
SQL Server	Relational database
SSMS	Database management
draw.io	ERD creation
Git	Version control
GitHub	Repository hosting
Markdown	Documentation
ASP.NET Core Web API	RESTful API (Part 2)
Entity Framework Core	ORM (Part 2)
xUnit	Unit testing (Part 2)
GitHub Actions	CI/CD (Part 2 & 3)
ASP.NET Core MVC	Frontend (Part 3)
Azure Blob Storage	File storage (Part 3)
Docker	Containerization (Part 3)
Bootstrap	UI Framework (Part 3)

👨‍💻 Author
Your Name: Adivhaho Ntsengeni
