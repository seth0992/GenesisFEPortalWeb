USE dbGenesisFEPortalWebApp


/************************************/
/* Datos Iniciales                 */
/************************************/
INSERT INTO Core.Tenants (Name, Identification, CommercialName, IsActive)
VALUES ('Sistema', '0000000000', 'Sistema', 1);

select * from core.Tenants

-- Roles

-- Insertar roles básicos del sistema
INSERT INTO Security.Roles (Name, Description, IsSystem, Permissions)
VALUES 
('SuperAdmin', 'Administrador del sistema con acceso total', 1, '*'),
('TenantAdmin', 'Administrador de empresa con acceso total a su tenant', 1, 'tenant.*'),
('User', 'Usuario básico con acceso limitado', 1, 'basic.*');

-- Usuario administrador inicial
INSERT INTO Security.Users (
    TenantId,
    Email,
    Username,
    PasswordHash,
    FirstName,
    LastName,
    RoleId,
    EmailConfirmed,
    IsActive
)
SELECT 
    t.ID,
    'admin@system.com',
    'admin',
    -- Password: Admin123!
    '$2a$11$XEyJPaiE7dT2u3UnS4MGOOyXeH4.bosU3k/nJ9.TgJBWoCJh7w6ge',
    'Admin',
    'System',
    r.ID,
    1,
    1
FROM Core.Tenants t
CROSS JOIN Security.Roles r
WHERE t.Name = 'Sistema'
AND r.Name = 'SuperAdmin';
GO

select * from Security.Users

-----------------------------------------------------------------------

-- Asegurarse de que todos los tenants tienen un secreto JWT
INSERT INTO [Security].[Secrets] (
    [TenantId], 
    [Key], 
    [Value], 
    [Description], 
    [IsActive], 
    [CreatedAt]
)
SELECT 
    t.ID as TenantId,
    'JWT_SECRET' as [Key],
    'MySuperSecret12k3jioasd8o12k3joiajsdij1l2kj3!!!!1k;lajskdjalkdj1sdlkj1ndas123qq' as [Value],
    'JWT signing key for tenant authentication' as [Description],
    1 as [IsActive],
    GETUTCDATE() as [CreatedAt]
FROM [Core].[Tenants] t
WHERE t.IsActive = 1
    AND NOT EXISTS (
        SELECT 1 
        FROM [Security].[Secrets] s 
        WHERE s.TenantId = t.ID 
        AND s.[Key] = 'JWT_SECRET'
    )
GO
