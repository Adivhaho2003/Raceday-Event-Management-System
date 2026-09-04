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

--WEATHERDATA table that will Weather forecasts for events
CREATE TABLE WeatherData (
    WeatherId INT IDENTITY(1,1) PRIMARY KEY,
    EventId INT NOT NULL,
    ForecastDate DATE NOT NULL,
    ForecastTime TIME,
    Temperature FLOAT,
    Humidity FLOAT,
    WindSpeed FLOAT,
    WindDirection NVARCHAR(50),
    Conditions NVARCHAR(255),
    Source NVARCHAR(100),
    RetrievedAt DATETIME DEFAULT GETDATE(),
    
    CONSTRAINT FK_WeatherData_EventId FOREIGN KEY (EventId) REFERENCES Events(EventId)
);  

--Insert Users (2 Organisers, 2 Participants)
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Phone, DateOfBirth, Role)
VALUES 
    ('thabo.mokoena@runnershub.co.za', 'hash_org1_secure', 'Thabo', 'Mokoena', '+27821234567', '1985-03-15', 'Organiser'),
    ('linda.vandermerwe@capetowncycling.co.za', 'hash_org2_secure', 'Linda', 'Van Der Merwe', '+27829876543', '1990-07-22', 'Organiser'),
    ('sipho.ndlovu@gmail.com', 'hash_user1_secure', 'Sipho', 'Ndlovu', '+27836543210', '1992-11-05', 'Participant'),
    ('johannes.smit@outlook.com', 'hash_user2_secure', 'Johannes', 'Smit', '+27837654321', '1988-09-18', 'Participant');

--Insert Categories
INSERT INTO Categories (CategoryName, CategoryType, Distance, AgeGroup, Gender, StandardFee)
VALUES 
    ('5km Fun Run', 'Running', '5 km', 'All Ages', 'Mixed', 100.00),
    ('10km Road Race', 'Running', '10 km', '16+', 'Mixed', 150.00),
    ('Half Marathon', 'Running', '21.1 km', '18+', 'Mixed', 250.00),
    ('Marathon', 'Running', '42.2 km', '20+', 'Mixed', 350.00),
    ('Ultra Marathon', 'Running', '56 km', '20+', 'Mixed', 450.00),
    ('5km Family Walk', 'Walking', '5 km', 'All Ages', 'Mixed', 80.00),
    ('10km Charity Walk', 'Walking', '10 km', 'All Ages', 'Mixed', 120.00),
    ('20km Cycle Tour', 'Cycling', '20 km', '16+', 'Mixed', 200.00),
    ('40km Cycle Challenge', 'Cycling', '40 km', '18+', 'Mixed', 300.00),
    ('100km Cycle Challenge', 'Cycling', '100 km', '18+', 'Mixed', 450.00); 

--Insert Events
INSERT INTO Events (
    EventName, Description, OrganiserId, Location, Venue, 
    EventDate, RegistrationDeadline, EntryFee, MaxParticipants, 
    EventType, Distance, Status
)
VALUES 
    (
        'Soweto Marathon 2026',
        'The iconic Soweto Marathon through the streets of Soweto, celebrating the rich heritage of South African road running.',
        1,
        'Soweto, Johannesburg',
        'FNB Stadium',
        '2026-11-07 06:00:00',
        '2026-10-31 23:59:59',
        300.00,
        15000,
        'Running',
        '42.2 km',
        'Open'
    ),
    (
        'Cape Town Cycle Tour 2026',
        'The largest timed cycling event in the world, riding around the Cape Peninsula with spectacular ocean views.',
        2,
        'Cape Town',
        'Grand Parade, Cape Town CBD',
        '2026-03-08 07:00:00',
        '2026-02-28 23:59:59',
        500.00,
        35000,
        'Cycling',
        '109 km',
        'Open'
    ),
    (
        'Two Oceans Marathon 2026',
        'The "Ultra Marathon" featuring stunning coastal scenery between Cape Town and Hout Bay.',
        1,
        'Cape Town',
        'Newlands Stadium',
        '2026-04-04 05:30:00',
        '2026-03-25 23:59:59',
        400.00,
        12000,
        'Running',
        '56 km',
        'Open'
    ); 

--Link Events to Categories (EventCategories)
INSERT INTO EventCategories (EventId, CategoryId, Fee, Capacity)
SELECT 
    (SELECT EventId FROM Events WHERE EventName = 'Soweto Marathon 2026'),
    CategoryId,
    StandardFee,
    CASE 
        WHEN CategoryName = 'Marathon' THEN 5000
        WHEN CategoryName = 'Half Marathon' THEN 4000
        WHEN CategoryName = '10km Road Race' THEN 3000
        WHEN CategoryName = '5km Fun Run' THEN 3000
        ELSE 2000
    END
FROM Categories
WHERE CategoryName IN ('Marathon', 'Half Marathon', '10km Road Race', '5km Fun Run');

INSERT INTO EventCategories (EventId, CategoryId, Fee, Capacity)
SELECT 
    (SELECT EventId FROM Events WHERE EventName = 'Cape Town Cycle Tour 2026'),
    CategoryId,
    StandardFee,
    CASE 
        WHEN CategoryName = '100km Cycle Challenge' THEN 20000
        WHEN CategoryName = '40km Cycle Challenge' THEN 10000
        WHEN CategoryName = '20km Cycle Tour' THEN 5000
        ELSE 2000
    END
FROM Categories
WHERE CategoryName IN ('100km Cycle Challenge', '40km Cycle Challenge', '20km Cycle Tour');

INSERT INTO EventCategories (EventId, CategoryId, Fee, Capacity)
SELECT 
    (SELECT EventId FROM Events WHERE EventName = 'Two Oceans Marathon 2026'),
    CategoryId,
    CASE 
        WHEN CategoryName = 'Ultra Marathon' THEN 480.00
        WHEN CategoryName = 'Marathon' THEN 400.00
        ELSE StandardFee
    END,
    CASE 
        WHEN CategoryName = 'Ultra Marathon' THEN 5000
        WHEN CategoryName = 'Marathon' THEN 4000
        WHEN CategoryName = 'Half Marathon' THEN 2000
        WHEN CategoryName = '10km Road Race' THEN 1000
        ELSE 500
    END
FROM Categories
WHERE CategoryName IN ('Ultra Marathon', 'Marathon', 'Half Marathon', '10km Road Race');

--Insert Enrolments (Sample Participant Registrations)
DECLARE @Participant1Id INT = (SELECT UserId FROM Users WHERE Email = 'sipho.ndlovu@gmail.com');
DECLARE @Participant2Id INT = (SELECT UserId FROM Users WHERE Email = 'johannes.smit@outlook.com');

INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
SELECT 
    e.EventId,
    @Participant1Id,
    ec.EventCategoryId,
    'Confirmed',
    'REG-2026-001',
    'Paid'
FROM Events e
JOIN EventCategories ec ON e.EventId = ec.EventId
JOIN Categories c ON ec.CategoryId = c.CategoryId
WHERE e.EventName = 'Soweto Marathon 2026'
  AND c.CategoryName = 'Marathon';

INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
SELECT 
    e.EventId,
    @Participant1Id,
    ec.EventCategoryId,
    'Confirmed',
    'REG-2026-002',
    'Paid'
FROM Events e
JOIN EventCategories ec ON e.EventId = ec.EventId
JOIN Categories c ON ec.CategoryId = c.CategoryId
WHERE e.EventName = 'Two Oceans Marathon 2026'
  AND c.CategoryName = 'Ultra Marathon';

INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
SELECT 
    e.EventId,
    @Participant2Id,
    ec.EventCategoryId,
    'Confirmed',
    'REG-2026-003',
    'Paid'
FROM Events e
JOIN EventCategories ec ON e.EventId = ec.EventId
JOIN Categories c ON ec.CategoryId = c.CategoryId
WHERE e.EventName = 'Soweto Marathon 2026'
  AND c.CategoryName = 'Half Marathon';

INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
SELECT 
    e.EventId,
    @Participant2Id,
    ec.EventCategoryId,
    'Confirmed',
    'REG-2026-004',
    'Paid'
FROM Events e
JOIN EventCategories ec ON e.EventId = ec.EventId
JOIN Categories c ON ec.CategoryId = c.CategoryId
WHERE e.EventName = 'Cape Town Cycle Tour 2026'
  AND c.CategoryName = '100km Cycle Challenge';

INSERT INTO Enrolments (EventId, ParticipantId, EventCategoryId, Status, RegistrationNumber, PaymentStatus)
SELECT 
    e.EventId,
    @Participant1Id,
    ec.EventCategoryId,
    'Registered',
    'REG-2026-005',
    'Pending'
FROM Events e
JOIN EventCategories ec ON e.EventId = ec.EventId
JOIN Categories c ON ec.CategoryId = c.CategoryId
WHERE e.EventName = 'Cape Town Cycle Tour 2026'
  AND c.CategoryName = '40km Cycle Challenge';