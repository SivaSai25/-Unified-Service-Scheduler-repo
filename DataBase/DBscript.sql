/*
 Keyloop Unified Service Scheduler - complete demo database
 Database: KeyloopSchedulerDemo1234567
*/
IF DB_ID(N'KeyloopSchedulerDemo1234567') IS NULL
    CREATE DATABASE KeyloopSchedulerDemo1234567;
GO
USE KeyloopSchedulerDemo1234567;
GO


/* 1. Users */
CREATE TABLE dbo.Users
(
    UserId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    PhoneNumber NVARCHAR(30) NULL,
    CreatedAtUtc DATETIME2(0) NOT NULL
        CONSTRAINT DF_Users_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT UQ_Users_Email UNIQUE (Email)
);

/* 2. Vehicles */
CREATE TABLE dbo.Vehicles
(
    VehicleId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Vehicles PRIMARY KEY,
    UserId BIGINT NOT NULL,
    RegistrationNumber NVARCHAR(20) NOT NULL,
    Make NVARCHAR(60) NOT NULL,
    Model NVARCHAR(60) NOT NULL,
    VehicleType NVARCHAR(30) NULL,
    CONSTRAINT FK_Vehicles_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT UQ_Vehicles_RegistrationNumber UNIQUE (RegistrationNumber)
);

/* 3. Dealers */
CREATE TABLE dbo.DealerCentres
(
    DealerId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_DealerCentres PRIMARY KEY,
    DealerName NVARCHAR(150) NOT NULL,
    Address NVARCHAR(250) NOT NULL,
    City NVARCHAR(80) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_DealerCentres_IsActive DEFAULT (1)
);

/* 4. Services */
CREATE TABLE dbo.Services
(
    ServiceId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Services PRIMARY KEY,
    ServiceName NVARCHAR(100) NOT NULL,
    DurationMinutes INT NOT NULL,
    Description NVARCHAR(500) NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Services_IsActive DEFAULT (1),
    CONSTRAINT CK_Services_Duration CHECK (DurationMinutes > 0)
);

/* 5. Services offered by each dealer */
CREATE TABLE dbo.DealerServices
(
    DealerId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL,
    CONSTRAINT PK_DealerServices PRIMARY KEY (DealerId, ServiceId),
    CONSTRAINT FK_DealerServices_Dealer FOREIGN KEY (DealerId)
        REFERENCES dbo.DealerCentres(DealerId),
    CONSTRAINT FK_DealerServices_Service FOREIGN KEY (ServiceId)
        REFERENCES dbo.Services(ServiceId)
);

/* 6. Service bays */
CREATE TABLE dbo.ServiceBays
(
    BayId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_ServiceBays PRIMARY KEY,
    DealerId BIGINT NOT NULL,
    BayName NVARCHAR(80) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_ServiceBays_IsActive DEFAULT (1),
    CONSTRAINT FK_ServiceBays_Dealer FOREIGN KEY (DealerId)
        REFERENCES dbo.DealerCentres(DealerId),
    CONSTRAINT UQ_ServiceBays_Dealer_BayName UNIQUE (DealerId, BayName)
);

/* 7. Bay capabilities */
CREATE TABLE dbo.BayCapabilities
(
    BayId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL,
    CONSTRAINT PK_BayCapabilities PRIMARY KEY (BayId, ServiceId),
    CONSTRAINT FK_BayCapabilities_Bay FOREIGN KEY (BayId)
        REFERENCES dbo.ServiceBays(BayId),
    CONSTRAINT FK_BayCapabilities_Service FOREIGN KEY (ServiceId)
        REFERENCES dbo.Services(ServiceId)
);

/* 8. Actual bay availability windows */
CREATE TABLE dbo.BayAvailability
(
    BayAvailabilityId BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_BayAvailability PRIMARY KEY,
    BayId BIGINT NOT NULL,
    StartDateTimeUtc DATETIME2(0) NOT NULL,
    EndDateTimeUtc DATETIME2(0) NOT NULL,
    CONSTRAINT FK_BayAvailability_Bay FOREIGN KEY (BayId)
        REFERENCES dbo.ServiceBays(BayId),
    CONSTRAINT CK_BayAvailability_Time CHECK (EndDateTimeUtc > StartDateTimeUtc)
);

/* 9. Technicians */
CREATE TABLE dbo.Technicians
(
    TechnicianId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Technicians PRIMARY KEY,
    DealerId BIGINT NOT NULL,
    TechnicianName NVARCHAR(100) NOT NULL,
    IsActive BIT NOT NULL CONSTRAINT DF_Technicians_IsActive DEFAULT (1),
    CONSTRAINT FK_Technicians_Dealer FOREIGN KEY (DealerId)
        REFERENCES dbo.DealerCentres(DealerId)
);

/* 10. Technician skills */
CREATE TABLE dbo.TechnicianSkills
(
    TechnicianId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL,
    CONSTRAINT PK_TechnicianSkills PRIMARY KEY (TechnicianId, ServiceId),
    CONSTRAINT FK_TechnicianSkills_Technician FOREIGN KEY (TechnicianId)
        REFERENCES dbo.Technicians(TechnicianId),
    CONSTRAINT FK_TechnicianSkills_Service FOREIGN KEY (ServiceId)
        REFERENCES dbo.Services(ServiceId)
);

/* 11. Actual technician availability windows */
CREATE TABLE dbo.TechnicianAvailability
(
    TechnicianAvailabilityId BIGINT IDENTITY(1,1) NOT NULL
        CONSTRAINT PK_TechnicianAvailability PRIMARY KEY,
    TechnicianId BIGINT NOT NULL,
    StartDateTimeUtc DATETIME2(0) NOT NULL,
    EndDateTimeUtc DATETIME2(0) NOT NULL,
    CONSTRAINT FK_TechnicianAvailability_Technician FOREIGN KEY (TechnicianId)
        REFERENCES dbo.Technicians(TechnicianId),
    CONSTRAINT CK_TechnicianAvailability_Time CHECK (EndDateTimeUtc > StartDateTimeUtc)
);

/* 12. Appointments */
CREATE TABLE dbo.Appointments
(
    AppointmentId BIGINT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Appointments PRIMARY KEY,
    UserId BIGINT NOT NULL,
    VehicleId BIGINT NOT NULL,
    DealerId BIGINT NOT NULL,
    ServiceId BIGINT NOT NULL,
    BayId BIGINT NOT NULL,
    TechnicianId BIGINT NOT NULL,
    StartTimeUtc DATETIME2(0) NOT NULL,
    EndTimeUtc DATETIME2(0) NOT NULL,
    Status VARCHAR(20) NOT NULL CONSTRAINT DF_Appointments_Status DEFAULT ('Booked'),
    CreatedAtUtc DATETIME2(0) NOT NULL
        CONSTRAINT DF_Appointments_CreatedAtUtc DEFAULT SYSUTCDATETIME(),
    CONSTRAINT FK_Appointments_User FOREIGN KEY (UserId) REFERENCES dbo.Users(UserId),
    CONSTRAINT FK_Appointments_Vehicle FOREIGN KEY (VehicleId) REFERENCES dbo.Vehicles(VehicleId),
    CONSTRAINT FK_Appointments_Dealer FOREIGN KEY (DealerId) REFERENCES dbo.DealerCentres(DealerId),
    CONSTRAINT FK_Appointments_Service FOREIGN KEY (ServiceId) REFERENCES dbo.Services(ServiceId),
    CONSTRAINT FK_Appointments_Bay FOREIGN KEY (BayId) REFERENCES dbo.ServiceBays(BayId),
    CONSTRAINT FK_Appointments_Technician FOREIGN KEY (TechnicianId) REFERENCES dbo.Technicians(TechnicianId),
    CONSTRAINT CK_Appointments_Time CHECK (EndTimeUtc > StartTimeUtc),
    CONSTRAINT CK_Appointments_Status CHECK
        (Status IN ('Booked', 'Confirmed', 'InProgress', 'Completed', 'Cancelled'))
);
GO

CREATE INDEX IX_Appointments_Bay_StartTime
    ON dbo.Appointments (BayId, StartTimeUtc) INCLUDE (EndTimeUtc, Status);
CREATE INDEX IX_Appointments_Technician_StartTime
    ON dbo.Appointments (TechnicianId, StartTimeUtc) INCLUDE (EndTimeUtc, Status);
CREATE INDEX IX_DealerCentres_City_IsActive
    ON dbo.DealerCentres (City, IsActive);
CREATE INDEX IX_DealerCentres_Name
    ON dbo.DealerCentres (DealerName);
CREATE INDEX IX_Technicians_Dealer_IsActive
    ON dbo.Technicians (DealerId, IsActive);
CREATE INDEX IX_ServiceBays_Dealer_IsActive
    ON dbo.ServiceBays (DealerId, IsActive);
CREATE INDEX IX_BayAvailability_Bay_Start
    ON dbo.BayAvailability (BayId, StartDateTimeUtc) INCLUDE (EndDateTimeUtc);
CREATE INDEX IX_TechnicianAvailability_Technician_Start
    ON dbo.TechnicianAvailability (TechnicianId, StartDateTimeUtc) INCLUDE (EndDateTimeUtc);
GO


/* ========================= STORED PROCEDURES ========================= */

/* Dealer search with filtered TotalCount and safe dynamic sort choice */
CREATE OR ALTER PROCEDURE dbo.GetDealers
(
    @Limit       INT = 50,
    @OffSet      INT = 0,
    @DealerName  NVARCHAR(150) = NULL,
    @City        NVARCHAR(80) = NULL,
    @ServiceType NVARCHAR(100) = NULL,
    @SortOrder   NVARCHAR(10) = N'ASC'
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Limit <= 0 SET @Limit = 50;
    IF @OffSet < 0 SET @OffSet = 0;

    SET @SortOrder = UPPER(LTRIM(RTRIM(ISNULL(@SortOrder, N'ASC'))));
    IF @SortOrder NOT IN (N'ASC', N'DESC')
        THROW 50100, 'SortOrder must be ASC or DESC.', 1;

    DECLARE @TotalCount INT;

	SELECT @TotalCount = COUNT(*)
	FROM dbo.DealerCentres as d
    WHERE
        (NULLIF(LTRIM(RTRIM(@DealerName)), N'') IS NULL
         OR d.DealerName LIKE N'%' + LTRIM(RTRIM(@DealerName)) + N'%')
        AND
        (NULLIF(LTRIM(RTRIM(@City)), N'') IS NULL
         OR d.City LIKE N'%' + LTRIM(RTRIM(@City)) + N'%')
        AND
        (NULLIF(LTRIM(RTRIM(@ServiceType)), N'') IS NULL
         OR EXISTS
         (
             SELECT 1
             FROM dbo.DealerServices ds
             INNER JOIN dbo.Services s ON s.ServiceId = ds.ServiceId
             WHERE ds.DealerId = d.DealerId
               AND s.IsActive = 1
               AND s.ServiceName LIKE N'%' + LTRIM(RTRIM(@ServiceType)) + N'%'
         ));

    SELECT
        d.DealerId, d.DealerName, d.Address, d.City, d.IsActive
    FROM dbo.DealerCentres AS d
    WHERE
        (NULLIF(LTRIM(RTRIM(@DealerName)), N'') IS NULL
         OR d.DealerName LIKE N'%' + LTRIM(RTRIM(@DealerName)) + N'%')
        AND
        (NULLIF(LTRIM(RTRIM(@City)), N'') IS NULL
         OR d.City LIKE N'%' + LTRIM(RTRIM(@City)) + N'%')
        AND
        (NULLIF(LTRIM(RTRIM(@ServiceType)), N'') IS NULL
         OR EXISTS
         (
             SELECT 1
             FROM dbo.DealerServices ds
             INNER JOIN dbo.Services s ON s.ServiceId = ds.ServiceId
             WHERE ds.DealerId = d.DealerId
               AND s.IsActive = 1
               AND s.ServiceName LIKE N'%' + LTRIM(RTRIM(@ServiceType)) + N'%'
         ))
    ORDER BY
        CASE WHEN @SortOrder = N'ASC' THEN d.DealerId END ASC,
        CASE WHEN @SortOrder = N'DESC' THEN d.DealerId END DESC
    OFFSET @OffSet ROWS FETCH NEXT @Limit ROWS ONLY;

	SELECT @TotalCount AS TotalCount;
END;
GO


CREATE OR ALTER PROCEDURE dbo.GetAvailableServiceSlots
(
    @DealerId    INT,
    @ServiceName NVARCHAR(100) = NULL,
    @StartTime   DATETIME2(0) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ServiceId INT = NULL;
    DECLARE @DurationMinutes INT = NULL;
    DECLARE @EndTime DATETIME2(0) = NULL;

    -- 1. Validate the dealer
    IF NOT EXISTS
    (
        SELECT 1
        FROM dbo.DealerCentres
        WHERE DealerId = @DealerId
          AND IsActive = 1
    )
    BEGIN
        THROW 50000, 'Dealer not found or inactive.', 1;
    END;

    -- 2. A start time requires a service
    IF @StartTime IS NOT NULL
       AND NULLIF(LTRIM(RTRIM(@ServiceName)), '') IS NULL
    BEGIN
        THROW 50001,
              'serviceName is required when startTime is provided.',
              1;
    END;

    -- 3. Find the requested active service
    IF NULLIF(LTRIM(RTRIM(@ServiceName)), '') IS NOT NULL
    BEGIN
        SELECT TOP (1)
            @ServiceId = ServiceId,
            @DurationMinutes = DurationMinutes
        FROM dbo.Services
        WHERE ServiceName LIKE '%' + LTRIM(RTRIM(@ServiceName)) + '%'
          AND IsActive = 1
        ORDER BY
            CASE
                WHEN ServiceName = LTRIM(RTRIM(@ServiceName))
                THEN 0
                ELSE 1
            END,
            ServiceName;
        
        IF @ServiceId IS NULL
        BEGIN
            THROW 50002, 'Service not found or inactive.', 1;
        END;
    END;

    -- 4. Calculate the appointment end time
    IF @StartTime IS NOT NULL
    BEGIN
        SET @EndTime =
            DATEADD(MINUTE, @DurationMinutes, @StartTime);
    END;

    -- 5. Return resources with separate availability flags
    SELECT DISTINCT
        dc.DealerId,
        dc.DealerName,
        s.ServiceName,
        @StartTime AS StartTimeUtc,
        @EndTime AS EndTimeUtc,

        sb.BayId,
        sb.BayName,

        tc.TechnicianId,
        tc.TechnicianName,

        -- Is this bay available for the requested time?
        CAST(
            CASE
                WHEN @StartTime IS NULL THEN NULL

                WHEN EXISTS
                (
                    SELECT 1
                    FROM dbo.BayAvailability ba
                    WHERE ba.BayId = sb.BayId
                      AND ba.StartDateTimeUtc <= @StartTime
                      AND ba.EndDateTimeUtc >= @EndTime
                )
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.Appointments a
                    WHERE a.BayId = sb.BayId
                      AND a.Status <> 'Cancelled'
                      AND a.StartTimeUtc < @EndTime
                      AND a.EndTimeUtc > @StartTime
                )
                THEN 1

                ELSE 0
            END
        AS BIT) AS IsBayAvailable,

        -- Is this technician available for the requested time?
        CAST(
            CASE
                WHEN @StartTime IS NULL THEN NULL

                WHEN EXISTS
                (
                    SELECT 1
                    FROM dbo.TechnicianAvailability ta
                    WHERE ta.TechnicianId = tc.TechnicianId
                      AND ta.StartDateTimeUtc <= @StartTime
                      AND ta.EndDateTimeUtc >= @EndTime
                )
                AND NOT EXISTS
                (
                    SELECT 1
                    FROM dbo.Appointments a
                    WHERE a.TechnicianId = tc.TechnicianId
                      AND a.Status <> 'Cancelled'
                      AND a.StartTimeUtc < @EndTime
                      AND a.EndTimeUtc > @StartTime
                )
                THEN 1

                ELSE 0
            END
        AS BIT) AS IsTechnicianAvailable

    FROM dbo.DealerCentres dc

    INNER JOIN dbo.ServiceBays sb
        ON sb.DealerId = dc.DealerId
       AND sb.IsActive = 1

    LEFT JOIN dbo.BayCapabilities bc
        ON bc.BayId = sb.BayId

    LEFT JOIN dbo.Services s
        ON s.ServiceId = bc.ServiceId
       AND s.IsActive = 1

    LEFT JOIN dbo.Technicians tc
        ON tc.DealerId = dc.DealerId
       AND tc.IsActive = 1

    LEFT JOIN dbo.TechnicianSkills ts
        ON ts.TechnicianId = tc.TechnicianId
       AND ts.ServiceId = s.ServiceId

    WHERE dc.DealerId = @DealerId
      AND dc.IsActive = 1

      -- If a service is selected, require the bay
      -- and technician to support that service.
      AND
      (
          @ServiceId IS NULL
          OR
          (
              bc.ServiceId = @ServiceId
              AND ts.ServiceId = @ServiceId
          )
      )

    ORDER BY sb.BayId, tc.TechnicianId;
END;
GO

/* Transactional booking procedure */
CREATE OR ALTER PROCEDURE dbo.BookAppointment
(
    @UserId       BIGINT,
    @VehicleId    BIGINT,
    @DealerId     BIGINT,
    @ServiceName  NVARCHAR(100),
    @BayId        BIGINT,
    @TechnicianId BIGINT,
    @StartTimeUtc DATETIME2(0)
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @ServiceId BIGINT = NULL;
    DECLARE @DurationMinutes INT = NULL;
    DECLARE @EndTimeUtc DATETIME2(0);
    DECLARE @AppointmentId BIGINT;
    DECLARE @ServiceSearch NVARCHAR(100) = NULLIF(LTRIM(RTRIM(@ServiceName)), N'');

    BEGIN TRY
        BEGIN TRANSACTION;

        IF NOT EXISTS (SELECT 1 FROM dbo.Users WHERE UserId = @UserId)
            THROW 50300, 'User not found.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.Vehicles
            WHERE VehicleId = @VehicleId AND UserId = @UserId
        )
            THROW 50301, 'Vehicle not found for the specified user.', 1;

        SELECT @ServiceId = ServiceId, @DurationMinutes = DurationMinutes
        FROM dbo.Services
        WHERE IsActive = 1 AND ServiceName like '%'+ @ServiceSearch +'%';

        IF @ServiceId IS NULL
            THROW 50302, 'Service not found or inactive. Use the exact service name.', 1;

        SET @EndTimeUtc = DATEADD(MINUTE, @DurationMinutes, @StartTimeUtc);

        IF @EndTimeUtc <= @StartTimeUtc
            THROW 50303, 'Invalid service duration or start time.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.DealerCentres
            WHERE DealerId = @DealerId AND IsActive = 1
        )
            THROW 50304, 'Dealer not found or inactive.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.DealerServices
            WHERE DealerId = @DealerId AND ServiceId = @ServiceId
        )
            THROW 50305, 'Dealer does not offer this service.', 1;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.ServiceBays sb
            INNER JOIN dbo.BayCapabilities bc ON bc.BayId = sb.BayId
            WHERE sb.BayId = @BayId AND sb.DealerId = @DealerId
              AND sb.IsActive = 1 AND bc.ServiceId = @ServiceId
        )
            THROW 50306, 'Bay is inactive or does not support this service.', 1;

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Technicians tc
            INNER JOIN dbo.TechnicianSkills ts ON ts.TechnicianId = tc.TechnicianId
            WHERE tc.TechnicianId = @TechnicianId AND tc.DealerId = @DealerId
              AND tc.IsActive = 1 AND ts.ServiceId = @ServiceId
        )
            THROW 50307, 'Technician is inactive or not qualified for this service.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.BayAvailability
            WHERE BayId = @BayId
              AND StartDateTimeUtc <= @StartTimeUtc
              AND EndDateTimeUtc >= @EndTimeUtc
        )
            THROW 50308, 'Bay is not available during the requested time.', 1;

        IF NOT EXISTS
        (
            SELECT 1 FROM dbo.TechnicianAvailability
            WHERE TechnicianId = @TechnicianId
              AND StartDateTimeUtc <= @StartTimeUtc
              AND EndDateTimeUtc >= @EndTimeUtc
        )
            THROW 50309, 'Technician is not available during the requested time.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Appointments WITH (UPDLOCK, HOLDLOCK)
            WHERE BayId = @BayId AND Status <> 'Cancelled'
              AND StartTimeUtc < @EndTimeUtc AND EndTimeUtc > @StartTimeUtc
        )
            THROW 50310, 'Bay already has an overlapping appointment.', 1;

        IF EXISTS
        (
            SELECT 1 FROM dbo.Appointments WITH (UPDLOCK, HOLDLOCK)
            WHERE TechnicianId = @TechnicianId AND Status <> 'Cancelled'
              AND StartTimeUtc < @EndTimeUtc AND EndTimeUtc > @StartTimeUtc
        )
            THROW 50311, 'Technician already has an overlapping appointment.', 1;

        INSERT dbo.Appointments
            (UserId, VehicleId, DealerId, ServiceId, BayId, TechnicianId,
             StartTimeUtc, EndTimeUtc, Status)
        VALUES
            (@UserId, @VehicleId, @DealerId, @ServiceId, @BayId, @TechnicianId,
             @StartTimeUtc, @EndTimeUtc, 'Booked');

        SET @AppointmentId = CONVERT(BIGINT, SCOPE_IDENTITY());

        COMMIT TRANSACTION;

        SELECT @AppointmentId AS AppointmentId;
    END TRY
    BEGIN CATCH
        IF XACT_STATE() <> 0 ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END;
GO

/* Seed data*/
select * from Users
INSERT dbo.Users (FullName, Email, PhoneNumber)
VALUES
(N'Aarav Sharma', N'aarav.sharma@example.com', N'+91-9000000001'),
(N'Priya Rao', N'priya.rao@example.com', N'+91-9000000002'),
(N'Rohan Mehta', N'rohan.mehta@example.com', N'+91-9000000003');

INSERT dbo.Vehicles (UserId, RegistrationNumber, Make, Model, VehicleType)
VALUES
(1, N'KA01AB1234', N'Maruti Suzuki', N'Baleno', N'Hatchback'),
(2, N'KA03CD5678', N'Hyundai', N'Creta', N'SUV'),
(3, N'KA05EF9012', N'Tata', N'Nexon', N'SUV');

/* Four dealers: three active, one inactive */
INSERT dbo.DealerCentres
    (DealerName, Address, City,  IsActive)
VALUES
(N'Keyloop Demo Motors - Central', N'100 Main Road', N'Bengaluru', 1),
(N'Keyloop Demo Motors - South', N'25 Service Road', N'Bengaluru', 1),
(N'Keyloop Demo Motors - Mysuru', N'8 Ring Road', N'Mysuru', 1),
(N'Keyloop Demo Motors - Inactive', N'50 Old Market Road', N'Mysuru', 0);

/* Three active services and one inactive service */
INSERT dbo.Services (ServiceName, DurationMinutes, Description, IsActive)
VALUES
(N'Basic Service', 60, N'Routine inspection and basic maintenance', 1),
(N'Full Service', 120, N'Comprehensive vehicle service', 1),
(N'Brake Inspection', 30, N'Brake system inspection', 1),
(N'Legacy Service', 45, N'Inactive service for validation testing', 0);

/* Dealer offerings */
INSERT dbo.DealerServices (DealerId, ServiceId)
VALUES
(1,1),(1,2),(1,3),
(2,1),(2,3),
(3,1),(3,2),
(4,1);

/* Bays: Bay 2 will be inactive to test filtering */
INSERT dbo.ServiceBays (DealerId, BayName, IsActive)
VALUES
(1,N'Bay A',1),
(1,N'Bay B',1),
(1,N'Bay C - Inactive',0),
(2,N'Bay A',1),
(3,N'Bay A',1),
(4,N'Bay A',1);

/* Bay capabilities */
INSERT dbo.BayCapabilities (BayId, ServiceId)
VALUES
(1,1),(1,2),(1,3),
(2,1),(2,3),
(3,1),(3,2),
(4,1),(4,3),
(5,1),(5,2),
(6,1);

/* Technicians: technician 3 inactive */
INSERT dbo.Technicians (DealerId, TechnicianName, IsActive)
VALUES
(1,N'Alex Morgan',1),
(1,N'Jamie Patel',1),
(1,N'Inactive Technician',0),
(2,N'Taylor Singh',1),
(3,N'Morgan Lewis',1),
(4,N'Inactive Dealer Technician',1);

/* Qualifications */
INSERT dbo.TechnicianSkills (TechnicianId, ServiceId)
VALUES
(1,1),(1,2),(1,3),
(2,1),(2,3),
(3,1),(3,2),
(4,1),(4,3),
(5,1),(5,2),
(6,1);


INSERT dbo.BayAvailability (BayId, StartDateTimeUtc, EndDateTimeUtc)
VALUES
(1,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(2,'2026-11-02T13:00:00','2026-11-02T17:00:00'),
(3,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(4,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(5,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(6,'2026-11-02T09:00:00','2026-11-02T17:00:00');

INSERT dbo.TechnicianAvailability (TechnicianId, StartDateTimeUtc, EndDateTimeUtc)
VALUES
(1,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(2,'2026-11-02T13:00:00','2026-11-02T17:00:00'),
(3,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(4,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(5,'2026-11-02T09:00:00','2026-11-02T17:00:00'),
(6,'2026-11-02T09:00:00','2026-11-02T17:00:00');
GO

/* Seed two appointments: one booked conflict, one cancelled (does not block). */
INSERT dbo.Appointments
    (UserId, VehicleId, DealerId, ServiceId, BayId, TechnicianId,
     StartTimeUtc, EndTimeUtc, Status)
VALUES
(1,1,1,1,1,1,'2026-11-02T10:00:00','2026-11-02T11:00:00','Booked'),
(2,2,1,3,2,2,'2026-11-02T14:00:00','2026-11-02T14:30:00','Cancelled');
GO