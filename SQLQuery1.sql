USE [SaigonRideDB]
GO

-- Xóa bảng cũ nếu có (để tránh conflict)
IF OBJECT_ID('dbo.Payments', 'U') IS NOT NULL DROP TABLE dbo.Payments;
IF OBJECT_ID('dbo.Rentals', 'U') IS NOT NULL DROP TABLE dbo.Rentals;
IF OBJECT_ID('dbo.Vehicles', 'U') IS NOT NULL DROP TABLE dbo.Vehicles;
IF OBJECT_ID('dbo.Users', 'U') IS NOT NULL DROP TABLE dbo.Users;
IF OBJECT_ID('dbo.Stations', 'U') IS NOT NULL DROP TABLE dbo.Stations;
GO

-- Tạo lại bảng từ đầu
CREATE TABLE [dbo].[Stations](
    [StationID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [StationName] [nvarchar](100) NOT NULL,
    [Location] [nvarchar](200) NULL,
    [Capacity] [int] NOT NULL,
    [CurrentInventory] [int] NULL DEFAULT(0)
)
GO

CREATE TABLE [dbo].[Users](
    [UserID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [FullName] [nvarchar](100) NOT NULL,
    [Email] [nvarchar](100) NOT NULL UNIQUE,
    [Password] [nvarchar](255) NOT NULL,
    [Phone] [nvarchar](20) NULL,
    [UserType] [nvarchar](20) NOT NULL,
    [PaymentPreference] [nvarchar](50) NULL,
    [PassportNumber] [nvarchar](50) NULL,
    [Nationality] [nvarchar](50) NULL,
    [AdminLevel] [int] NULL DEFAULT(0)
)
GO

CREATE TABLE [dbo].[Vehicles](
    [VehicleID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [Type] [nvarchar](50) NOT NULL,
    [Status] [nvarchar](20) NOT NULL,
    [PricePerMinute] [float] NOT NULL,
    [StationID] [int] NULL FOREIGN KEY REFERENCES Stations(StationID)
)
GO

CREATE TABLE [dbo].[Rentals](
    [RentalID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [UserID] [int] NULL FOREIGN KEY REFERENCES Users(UserID),
    [VehicleID] [int] NULL FOREIGN KEY REFERENCES Vehicles(VehicleID),
    [StartStationID] [int] NULL FOREIGN KEY REFERENCES Stations(StationID),
    [ReturnStationID] [int] NULL FOREIGN KEY REFERENCES Stations(StationID),
    [StartTime] [datetime] NOT NULL,
    [EndTime] [datetime] NULL,
    [TotalFare] [float] NULL,
    [Discount] [float] NULL DEFAULT(0),
    [Status] [nvarchar](20) NULL DEFAULT('Active')
)
GO

CREATE TABLE [dbo].[Payments](
    [PaymentID] [int] IDENTITY(1,1) NOT NULL PRIMARY KEY,
    [RentalID] [int] NULL FOREIGN KEY REFERENCES Rentals(RentalID),
    [Amount] [float] NOT NULL,
    [PaymentMethod] [nvarchar](50) NOT NULL,
    [PaymentStatus] [nvarchar](20) NULL DEFAULT('Pending'),
    [PaymentDate] [datetime] NULL
)
GO

-- Thêm Stations
INSERT INTO Stations (StationName, Location, Capacity, CurrentInventory) VALUES
('Ben Thanh', 'Quan 1, Ho Chi Minh City', 10, 8),
('Thao Dien', 'Quan 2, Ho Chi Minh City', 10, 1),
('Bui Vien', 'Quan 1, Ho Chi Minh City', 10, 5),
('Tan Binh', 'Quan Tan Binh, Ho Chi Minh City', 10, 9),
('Phu My Hung', 'Quan 7, Ho Chi Minh City', 10, 2),
('Binh Thanh', 'Quan Binh Thanh, Ho Chi Minh City', 10, 7);

-- Thêm Vehicles
INSERT INTO Vehicles (Type, Status, PricePerMinute, StationID) VALUES
('Standard Bike', 'Available', 500, 1),
('Standard Bike', 'Available', 500, 1),
('E-Scooter', 'Available', 1500, 1),
('E-Scooter', 'Available', 1500, 2),
('Standard Bike', 'Available', 500, 2),
('E-Scooter', 'Maintenance', 1500, 3),
('Standard Bike', 'Available', 500, 3),
('E-Scooter', 'Available', 1500, 4),
('Standard Bike', 'Available', 500, 5),
('E-Scooter', 'Available', 1500, 6);

-- Thêm Users
INSERT INTO Users (FullName, Email, Password, Phone, UserType, AdminLevel) 
VALUES ('Admin SaigonRide', 'admin@saigonride.com', '123456', '0900000000', 'Admin', 1);

INSERT INTO Users (FullName, Email, Password, Phone, UserType, PaymentPreference) 
VALUES ('Nguyen Van A', 'local1@gmail.com', '123456', '0901234567', 'Local', 'MoMo');

INSERT INTO Users (FullName, Email, Password, Phone, UserType, PaymentPreference) 
VALUES ('Tran Thi B', 'local2@gmail.com', '123456', '0902345678', 'Local', 'VNPay');

INSERT INTO Users (FullName, Email, Password, Phone, UserType, PaymentPreference) 
VALUES ('Le Van C', 'local3@gmail.com', '123456', '0903456789', 'Local', 'Cash');

INSERT INTO Users (FullName, Email, Password, Phone, UserType, PassportNumber, Nationality) 
VALUES ('John Smith', 'tourist1@gmail.com', '123456', '0904567890', 'Tourist', 'AB123456', 'American');

INSERT INTO Users (FullName, Email, Password, Phone, UserType, PassportNumber, Nationality) 
VALUES ('Emily Johnson', 'tourist2@gmail.com', '123456', '0905678901', 'Tourist', 'CD789012', 'British');

-- Thêm Rentals mẫu (đã hoàn thành)
INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (2, 1, 1, 2, '2026-05-01 08:00:00', '2026-05-01 08:30:00', 15000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (3, 3, 1, 5, '2026-05-01 09:00:00', '2026-05-01 09:45:00', 63750, 11250, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (5, 4, 2, 3, '2026-05-02 10:00:00', '2026-05-02 10:20:00', 30000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (4, 7, 3, 4, '2026-05-02 14:00:00', '2026-05-02 14:40:00', 20000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (6, 8, 4, 5, '2026-05-03 07:00:00', '2026-05-03 07:50:00', 63750, 11250, 'Completed');

-- Thêm Payments mẫu
INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (1, 15000, 'MoMo', 'Completed', '2026-05-01 08:30:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (2, 63750, 'VNPay', 'Completed', '2026-05-01 09:45:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (3, 30000, 'Apple Pay', 'Completed', '2026-05-02 10:20:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (4, 20000, 'Cash', 'Completed', '2026-05-02 14:40:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (5, 63750, 'PayPal', 'Completed', '2026-05-03 07:50:00');

USE SaigonRideDB;

--  Rentals với UserID đúng
INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (5, 1, 1, 2, '2026-05-01 08:00:00', '2026-05-01 08:30:00', 15000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (6, 3, 1, 5, '2026-05-01 09:00:00', '2026-05-01 09:45:00', 63750, 11250, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (8, 4, 2, 3, '2026-05-02 10:00:00', '2026-05-02 10:20:00', 30000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (7, 7, 3, 4, '2026-05-02 14:00:00', '2026-05-02 14:40:00', 20000, 0, 'Completed');

INSERT INTO Rentals (UserID, VehicleID, StartStationID, ReturnStationID, StartTime, EndTime, TotalFare, Discount, Status) 
VALUES (9, 8, 4, 5, '2026-05-03 07:00:00', '2026-05-03 07:50:00', 63750, 11250, 'Completed');

--  payment với rentalID đúng
INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (6, 15000, 'MoMo', 'Completed', '2026-05-01 08:30:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (7, 63750, 'VNPay', 'Completed', '2026-05-01 09:45:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (8, 30000, 'Apple Pay', 'Completed', '2026-05-02 10:20:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (9, 20000, 'Cash', 'Completed', '2026-05-02 14:40:00');

INSERT INTO Payments (RentalID, Amount, PaymentMethod, PaymentStatus, PaymentDate) 
VALUES (10, 63750, 'PayPal', 'Completed', '2026-05-03 07:50:00');

/*DELETE FROM Payments;
DELETE FROM Rentals;
DELETE FROM Vehicles;
DELETE FROM Users;
DELETE FROM Stations;*/