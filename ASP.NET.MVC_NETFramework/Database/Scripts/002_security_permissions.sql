/*
Permisos mínimos para que el login de Windows pueda operar el módulo de accesos.
Ajusta @LoginName al usuario o grupo AD correspondiente.
*/
USE [AdventureWorks2022];
GO

DECLARE @LoginName SYSNAME = N'CALEL\josep';
DECLARE @CreateUserSql NVARCHAR(MAX);

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = @LoginName)
BEGIN
    SET @CreateUserSql = N'CREATE USER [' + REPLACE(@LoginName, ']', ']]') + N'] FOR LOGIN [' + REPLACE(@LoginName, ']', ']]') + N']';
    EXEC (@CreateUserSql);
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'AccessAppRole')
BEGIN
    CREATE ROLE [AccessAppRole] AUTHORIZATION [dbo];
END
GO

GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::[Security] TO [AccessAppRole];
GO

ALTER ROLE [AccessAppRole] ADD MEMBER [CALEL\josep];
GO
