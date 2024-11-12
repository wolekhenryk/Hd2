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
GO

CREATE TABLE [Adresy] (
    [Id] int NOT NULL IDENTITY,
    [Region] nvarchar(100) NOT NULL,
    [City] nvarchar(100) NOT NULL,
    [Street] nvarchar(100) NOT NULL,
    [HouseNumber] int NOT NULL,
    CONSTRAINT [PK_Adresy] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [TypyOrganow] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK_TypyOrganow] PRIMARY KEY ([Id])
);
GO

CREATE TABLE [Osoby] (
    [Pesel] nvarchar(11) NOT NULL,
    [FirstName] nvarchar(50) NOT NULL,
    [LastName] nvarchar(50) NOT NULL,
    [BirthDate] date NOT NULL,
    [AddressId] int NOT NULL,
    CONSTRAINT [PK_Osoby] PRIMARY KEY ([Pesel]),
    CONSTRAINT [FK_Osoby_Adresy_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Adresy] ([Id])
);
GO

CREATE TABLE [Szpitale] (
    [Id] int NOT NULL IDENTITY,
    [Name] nvarchar(100) NOT NULL,
    [AddressId] int NOT NULL,
    CONSTRAINT [PK_Szpitale] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Szpitale_Adresy_AddressId] FOREIGN KEY ([AddressId]) REFERENCES [Adresy] ([Id])
);
GO

CREATE TABLE [Dawcy] (
    [Pesel] nvarchar(11) NOT NULL,
    [BloodType] nvarchar(5) NOT NULL,
    [HealthStatus] nvarchar(1000) NOT NULL,
    [AliveDuringExtraction] bit NOT NULL,
    CONSTRAINT [PK_Dawcy] PRIMARY KEY ([Pesel]),
    CONSTRAINT [FK_Dawcy_Osoby_Pesel] FOREIGN KEY ([Pesel]) REFERENCES [Osoby] ([Pesel]) ON DELETE CASCADE
);
GO

CREATE TABLE [Pacjenci] (
    [Pesel] nvarchar(11) NOT NULL,
    [BloodType] nvarchar(5) NOT NULL,
    [HealthStatus] nvarchar(1000) NOT NULL,
    CONSTRAINT [PK_Pacjenci] PRIMARY KEY ([Pesel]),
    CONSTRAINT [FK_Pacjenci_Osoby_Pesel] FOREIGN KEY ([Pesel]) REFERENCES [Osoby] ([Pesel]) ON DELETE CASCADE
);
GO

CREATE TABLE [Lekarze] (
    [Pesel] nvarchar(11) NOT NULL,
    [Pwz] nvarchar(50) NOT NULL,
    [Specialization] nvarchar(50) NOT NULL,
    [HospitalId] int NOT NULL,
    CONSTRAINT [PK_Lekarze] PRIMARY KEY ([Pesel]),
    CONSTRAINT [FK_Lekarze_Osoby_Pesel] FOREIGN KEY ([Pesel]) REFERENCES [Osoby] ([Pesel]) ON DELETE CASCADE,
    CONSTRAINT [FK_Lekarze_Szpitale_HospitalId] FOREIGN KEY ([HospitalId]) REFERENCES [Szpitale] ([Id])
);
GO

CREATE TABLE [Narzady] (
    [Id] int NOT NULL IDENTITY,
    [HarvestDateTime] datetime2 NOT NULL,
    [StorageType] nvarchar(100) NOT NULL,
    [OrganTypeId] int NOT NULL,
    [DonorPesel] nvarchar(11) NOT NULL,
    CONSTRAINT [PK_Narzady] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Narzady_Dawcy_DonorPesel] FOREIGN KEY ([DonorPesel]) REFERENCES [Dawcy] ([Pesel]),
    CONSTRAINT [FK_Narzady_TypyOrganow_OrganTypeId] FOREIGN KEY ([OrganTypeId]) REFERENCES [TypyOrganow] ([Id])
);
GO

CREATE TABLE [Zabiegi] (
    [Id] int NOT NULL IDENTITY,
    [StartDateTime] datetime2 NOT NULL,
    [EndDateTime] datetime2 NOT NULL,
    [PatientPesel] nvarchar(11) NOT NULL,
    [HospitalId] int NOT NULL,
    [OrganId] int NOT NULL,
    CONSTRAINT [PK_Zabiegi] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Zabiegi_Narzady_OrganId] FOREIGN KEY ([OrganId]) REFERENCES [Narzady] ([Id]),
    CONSTRAINT [FK_Zabiegi_Pacjenci_PatientPesel] FOREIGN KEY ([PatientPesel]) REFERENCES [Pacjenci] ([Pesel]),
    CONSTRAINT [FK_Zabiegi_Szpitale_HospitalId] FOREIGN KEY ([HospitalId]) REFERENCES [Szpitale] ([Id])
);
GO

CREATE TABLE [DoctorProcedure] (
    [DoctorsPesel] nvarchar(11) NOT NULL,
    [ProceduresId] int NOT NULL,
    CONSTRAINT [PK_DoctorProcedure] PRIMARY KEY ([DoctorsPesel], [ProceduresId]),
    CONSTRAINT [FK_DoctorProcedure_Lekarze_DoctorsPesel] FOREIGN KEY ([DoctorsPesel]) REFERENCES [Lekarze] ([Pesel]) ON DELETE CASCADE,
    CONSTRAINT [FK_DoctorProcedure_Zabiegi_ProceduresId] FOREIGN KEY ([ProceduresId]) REFERENCES [Zabiegi] ([Id]) ON DELETE CASCADE
);
GO

CREATE TABLE [Komplikacje] (
    [Id] int NOT NULL IDENTITY,
    [Description] nvarchar(1000) NOT NULL,
    [Type] nvarchar(255) NOT NULL,
    [ProcedureId] int NOT NULL,
    CONSTRAINT [PK_Komplikacje] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_Komplikacje_Zabiegi_ProcedureId] FOREIGN KEY ([ProcedureId]) REFERENCES [Zabiegi] ([Id])
);
GO

CREATE INDEX [IX_DoctorProcedure_ProceduresId] ON [DoctorProcedure] ([ProceduresId]);
GO

CREATE INDEX [IX_Komplikacje_ProcedureId] ON [Komplikacje] ([ProcedureId]);
GO

CREATE INDEX [IX_Lekarze_HospitalId] ON [Lekarze] ([HospitalId]);
GO

CREATE UNIQUE INDEX [IX_Lekarze_Pwz] ON [Lekarze] ([Pwz]) WHERE [Pwz] IS NOT NULL;
GO

CREATE INDEX [IX_Narzady_DonorPesel] ON [Narzady] ([DonorPesel]);
GO

CREATE INDEX [IX_Narzady_OrganTypeId] ON [Narzady] ([OrganTypeId]);
GO

CREATE INDEX [IX_Osoby_AddressId] ON [Osoby] ([AddressId]);
GO

CREATE UNIQUE INDEX [IX_Szpitale_AddressId] ON [Szpitale] ([AddressId]);
GO

CREATE INDEX [IX_Zabiegi_HospitalId] ON [Zabiegi] ([HospitalId]);
GO

CREATE UNIQUE INDEX [IX_Zabiegi_OrganId] ON [Zabiegi] ([OrganId]);
GO

CREATE INDEX [IX_Zabiegi_PatientPesel] ON [Zabiegi] ([PatientPesel]);
GO

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20241112135159_initial', N'8.0.10');
GO

COMMIT;
GO

