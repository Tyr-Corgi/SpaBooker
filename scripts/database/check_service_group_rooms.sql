-- Check if ServiceGroupRooms table exists and has data
SELECT COUNT(*) as "ServiceGroupRooms Count" FROM "ServiceGroupRooms";

-- Show sample data
SELECT sgr."ServiceGroupId", sgr."RoomId", sg."Name" as "ServiceName", r."Name" as "RoomName"
FROM "ServiceGroupRooms" sgr
INNER JOIN "ServiceGroups" sg ON sgr."ServiceGroupId" = sg."Id"
INNER JOIN "Rooms" r ON sgr."RoomId" = r."Id"
LIMIT 5;











