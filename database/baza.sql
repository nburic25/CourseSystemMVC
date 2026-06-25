CREATE DATABASE CourseSystemDB;
GO

USE CourseSystemDB;
GO

CREATE TABLE Roles (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL
);
GO

CREATE TABLE Categories (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);
GO

CREATE TABLE Instructors (
    InstructorID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    Bio NVARCHAR(255)
);
GO

CREATE TABLE Users (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    FirstName NVARCHAR(50),
    LastName NVARCHAR(50),
    Email NVARCHAR(100) NOT NULL UNIQUE,
    Password NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
    RoleID INT NOT NULL,

    FOREIGN KEY (RoleID) REFERENCES Roles(RoleID)
);
GO

CREATE TABLE Courses (
    CourseID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Price DECIMAL(10,2),
    CategoryID INT NOT NULL,
    InstructorID INT NOT NULL,

    FOREIGN KEY (CategoryID) REFERENCES Categories(CategoryID),
    FOREIGN KEY (InstructorID) REFERENCES Instructors(InstructorID)
);
GO

CREATE TABLE Lessons (
    LessonID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    Content NVARCHAR(MAX),
    CourseID INT NOT NULL,

    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID)
);
GO

CREATE TABLE Materials (
    MaterialID INT IDENTITY(1,1) PRIMARY KEY,
    FilePath NVARCHAR(255),
    LessonID INT NOT NULL,

    FOREIGN KEY (LessonID) REFERENCES Lessons(LessonID)
);
GO

CREATE TABLE Enrollments (
    EnrollmentID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL,
    CourseID INT NOT NULL,
    EnrollmentDate DATETIME DEFAULT GETDATE(),
    Status NVARCHAR(20) DEFAULT 'Active',

    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID),

    CONSTRAINT UQ_Enrollments UNIQUE (UserID, CourseID)
);
GO

CREATE TABLE Reviews (
    ReviewID INT IDENTITY(1,1) PRIMARY KEY,
    Rating INT CHECK (Rating BETWEEN 1 AND 5),
    Comment NVARCHAR(255),
    UserID INT NOT NULL,
    CourseID INT NOT NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID),

    CONSTRAINT UQ_Reviews UNIQUE (UserID, CourseID)
);
GO

CREATE TABLE Payments (
    PaymentID INT IDENTITY(1,1) PRIMARY KEY,
    Amount DECIMAL(10,2) NOT NULL,
    PaymentDate DATETIME DEFAULT GETDATE(),
    UserID INT NOT NULL,
    CourseID INT NOT NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID)
);
GO

CREATE TABLE Notifications (
    NotificationID INT IDENTITY(1,1) PRIMARY KEY,
    Message NVARCHAR(255),
    IsRead BIT DEFAULT 0,
    UserID INT NOT NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID)
);
GO

CREATE TABLE Exams (
    ExamID INT IDENTITY(1,1) PRIMARY KEY,
    Title NVARCHAR(100),
    CourseID INT NOT NULL,

    FOREIGN KEY (CourseID) REFERENCES Courses(CourseID)
);
GO

CREATE TABLE Results (
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    Score INT,
    UserID INT NOT NULL,
    ExamID INT NOT NULL,

    FOREIGN KEY (UserID) REFERENCES Users(UserID),
    FOREIGN KEY (ExamID) REFERENCES Exams(ExamID)
);
GO


-- TEST INSERT
INSERT INTO Categories (Name, Description)
VALUES 
('Programming', 'Software development courses'),
('Design', 'UI/UX design'),
('Databases', 'SQL and data modeling');

INSERT INTO Instructors (FirstName, LastName, Bio)
VALUES 
('John', 'Doe', 'Senior software engineer'),
('Jane', 'Smith', 'UI/UX designer'),
('Mark', 'Brown', 'Database specialist');

-- DELETE FROM Reviews
-- WHERE Rating = 4;

-- DELETE FROM Courses
-- WHERE CourseID = 2

-- SELECT * FROM Courses
-- SELECT * FROM Lessons
-- SELECT * FROM Materials
-- SELECT * FROM Enrollments
-- SELECT * FROM Exams
-- SELECT * FROM Users

-- DELETE FROM Users
-- WHERE RoleID = 2

-- DELETE FROM Payments

-- SELECT * FROM Notifications;
-- DELETE FROM Notifications