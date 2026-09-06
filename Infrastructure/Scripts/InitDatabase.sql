-- ==========================================================
-- Database Initialization Script for Conference Room Booking API
-- Creates Tables and Seeds Initial Data
-- ==========================================================

-- 1. Create Rooms Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Rooms')
BEGIN
    CREATE TABLE Rooms (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Capacity INT NOT NULL,
        BasePricePerHour DECIMAL(18,2) NOT NULL
    );
END;

-- 2. Create Services Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Services')
BEGIN
    CREATE TABLE Services (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Price DECIMAL(18,2) NOT NULL
    );
END;

-- 3. Create Bookings Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    CREATE TABLE Bookings (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoomId INT NOT NULL,
        BookingDate DATETIME2 NOT NULL,
        DurationHours INT NOT NULL,
        TotalPrice DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Bookings_Rooms FOREIGN KEY (RoomId) REFERENCES Rooms(Id) ON DELETE CASCADE
    );
END;

-- 4. Create BookingServices Junction Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'BookingServices')
BEGIN
    CREATE TABLE BookingServices (
        BookingId INT NOT NULL,
        ServiceId INT NOT NULL,
        PRIMARY KEY (BookingId, ServiceId),
        CONSTRAINT FK_BookingServices_Bookings FOREIGN KEY (BookingId) REFERENCES Bookings(Id) ON DELETE CASCADE,
        CONSTRAINT FK_BookingServices_Services FOREIGN KEY (ServiceId) REFERENCES Services(Id) ON DELETE CASCADE
    );
END;

-- 5. Seed Initial Rooms
IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Room A')
BEGIN
    INSERT INTO Rooms (Name, Capacity, BasePricePerHour) VALUES ('Room A', 50, 2000.00);
END;

IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Room B')
BEGIN
    INSERT INTO Rooms (Name, Capacity, BasePricePerHour) VALUES ('Room B', 100, 3500.00);
END;

IF NOT EXISTS (SELECT 1 FROM Rooms WHERE Name = 'Room C')
BEGIN
    INSERT INTO Rooms (Name, Capacity, BasePricePerHour) VALUES ('Room C', 30, 1500.00);
END;

-- 6. Seed Initial Services
IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Projector')
BEGIN
    INSERT INTO Services (Name, Price) VALUES ('Projector', 500.00);
END;

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Wi-Fi')
BEGIN
    INSERT INTO Services (Name, Price) VALUES ('Wi-Fi', 300.00);
END;

IF NOT EXISTS (SELECT 1 FROM Services WHERE Name = 'Sound')
BEGIN
    INSERT INTO Services (Name, Price) VALUES ('Sound', 700.00);
END;
