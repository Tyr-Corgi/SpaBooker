-- Data migration script to group existing SpaServices into ServiceGroups with ServiceDurations
-- This script analyzes service names to detect duration patterns and groups them accordingly

BEGIN TRANSACTION;

-- Step 1: Create ServiceGroups from unique base service names
INSERT INTO "ServiceGroups" ("Name", "Description", "ImageUrl", "LocationId", "RequiresMembership", "IsActive", "CreatedAt", "UpdatedAt")
SELECT 
    -- Extract base name by removing duration suffix patterns like "(30 min)", "(60 min)"
    CASE 
        WHEN "Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE("Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE "Name"
    END as base_name,
    MIN("Description") as "Description",
    MIN("ImageUrl") as "ImageUrl",
    "LocationId",
    bool_or("RequiresMembership") as "RequiresMembership",
    bool_and("IsActive") as "IsActive",
    MIN("CreatedAt") as "CreatedAt",
    MAX("UpdatedAt") as "UpdatedAt"
FROM "SpaServices"
GROUP BY 
    CASE 
        WHEN "Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE("Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE "Name"
    END,
    "LocationId";

-- Step 2: Create ServiceDurations for each original service
INSERT INTO "ServiceDurations" ("ServiceGroupId", "DurationMinutes", "Price", "IsActive", "CreatedAt")
SELECT 
    sg."Id" as "ServiceGroupId",
    s."DurationMinutes",
    s."BasePrice" as "Price",
    s."IsActive",
    s."CreatedAt"
FROM "SpaServices" s
INNER JOIN "ServiceGroups" sg ON
    (CASE 
        WHEN s."Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s."Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE s."Name"
    END) = sg."Name"
    AND s."LocationId" = sg."LocationId";

-- Step 3: Migrate ServiceTherapist to ServiceGroupTherapist
INSERT INTO "ServiceGroupTherapists" ("ServiceGroupId", "TherapistId", "AssignedAt")
SELECT DISTINCT
    sg."Id" as "ServiceGroupId",
    st."TherapistId",
    MIN(st."AssignedAt") as "AssignedAt"
FROM "ServiceTherapists" st
INNER JOIN "SpaServices" s ON st."ServiceId" = s."Id"
INNER JOIN "ServiceGroups" sg ON
    (CASE 
        WHEN s."Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s."Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE s."Name"
    END) = sg."Name"
    AND s."LocationId" = sg."LocationId"
GROUP BY sg."Id", st."TherapistId";

-- Step 4: Update bookings to reference ServiceDuration
UPDATE "Bookings" b
SET "ServiceDurationId" = sd."Id"
FROM "SpaServices" s
INNER JOIN "ServiceGroups" sg ON
    (CASE 
        WHEN s."Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s."Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE s."Name"
    END) = sg."Name"
    AND s."LocationId" = sg."LocationId"
INNER JOIN "ServiceDurations" sd ON 
    sd."ServiceGroupId" = sg."Id" 
    AND sd."DurationMinutes" = s."DurationMinutes"
WHERE b."ServiceId" = s."Id";

COMMIT;

