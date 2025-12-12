-- Check if ServiceGroups exist
SELECT COUNT(*) as "ServiceGroups Count" FROM "ServiceGroups";

-- Check if SpaServices exist
SELECT COUNT(*) as "SpaServices Count" FROM "SpaServices";

-- Show first 5 ServiceGroups with their status
SELECT "Id", "Name", "IsActive", "Category", "LocationId" 
FROM "ServiceGroups" 
ORDER BY "Name" 
LIMIT 5;











