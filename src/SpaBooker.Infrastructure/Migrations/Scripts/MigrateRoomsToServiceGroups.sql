-- =====================================================
-- Data Migration: Populate ServiceGroupRoom from RoomServiceCapability
-- =====================================================
-- This script migrates room assignments from the old SpaService model
-- to the new ServiceGroup model
-- =====================================================

BEGIN;

-- Step 1: Populate ServiceGroupRoom from RoomServiceCapability
-- Groups services by name to find matching ServiceGroups
INSERT INTO "ServiceGroupRooms" ("ServiceGroupId", "RoomId", "AssignedAt")
SELECT DISTINCT
    sg."Id" as "ServiceGroupId",
    rsc."RoomId",
    MIN(rsc."CreatedAt") as "AssignedAt"
FROM "RoomServiceCapabilities" rsc
INNER JOIN "SpaServices" s ON rsc."ServiceId" = s."Id"
INNER JOIN "ServiceGroups" sg ON
    -- Match by service name (removing duration suffix if present)
    (CASE 
        WHEN s."Name" ~* '\s*\((\d+)\s*min\)$' THEN REGEXP_REPLACE(s."Name", '\s*\((\d+)\s*min\)$', '', 'i')
        ELSE s."Name"
    END) = sg."Name"
    AND s."LocationId" = sg."LocationId"
WHERE NOT EXISTS (
    -- Avoid duplicates
    SELECT 1 FROM "ServiceGroupRooms" sgr
    WHERE sgr."ServiceGroupId" = sg."Id"
    AND sgr."RoomId" = rsc."RoomId"
)
GROUP BY sg."Id", rsc."RoomId";

-- Step 2: Add default category for existing ServiceGroups (if Category is empty)
UPDATE "ServiceGroups"
SET "Category" = 'General'
WHERE "Category" IS NULL OR "Category" = '';

-- Report results
DO $$
DECLARE
    room_count INTEGER;
    group_count INTEGER;
BEGIN
    SELECT COUNT(*) INTO room_count FROM "ServiceGroupRooms";
    SELECT COUNT(*) INTO group_count FROM "ServiceGroups";
    
    RAISE NOTICE 'Migration Complete:';
    RAISE NOTICE '  - % room assignments migrated', room_count;
    RAISE NOTICE '  - % service groups updated', group_count;
END $$;

COMMIT;

