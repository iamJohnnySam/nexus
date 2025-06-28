CREATE TABLE IF NOT EXISTS Project (
    ProjectId INTEGER PRIMARY KEY,
    ProjectName TEXT NOT NULL,
    CustomerID INTEGER,
    DesignCode TEXT,
    Priority INTEGER,
    POStatus INTEGER,
    ProductId INTEGER
);

CREATE TABLE IF NOT EXISTS Module (
    ModuleId INTEGER PRIMARY KEY,
    ModuleName TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS Product (
    ProductId INTEGER PRIMARY KEY,
    ProductName TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS ReviewItem (
    ReviewItemId INTEGER PRIMARY KEY,
    ProjectId INTEGER,
    Approved INTEGER,
    LastReviewDate TEXT,
    ReviewComments TEXT,
    ReviewResponsibleID INTEGER
);

CREATE TABLE IF NOT EXISTS ReviewPoint (
    ReviewPointId INTEGER PRIMARY KEY,
    ModuleId INTEGER,
    ReviewDescription TEXT NOT NULL
);

CREATE TABLE IF NOT EXISTS TaskItem (
    TaskId INTEGER PRIMARY KEY,
    ProjectId INTEGER,
    Title TEXT NOT NULL,
    CreatedOn TEXT,
    Deadline TEXT NOT NULL,
    IsCompleted INTEGER,
    ParentTaskId INTEGER
);

CREATE TABLE IF NOT EXISTS TaskItem_Responsibles (
    TaskId INTEGER,
    EmployeeId INTEGER
);

CREATE TABLE IF NOT EXISTS Project_PreviousCodes (
    ProjectId INTEGER,
    Code TEXT
);

CREATE TABLE IF NOT EXISTS Project_POCodes (
    ProjectId INTEGER,
    Code TEXT
);

CREATE TABLE IF NOT EXISTS Designation (
    DesignationId INTEGER PRIMARY KEY,
    DesignationName TEXT NOT NULL,
    Department TEXT
);

CREATE TABLE IF NOT EXISTS Employee (
    EmployeeId INTEGER PRIMARY KEY,
    EmployeeName TEXT NOT NULL,
    GradeId INTEGER,
    DesignationId INTEGER,
    JoinDate TEXT,
    LeaveDate TEXT,
    IsActive INTEGER,
    ReplacedEmployeeId INTEGER
);

CREATE TABLE IF NOT EXISTS Grade (
    GradeId INTEGER PRIMARY KEY,
    GradeName TEXT NOT NULL,
    GradeScore INTEGER
);
