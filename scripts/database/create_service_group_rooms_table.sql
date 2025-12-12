-- Create ServiceGroupRooms table
CREATE TABLE IF NOT EXISTS "ServiceGroupRooms" (
    "ServiceGroupId" integer NOT NULL,
    "RoomId" integer NOT NULL,
    "AssignedAt" timestamp with time zone NOT NULL DEFAULT (NOW() AT TIME ZONE 'UTC'),
    CONSTRAINT "PK_ServiceGroupRooms" PRIMARY KEY ("ServiceGroupId", "RoomId"),
    CONSTRAINT "FK_ServiceGroupRooms_ServiceGroups_ServiceGroupId" FOREIGN KEY ("ServiceGroupId") REFERENCES "ServiceGroups" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_ServiceGroupRooms_Rooms_RoomId" FOREIGN KEY ("RoomId") REFERENCES "Rooms" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ServiceGroupRooms_RoomId" ON "ServiceGroupRooms" ("RoomId");

-- Populate from RoomServiceCapabilities
INSERT INTO "ServiceGroupRooms" ("ServiceGroupId", "RoomId", "AssignedAt")
SELECT 
    sg."Id" as "ServiceGroupId",
    rsc."RoomId",
    NOW() AT TIME ZONE 'UTC' as "AssignedAt"
FROM "RoomServiceCapabilities" rsc
INNER JOIN "SpaServices" ss ON rsc."ServiceId" = ss."Id"
INNER JOIN "ServiceGroups" sg ON ss."Name" = sg."Name" AND ss."LocationId" = sg."LocationId"
WHERE NOT EXISTS (
    SELECT 1 FROM "ServiceGroupRooms" sgr 
    WHERE sgr."ServiceGroupId" = sg."Id" AND sgr."RoomId" = rsc."RoomId"
)
ON CONFLICT DO NOTHING;










