/*
Fase 1 - Estructura para control de accesos y autorización interna.
Ejecutar sobre AdventureWorks2022.
*/

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Security')
    EXEC('CREATE SCHEMA [Security]');
GO

IF OBJECT_ID('Security.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE Security.Roles
    (
        RoleId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Roles PRIMARY KEY,
        RoleCode NVARCHAR(50) NOT NULL CONSTRAINT UQ_Roles_RoleCode UNIQUE,
        RoleName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Roles_IsActive DEFAULT(1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Roles_CreatedAt DEFAULT(SYSDATETIME()),
        UpdatedAt DATETIME2(0) NULL
    );
END
GO

IF OBJECT_ID('Security.Users', 'U') IS NULL
BEGIN
    CREATE TABLE Security.Users
    (
        UserId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
        UserName NVARCHAR(150) NOT NULL CONSTRAINT UQ_Users_UserName UNIQUE, -- DOMAIN\usuario
        DisplayName NVARCHAR(150) NULL,
        Email NVARCHAR(150) NULL,
        IsActive BIT NOT NULL CONSTRAINT DF_Users_IsActive DEFAULT(1),
        CreatedAt DATETIME2(0) NOT NULL CONSTRAINT DF_Users_CreatedAt DEFAULT(SYSDATETIME()),
        UpdatedAt DATETIME2(0) NULL
    );
END
GO

IF OBJECT_ID('Security.UserRoles', 'U') IS NULL
BEGIN
    CREATE TABLE Security.UserRoles
    (
        UserId INT NOT NULL,
        RoleId INT NOT NULL,
        AssignedAt DATETIME2(0) NOT NULL CONSTRAINT DF_UserRoles_AssignedAt DEFAULT(SYSDATETIME()),
        AssignedBy NVARCHAR(150) NULL,
        CONSTRAINT PK_UserRoles PRIMARY KEY (UserId, RoleId),
        CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Security.Users(UserId),
        CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Security.Roles(RoleId)
    );
END
GO

IF NOT EXISTS (SELECT 1 FROM Security.Roles WHERE RoleCode = 'CrudReader')
    INSERT INTO Security.Roles (RoleCode, RoleName) VALUES ('CrudReader', 'Lectura de CRUD');
IF NOT EXISTS (SELECT 1 FROM Security.Roles WHERE RoleCode = 'CrudWriter')
    INSERT INTO Security.Roles (RoleCode, RoleName) VALUES ('CrudWriter', 'Edición de CRUD');
IF NOT EXISTS (SELECT 1 FROM Security.Roles WHERE RoleCode = 'CrudAdmin')
    INSERT INTO Security.Roles (RoleCode, RoleName) VALUES ('CrudAdmin', 'Administrador de CRUD');
IF NOT EXISTS (SELECT 1 FROM Security.Roles WHERE RoleCode = 'DashboardViewer')
    INSERT INTO Security.Roles (RoleCode, RoleName) VALUES ('DashboardViewer', 'Visualización Dashboard');
GO

-- Usuario de ejemplo (ajustar DOMAIN\usuario según entorno)
IF NOT EXISTS (SELECT 1 FROM Security.Users WHERE UserName = 'DOMAIN\\usuario')
    INSERT INTO Security.Users (UserName, DisplayName, Email) VALUES ('DOMAIN\\usuario', 'Usuario Admin', 'usuario@empresa.com');
GO

-- Asignación de ejemplo: DOMAIN\usuario -> CrudAdmin
DECLARE @UserId INT = (SELECT UserId FROM Security.Users WHERE UserName = 'DOMAIN\\usuario');
DECLARE @RoleId INT = (SELECT RoleId FROM Security.Roles WHERE RoleCode = 'CrudAdmin');

IF @UserId IS NOT NULL AND @RoleId IS NOT NULL
AND NOT EXISTS (SELECT 1 FROM Security.UserRoles WHERE UserId = @UserId AND RoleId = @RoleId)
BEGIN
    INSERT INTO Security.UserRoles (UserId, RoleId, AssignedBy) VALUES (@UserId, @RoleId, SYSTEM_USER);
END
GO
