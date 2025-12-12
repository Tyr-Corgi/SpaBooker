-- Add Category column to ServiceGroups table
ALTER TABLE "ServiceGroups" 
ADD COLUMN IF NOT EXISTS "Category" text NOT NULL DEFAULT 'General';

-- Show the updated structure
SELECT "Id", "Name", "IsActive", "Category", "LocationId" 
FROM "ServiceGroups" 
ORDER BY "Name" 
LIMIT 5;











