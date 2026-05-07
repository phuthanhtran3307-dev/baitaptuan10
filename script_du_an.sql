IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
CREATE TABLE [Flights] (
    [Id] int NOT NULL IDENTITY,
    [FromCity] nvarchar(max) NOT NULL,
    [ToCity] nvarchar(max) NOT NULL,
    [DepartureTime] datetime2 NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Flights] PRIMARY KEY ([Id])
);

CREATE TABLE [Users] (
    [Id] int NOT NULL IDENTITY,
    [FullName] nvarchar(max) NOT NULL,
    [Email] nvarchar(max) NOT NULL,
    [PhoneNumber] nvarchar(max) NOT NULL,
    [Password] nvarchar(max) NOT NULL,
    [Role] nvarchar(max) NOT NULL,
    CONSTRAINT [PK_Users] PRIMARY KEY ([Id])
);

CREATE TABLE [Bookings] (
    [Id] int NOT NULL IDENTITY,
    [UserId] int NOT NULL,
    [FlightId] int NULL,
    [BookingDate] datetime2 NOT NULL,
    [Status] nvarchar(50) NOT NULL,
    [TotalAmount] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_Bookings] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Bookings_Flights_FlightId] FOREIGN KEY ([FlightId]) REFERENCES [Flights] ([Id]),
    CONSTRAINT [FK_Bookings_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [Users] ([Id]) ON DELETE CASCADE
);

CREATE INDEX [IX_Bookings_FlightId] ON [Bookings] ([FlightId]);

CREATE INDEX [IX_Bookings_UserId] ON [Bookings] ([UserId]);

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260401185728_ReCreateDatabase', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260401190302_UpdateNullableFlightId', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260401204440_MakeFlightIdNullable', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260401204957_MakeFlightIdNullableV2', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260401210151_FixNullableFlightId', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
ALTER TABLE [Flights] ADD [AvailableSeats] int NOT NULL DEFAULT 0;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260415142716_AddAvailableSeatsToFlight', N'10.0.5');

COMMIT;
GO

BEGIN TRANSACTION;
INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260415143759_UpdateFlightSeats', N'10.0.5');

COMMIT;
GO

