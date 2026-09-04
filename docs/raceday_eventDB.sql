--Creating a database
CREATE DATABASE raceday_eventDB;

--Use the database
use [raceday_eventDB];

--USERS table to stores all system users
CREATE TABLE Users (
    UserId INT IDENTITY(1,1) PRIMARY KEY,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    DateOfBirth DATE,
    Role NVARCHAR(50) NOT NULL DEFAULT 'Participant',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CK_Users_Role CHECK (Role IN ('Participant', 'Organiser', 'Admin'))
);

--Events to store events created by Organisers
CREATE TABLE Events (
    EventId INT IDENTITY(1,1) PRIMARY KEY,
    EventName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX),
    OrganiserId INT NOT NULL,
    Location NVARCHAR(255) NOT NULL,
    Venue NVARCHAR(255),
    EventDate DATETIME NOT NULL,
    RegistrationDeadline DATETIME,
    EntryFee DECIMAL(10,2) DEFAULT 0.00,
    MaxParticipants INT,
    EventType NVARCHAR(50) NOT NULL,
    Distance NVARCHAR(50),
    Status NVARCHAR(50) DEFAULT 'Pending',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Events_OrganiserId FOREIGN KEY (OrganiserId) REFERENCES Users(UserId),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('Pending', 'Open', 'Closed', 'Cancelled', 'Completed')),
    CONSTRAINT CK_Events_EventType CHECK (EventType IN ('Running', 'Walking', 'Cycling'))
);

--CATEGORIES TABLE to store race categories
CREATE TABLE Categories (
    CategoryId INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL,
    CategoryType NVARCHAR(50) NOT NULL,
    Distance NVARCHAR(50),
    AgeGroup NVARCHAR(50),
    Gender NVARCHAR(20),
    StandardFee DECIMAL(10,2) DEFAULT 0.00,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT CK_Categories_Type CHECK (CategoryType IN ('Running', 'Walking', 'Cycling')),
    CONSTRAINT CK_Categories_Gender CHECK (Gender IN ('Male', 'Female', 'Mixed', NULL))
); 

--EVENTCATEGORIES TABLE that Links Events to Categories with event-specific fees and capacities
CREATE TABLE EventCategories (
    EventCategoryId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    Fee DECIMAL(10,2),
    Capacity INT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_EventCategories_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_EventCategories_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT UQ_EventCategories_EventCategory UNIQUE (EventId, CategoryId),
    CONSTRAINT CK_EventCategories_Capacity CHECK (Capacity > 0)
); 

--ENROLMENTS table for Participant registrations for events
CREATE TABLE Enrolments (
    EnrolmentId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    ParticipantId INT NOT NULL,
    EventCategoryId INT NOT NULL,
    Status NVARCHAR(50) DEFAULT 'Registered',
    RegistrationNumber NVARCHAR(50) NOT NULL,
    RegistrationDate DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    ConfirmationDate DATETIME,
    PaymentStatus NVARCHAR(50) DEFAULT 'Pending',
    
    CONSTRAINT FK_Enrolments_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_Enrolments_ParticipantId FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT FK_Enrolments_EventCategoryId FOREIGN KEY (EventCategoryId) REFERENCES EventCategories(EventCategoryId),
    CONSTRAINT CK_Enrolments_Status CHECK (Status IN ('Registered', 'Confirmed', 'DNS', 'DNF', 'Completed')),
    CONSTRAINT CK_Enrolments_PaymentStatus CHECK (PaymentStatus IN ('Pending', 'Paid', 'Refunded')),
    CONSTRAINT UQ_Enrolments_RegistrationNumber UNIQUE (RegistrationNumber),
    CONSTRAINT UQ_Enrolments_ParticipantEvent UNIQUE (ParticipantId, EventId)
); 

--RESULTS table that will store race results captured by Organisers
CREATE TABLE Results (
    ResultId INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentId INT NOT NULL,
    ParticipantId INT NOT NULL,
    EventId INT NOT NULL,
    CategoryId INT NOT NULL,
    GunTime TIME,
    ChipTime TIME,
    OverallPosition INT,
    CategoryPosition INT,
    Pace FLOAT,
    Status NVARCHAR(50) DEFAULT 'Completed',
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_Results_EnrolmentId FOREIGN KEY (EnrolmentId) REFERENCES Enrolments(EnrolmentId),
    CONSTRAINT FK_Results_ParticipantId FOREIGN KEY (ParticipantId) REFERENCES Users(UserId),
    CONSTRAINT FK_Results_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId),
    CONSTRAINT FK_Results_CategoryId FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId),
    CONSTRAINT CK_Results_Status CHECK (Status IN ('DNS', 'DNF', 'Completed')),
    CONSTRAINT UQ_Results_Enrolment UNIQUE (EnrolmentId)
);