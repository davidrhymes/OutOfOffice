create database outofoffice
GO

use outofoffice
GO

CREATE TABLE Employees (
    ID INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(255) NOT NULL,
    Subdivision NVARCHAR(255) NOT NULL,
    Position NVARCHAR(255) NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    PeoplePartnerID INT NULL FOREIGN KEY (PeoplePartnerID) REFERENCES Employees(ID) ON DELETE NO ACTION,
    Balance DECIMAL(18, 2) NOT NULL,
    Photo VARBINARY(MAX) NULL,
	Login VARCHAR(255) NOT NULL,
	Password VARCHAR(255) NOT NULL,
	Role INT NOT NULL
);

CREATE TABLE LeaveRequests (
    ID INT PRIMARY KEY IDENTITY(1,1),
    EmployeeID INT NOT NULL FOREIGN KEY (EmployeeID) REFERENCES Employees(ID) ON DELETE CASCADE,
    AbsenceReason NVARCHAR(255) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NOT NULL,
    Comment NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL,
);

CREATE TABLE ApprovalRequests (
    ID INT PRIMARY KEY IDENTITY(1,1),
    ApproverID INT NOT NULL FOREIGN KEY (ApproverID) REFERENCES Employees(ID),
    LeaveRequestID INT NOT NULL FOREIGN KEY (LeaveRequestID) REFERENCES LeaveRequests(ID),
    Status NVARCHAR(50) NOT NULL,
    Comment NVARCHAR(MAX), 
);

CREATE TABLE Projects (
    ID INT PRIMARY KEY IDENTITY(1,1),
    ProjectType NVARCHAR(255) NOT NULL,
    StartDate DATE NOT NULL,
    EndDate DATE NULL,
    ProjectManagerID INT NOT NULL FOREIGN KEY (ProjectManagerID) REFERENCES Employees(ID),
    Comment NVARCHAR(MAX) NULL,
    Status NVARCHAR(50) NOT NULL,
);

CREATE TABLE ProjectEmployees (
	ID INT PRIMARY KEY IDENTITY(1,1),
    ProjectID INT NOT NULL FOREIGN KEY (ProjectID) REFERENCES Projects(ID) ON DELETE CASCADE,
    EmployeeID INT NOT NULL FOREIGN KEY (EmployeeID) REFERENCES Employees(ID) ON DELETE CASCADE
);

INSERT INTO Employees (FullName, Subdivision, Position, Status, PeoplePartnerID, Balance, Photo, Login, Password, Role)
VALUES ('John Doe', 'ADMIN', 'ADMIN', 'Active', NULL, 1000.00, NULL, 'admin', 'admin', 1);

GO