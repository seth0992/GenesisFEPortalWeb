-- Crear la base de datos
CREATE DATABASE dbFEGenesisAppWeb;
GO

USE dbFEGenesisAppWeb;
GO

-- Creación de esquemas
CREATE SCHEMA Security;    -- Autenticación y autorización
GO
CREATE SCHEMA Core;       -- Tablas principales del sistema
GO
CREATE SCHEMA Catalog;    -- Catálogos y listas de valores
GO
CREATE SCHEMA Billing;    -- Facturación electrónica
GO
CREATE SCHEMA Audit;      -- Auditoría y logs
GO
CREATE SCHEMA Reports;    -- Reportes y estadísticas
GO
CREATE SCHEMA Notifications; -- Sistema de notificaciones
GO

/*************************************
* TABLAS DEL CORE                    *
*************************************/
CREATE TABLE Core.Tenants (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(255) NOT NULL,
    Identification NVARCHAR(50) NOT NULL UNIQUE,
    CommercialName NVARCHAR(255),
    Email NVARCHAR(255) NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(500),
    Province NVARCHAR(50),
    Canton NVARCHAR(50),
    District NVARCHAR(50),
    Logo VARBINARY(MAX),
    -- Configuración de Facturación
    InvoicePrefix NVARCHAR(10),
    DefaultCurrency NVARCHAR(3) DEFAULT 'CRC',
    DefaultPaymentTerm INT DEFAULT 0,
    -- Configuración de Correos
    SmtpServer NVARCHAR(100),
    SmtpPort INT,
    SmtpUsername NVARCHAR(100),
    SmtpPassword NVARCHAR(MAX),
    SmtpEnableSsl BIT DEFAULT 1,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

CREATE TABLE Core.DigitalCertificates (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    CertificateData VARBINARY(MAX) NOT NULL,
    Password NVARCHAR(MAX),
    ExpirationDate DATETIME NOT NULL,
    NotificationsSent INT DEFAULT 0,
    LastNotificationDate DATETIME,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID)
);

CREATE TABLE Core.ApiConfiguration (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    ApiKey NVARCHAR(MAX) NOT NULL,
    ApiSecret NVARCHAR(MAX) NOT NULL,
    Environment NVARCHAR(20) NOT NULL, -- Producción/Pruebas
    EndpointUrl NVARCHAR(255),
    TimeoutSeconds INT DEFAULT 30,
    MaxRetries INT DEFAULT 3,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID)
);

/*************************************
* TABLAS DE SEGURIDAD               *
*************************************/
CREATE TABLE Security.Roles (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    Permissions NVARCHAR(MAX),
    IsSystem BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

CREATE TABLE Security.Users (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    Email NVARCHAR(255) NOT NULL,
    Username NVARCHAR(100) NOT NULL,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    RoleId BIGINT NOT NULL,
    PhoneNumber NVARCHAR(20),
    EmailConfirmed BIT DEFAULT 0,
    TwoFactorEnabled BIT DEFAULT 0,
    LockoutEnd DATETIME,
    AccessFailedCount INT DEFAULT 0,
    LastLoginDate DATETIME,
    LastPasswordChangeDate DATETIME,
    SecurityStamp NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    LastSuccessfulLogin DATETIME NULL,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (RoleId) REFERENCES Security.Roles(ID)
);


CREATE TABLE Security.RefreshTokens (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NOT NULL,
    Token NVARCHAR(MAX) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    CreatedAt DATETIME DEFAULT GETDATE(),
	UpdatedAt DATETIME,
    CreatedByIp NVARCHAR(50),
    RevokedAt DATETIME,
    RevokedByIp NVARCHAR(50),
    ReplacedByToken NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Security.Users(ID)
);


CREATE TABLE Security.Secrets (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    [Key] NVARCHAR(100) NOT NULL,
    [Value] NVARCHAR(MAX) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    CONSTRAINT UQ_Secrets_TenantKey UNIQUE (TenantId, [Key])
);


CREATE TABLE Security.PasswordResetTokens (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    UserId BIGINT NOT NULL,
    Token NVARCHAR(100) NOT NULL,
    ExpiryDate DATETIME NOT NULL,
    IsUsed BIT DEFAULT 0,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    IsActive BIT DEFAULT 1,
    FOREIGN KEY (UserId) REFERENCES Security.Users(ID)
);

CREATE INDEX IX_PasswordResetTokens_Token ON Security.PasswordResetTokens(Token);
CREATE INDEX IX_PasswordResetTokens_UserId ON Security.PasswordResetTokens(UserId);
/*************************************
* TABLAS DE CATÁLOGOS               *
*************************************/
CREATE TABLE Catalog.IdentificationTypes (
    ID NVARCHAR(3) PRIMARY KEY,
    Description NVARCHAR(255) NOT NULL,
    IsActive BIT DEFAULT 1
);

CREATE TABLE Catalog.TaxTypes (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Rate DECIMAL(5,2) NOT NULL,
    IsExemption BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

CREATE TABLE Catalog.PaymentMethods (
    ID NVARCHAR(2) PRIMARY KEY,
    Description NVARCHAR(100) NOT NULL,
    IsActive BIT DEFAULT 1
);

CREATE TABLE Catalog.DocumentTypes (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    Code NVARCHAR(10) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME
);

CREATE TABLE Catalog.Products (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    Code NVARCHAR(50) NOT NULL,
    CabysCode NVARCHAR(20) NOT NULL,
    Name NVARCHAR(255) NOT NULL,
    Description NVARCHAR(500),
    UnitPrice DECIMAL(18,2) NOT NULL,
    UnitOfMeasure NVARCHAR(50) NOT NULL,
    TaxTypeId BIGINT NOT NULL,
    IsService BIT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (TaxTypeId) REFERENCES Catalog.TaxTypes(ID),
    CONSTRAINT UQ_ProductCode UNIQUE (TenantId, Code)
);

CREATE TABLE Catalog.Region (
    RegionID INT IDENTITY(1,1) PRIMARY KEY,
    RegionName VARCHAR(150) NOT NULL,
    CONSTRAINT UQ_NombreRegion UNIQUE (RegionName)
);

CREATE TABLE Catalog.Provinces(
    ProvinceID INT PRIMARY KEY, -- Llave primaria con autoincremento
    ProvinceName NVARCHAR(255) NOT NULL UNIQUE -- Nombre de la provincia
);

CREATE TABLE Catalog.Cantons(
    CantonID INT PRIMARY KEY, -- Llave primaria con autoincremento
    CantonName NVARCHAR(255) NOT NULL, -- Nombre del cantón
    ProvinceId INT , -- Provincia
    FOREIGN KEY (ProvinceId) REFERENCES Catalog.Provinces(ProvinceID),
	CONSTRAINT UQ_Canton_Provincia UNIQUE (ProvinceId, CantonName)
);

CREATE TABLE Catalog.Districts(
    DistrictID INT PRIMARY KEY, -- Llave primaria con autoincremento
    DistrictName NVARCHAR(255) NOT NULL, -- Nombre del distrito
    CantonId INT , -- Cantón
	RegionID INT 
    FOREIGN KEY (CantonId) REFERENCES Catalog.Cantons(CantonID),
	FOREIGN KEY (RegionID) REFERENCES  Catalog.Region(RegionID)
);


/*************************************
* TABLAS DE FACTURACIÓN             *
*************************************/
CREATE TABLE Billing.Customers (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    CustomerName NVARCHAR(255) NOT NULL,
    CommercialName NVARCHAR(255),
    IdentificationTypeId NVARCHAR(3) NOT NULL,
    Identification NVARCHAR(50) NOT NULL,
    Email NVARCHAR(255),
    PhoneCode NVARCHAR(10),
    Phone NVARCHAR(20),
    Address NVARCHAR(500),
    Neighborhood NVARCHAR(250),
    DistrictID INT,
    PaymentTerm INT DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (IdentificationTypeId) REFERENCES Catalog.IdentificationTypes(ID),
    FOREIGN KEY (DistrictID) REFERENCES Catalog.Districts(DistrictID)
);


CREATE TABLE Billing.CustomerExonerations (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    CustomerId BIGINT NOT NULL,
    DocumentNumber NVARCHAR(50) NOT NULL,
    DocumentType NVARCHAR(50) NOT NULL,
    TaxTypeId BIGINT NOT NULL,
    ExonerationPercentage DECIMAL(5,2) NOT NULL,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (CustomerId) REFERENCES Billing.Customers(ID),
    FOREIGN KEY (TaxTypeId) REFERENCES Catalog.TaxTypes(ID)
);

CREATE TABLE Billing.Invoices (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    CustomerId BIGINT NOT NULL,
    DocumentTypeId BIGINT NOT NULL,
    InvoiceNumber NVARCHAR(50) NOT NULL,
    ConsecutiveNumber NVARCHAR(50) NOT NULL,
    KeyDocument NVARCHAR(100) NOT NULL,
    ReferenceDocument NVARCHAR(50),
    IssueDate DATETIME NOT NULL,
    DueDate DATETIME,
    Currency NVARCHAR(3) NOT NULL DEFAULT 'CRC',
    ExchangeRate DECIMAL(18,5) DEFAULT 1,
    PaymentTerm INT DEFAULT 0,
    PaymentMethod NVARCHAR(2) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    Total DECIMAL(18,2) NOT NULL,
    Notes NVARCHAR(MAX),
    XmlDocument XML,
    PdfDocument VARBINARY(MAX),
    IsVoided BIT DEFAULT 0,
    VoidReason NVARCHAR(MAX),
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (CustomerId) REFERENCES Billing.Customers(ID),
    FOREIGN KEY (DocumentTypeId) REFERENCES Catalog.DocumentTypes(ID)
);

CREATE TABLE Billing.InvoiceLines (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId BIGINT NOT NULL,
    ProductId BIGINT NOT NULL,
    LineNumber INT NOT NULL,
    Description NVARCHAR(255) NOT NULL,
    Quantity DECIMAL(18,2) NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    DiscountPercentage DECIMAL(5,2) DEFAULT 0,
    DiscountAmount DECIMAL(18,2) DEFAULT 0,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxTypeId BIGINT NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    ExonerationId BIGINT,
    Total DECIMAL(18,2) NOT NULL,
    FOREIGN KEY (InvoiceId) REFERENCES Billing.Invoices(ID),
    FOREIGN KEY (ProductId) REFERENCES Catalog.Products(ID),
    FOREIGN KEY (TaxTypeId) REFERENCES Catalog.TaxTypes(ID),
    FOREIGN KEY (ExonerationId) REFERENCES Billing.CustomerExonerations(ID)
);

CREATE TABLE Billing.InvoiceStatus (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    InvoiceId BIGINT NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    StatusDetail NVARCHAR(MAX),
    HaciendaResponse XML,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (InvoiceId) REFERENCES Billing.Invoices(ID)
);

/*************************************
* TABLAS DE REPORTES                *
*************************************/
CREATE TABLE Reports.ReportConfigurations (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- SALES, INVOICES, CUSTOMERS
    Configuration NVARCHAR(MAX), -- JSON con configuración específica
    Schedule NVARCHAR(50), -- Programación de generación automática
    Format NVARCHAR(20), -- PDF, CSV, EXCEL
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID)
);

CREATE TABLE Reports.GeneratedReports (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    ConfigurationId BIGINT NOT NULL,
    GeneratedBy BIGINT NOT NULL,
    FilePath NVARCHAR(500),
    Status NVARCHAR(20),
    GeneratedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (ConfigurationId) REFERENCES Reports.ReportConfigurations(ID),
    FOREIGN KEY (GeneratedBy) REFERENCES Security.Users(ID)
);

/*************************************
* TABLAS DE NOTIFICACIONES          *
*************************************/
CREATE TABLE Notifications.Templates (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    Type NVARCHAR(50) NOT NULL, -- EMAIL, SYSTEM, SMS
    Name NVARCHAR(100) NOT NULL,
    Subject NVARCHAR(255),
    Template NVARCHAR(MAX) NOT NULL,
    IsActive BIT DEFAULT 1,
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME,
	FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID)
);

/*************************************
* CONTINUACIÓN TABLAS DE NOTIFICACIONES *
*************************************/
CREATE TABLE Notifications.Notifications (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    TemplateId BIGINT NOT NULL,
    UserId BIGINT NOT NULL,
    Title NVARCHAR(255) NOT NULL,
    Message NVARCHAR(MAX) NOT NULL,
    Type NVARCHAR(50) NOT NULL,
    IsRead BIT DEFAULT 0,
    ReadAt DATETIME,
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (TemplateId) REFERENCES Notifications.Templates(ID),
    FOREIGN KEY (UserId) REFERENCES Security.Users(ID)
);

CREATE TABLE Notifications.NotificationDelivery (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    NotificationId BIGINT NOT NULL,
    DeliveryType NVARCHAR(50) NOT NULL, -- EMAIL, SMS, SYSTEM
    Status NVARCHAR(20) NOT NULL,
    ErrorMessage NVARCHAR(MAX),
    SentAt DATETIME,
    FOREIGN KEY (NotificationId) REFERENCES Notifications.Notifications(ID)
);

/*************************************
* TABLAS DE AUDITORÍA               *
*************************************/
CREATE TABLE Audit.ActivityLogs (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT NOT NULL,
    UserId BIGINT,
    Action NVARCHAR(50) NOT NULL,
    EntityName NVARCHAR(50) NOT NULL,
    EntityId NVARCHAR(50),
    OldValues NVARCHAR(MAX),
    NewValues NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    UserAgent NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (UserId) REFERENCES Security.Users(ID)
);

CREATE TABLE Audit.SecurityLogs (
    ID BIGINT IDENTITY(1,1) PRIMARY KEY,
    TenantId BIGINT,
    UserId BIGINT,
    EventType NVARCHAR(50) NOT NULL,
    Description NVARCHAR(MAX),
    IpAddress NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE(),
    FOREIGN KEY (TenantId) REFERENCES Core.Tenants(ID),
    FOREIGN KEY (UserId) REFERENCES Security.Users(ID)
);

/*************************************
* ÍNDICES                           *
*************************************/

-- Índices para Core
CREATE INDEX IX_Tenants_Identification ON Core.Tenants(Identification);
CREATE INDEX IX_Tenants_IsActive ON Core.Tenants(IsActive);
CREATE INDEX IX_DigitalCertificates_TenantId_Expiration 
    ON Core.DigitalCertificates(TenantId, ExpirationDate, IsActive);
CREATE INDEX IX_ApiConfiguration_TenantId 
    ON Core.ApiConfiguration(TenantId, IsActive);

-- Índices para Security
CREATE INDEX IX_Users_Email_TenantId ON Security.Users(Email, TenantId);
CREATE INDEX IX_Users_TenantId_IsActive ON Security.Users(TenantId, IsActive);
CREATE INDEX IX_Users_Username_TenantId ON Security.Users(Username, TenantId);
CREATE INDEX IX_RefreshTokens_UserId_ExpiryDate 
    ON Security.RefreshTokens(UserId, ExpiryDate, IsActive);
CREATE INDEX IX_Secrets_TenantId_Key 
    ON Security.Secrets(TenantId, [Key], IsActive);

-- Índices para Billing
CREATE INDEX IX_Customers_TenantId_Identification 
    ON Billing.Customers(TenantId, Identification, IsActive);
CREATE INDEX IX_Products_TenantId_Code 
    ON Catalog.Products(TenantId, Code, IsActive);
CREATE INDEX IX_Products_CabysCode 
    ON Catalog.Products(CabysCode);
CREATE INDEX IX_Invoices_TenantId_Status 
    ON Billing.Invoices(TenantId, Status, IsActive);
CREATE INDEX IX_Invoices_TenantId_IssueDate 
    ON Billing.Invoices(TenantId, IssueDate);
CREATE INDEX IX_Invoices_CustomerId 
    ON Billing.Invoices(CustomerId);
CREATE INDEX IX_InvoiceLines_InvoiceId 
    ON Billing.InvoiceLines(InvoiceId);
CREATE INDEX IX_InvoiceStatus_InvoiceId_Status 
    ON Billing.InvoiceStatus(InvoiceId, Status);

-- Índices para Audit
CREATE INDEX IX_ActivityLogs_TenantId_CreatedAt 
    ON Audit.ActivityLogs(TenantId, CreatedAt);
CREATE INDEX IX_ActivityLogs_UserId_Action 
    ON Audit.ActivityLogs(UserId, Action);
CREATE INDEX IX_SecurityLogs_TenantId_EventType 
    ON Audit.SecurityLogs(TenantId, EventType);

/*************************************
* DATOS INICIALES                    *
*************************************/

-- Insertar tipos de identificación
INSERT INTO Catalog.IdentificationTypes (ID, Description, IsActive) VALUES
('01', 'Cédula Física', 1),
('02', 'Cédula Jurídica', 1),
('03', 'DIMEX', 1),
('04', 'NITE', 1);

-- Insertar tipos de documentos electrónicos
INSERT INTO Catalog.DocumentTypes (Code, Name, Description, IsActive) VALUES
('01', 'Factura Electrónica', 'Factura electrónica normal', 1),
('02', 'Nota de Débito', 'Nota de débito electrónica', 1),
('03', 'Nota de Crédito', 'Nota de crédito electrónica', 1),
('04', 'Tiquete Electrónico', 'Tiquete electrónico', 1),
('08', 'Factura Electrónica Compra', 'Factura electrónica de compra', 1),
('09', 'Factura Electrónica Exportación', 'Factura electrónica de exportación', 1);

-- Insertar tipos de impuestos
INSERT INTO Catalog.TaxTypes (Code, Name, Rate, IsExemption, IsActive) VALUES
('01', 'IVA 13%', 13.00, 0, 1),
('02', 'IVA 2%', 2.00, 0, 1),
('03', 'IVA 1%', 1.00, 0, 1),
('04', 'IVA 4%', 4.00, 0, 1),
('05', 'IVA 0%', 0.00, 0, 1),
('06', 'Exento', 0.00, 1, 1);

-- Insertar métodos de pago
INSERT INTO Catalog.PaymentMethods (ID, Description, IsActive) VALUES
('01', 'Efectivo', 1),
('02', 'Tarjeta', 1),
('03', 'Cheque', 1),
('04', 'Transferencia', 1),
('05', 'Recaudado por terceros', 1);

-- Insertar roles básicos del sistema
INSERT INTO Security.Roles (Name, Description, Permissions, IsSystem) VALUES
('SuperAdmin', 'Administrador del sistema', '*', 1),
('TenantAdmin', 'Administrador de empresa', 'tenant.*', 1),
('Billing', 'Usuario de facturación', 'billing.*', 1),
('Reports', 'Usuario de reportes', 'reports.*', 1);

-- Insertar tenant inicial del sistema
INSERT INTO Core.Tenants (
    Name, 
    Identification, 
    CommercialName, 
    Email,
    DefaultCurrency,
    IsActive
) VALUES (
    'Sistema',
    '000000000',
    'Sistema Principal',
    'admin@system.com',
    'CRC',
    1
);

-- Insertar usuario administrador inicial
DECLARE @TenantId BIGINT = (SELECT ID FROM Core.Tenants WHERE Identification = '000000000');
DECLARE @RoleId BIGINT = (SELECT ID FROM Security.Roles WHERE Name = 'SuperAdmin');

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
) VALUES (
    @TenantId,
    'admin@system.com',
    'admin',
    -- Password: Admin123! (BCrypt hash)
    '$2a$11$XEyJPaiE7dT2u3UnS4MGOOyXeH4.bosU3k/nJ9.TgJBWoCJh7w6ge',
    'Admin',
    'System',
    @RoleId,
    1,
    1
);

-- Insertar secreto JWT inicial
INSERT INTO Security.Secrets (
    TenantId,
    [Key],
    [Value],
    Description,
    IsActive
) VALUES (
    @TenantId,
    'JWT_SECRET',
    'MySuperSecret12k3jioasd8o12k3joiajsdij1l2kj3!!!!1k;lajskdjalkdj1sdlkj1ndas123qq',
    'JWT signing key for tenant authentication',
    1
);

-- Insertar plantillas de notificación básicas
INSERT INTO Notifications.Templates (
    TenantId,
    Type,
    Name,
    Subject,
    Template,
    IsActive
) VALUES 
(
    @TenantId,
    'EMAIL',
    'InvoiceCreated',
    'Nueva Factura Electrónica',
    'Estimado {CustomerName}, se ha generado la factura electrónica #{InvoiceNumber}',
    1
),
(
    @TenantId,
    'EMAIL',
    'InvoiceRejected',
    'Factura Electrónica Rechazada',
    'La factura electrónica #{InvoiceNumber} ha sido rechazada por Hacienda',
    1
);

GO