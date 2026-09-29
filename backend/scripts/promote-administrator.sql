-- Run against the Vocabularity database using a trusted database operator account.
-- Register the account through the API first, then set its email below.
-- This script promotes only the named existing account; there is no public role-assignment API.
SET XACT_ABORT ON;
DECLARE @Email nvarchar(254) = N'REPLACE_WITH_REGISTERED_EMAIL';

IF @Email = N'REPLACE_WITH_REGISTERED_EMAIL'
    THROW 50000, 'Set @Email to the account to promote before running this script.', 1;

UPDATE dbo.Users
SET Role = N'Administrator', UpdatedAt = SYSUTCDATETIME()
WHERE NormalizedEmail = UPPER(LTRIM(RTRIM(@Email))) AND IsActive = 1;

IF @@ROWCOUNT <> 1
    THROW 50001, 'Exactly one active registered account must match the supplied email.', 1;
