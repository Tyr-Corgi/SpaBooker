-- Verify Moss Spa Services Import
-- Run this to check the services are in the database

-- Count total services
SELECT COUNT(*) as "Total Services" FROM "SpaServices";

-- Count by category (based on name patterns)
SELECT 
    CASE 
        WHEN "Name" LIKE '%Facial%' OR "Name" LIKE '%Peel%' OR "Name" LIKE '%Eye Mask%' THEN 'Facial'
        WHEN "Name" LIKE '%Massage%' OR "Name" LIKE '%CoreStone%' OR "Name" LIKE '%Scalp%' OR "Name" LIKE '%Bambo%' OR "Name" LIKE '%Harmony%' THEN 'Massage'
        WHEN "Name" LIKE '%Package%' OR "Name" LIKE '%Pamper%' OR "Name" LIKE '%Honey%' OR "Name" LIKE '%Tea%' OR "Name" LIKE '%Connection%' OR "Name" LIKE '%Bridal%' OR "Name" LIKE '%Restore%' THEN 'Package'
        ELSE 'Body Treatment'
    END as "Category",
    COUNT(*) as "Count"
FROM "SpaServices"
WHERE "IsActive" = true
GROUP BY 
    CASE 
        WHEN "Name" LIKE '%Facial%' OR "Name" LIKE '%Peel%' OR "Name" LIKE '%Eye Mask%' THEN 'Facial'
        WHEN "Name" LIKE '%Massage%' OR "Name" LIKE '%CoreStone%' OR "Name" LIKE '%Scalp%' OR "Name" LIKE '%Bamboo%' OR "Name" LIKE '%Harmony%' THEN 'Massage'
        WHEN "Name" LIKE '%Package%' OR "Name" LIKE '%Pamper%' OR "Name" LIKE '%Honey%' OR "Name" LIKE '%Tea%' OR "Name" LIKE '%Connection%' OR "Name" LIKE '%Bridal%' OR "Name" LIKE '%Restore%' THEN 'Package'
        ELSE 'Body Treatment'
    END
ORDER BY "Category";

-- List all services with details
SELECT 
    "Id",
    "Name",
    "DurationMinutes" as "Duration (min)",
    "BasePrice" as "Price",
    "IsActive",
    "LocationId"
FROM "SpaServices"
ORDER BY "Name";

-- Check for old filler services (should be none)
SELECT * FROM "SpaServices"
WHERE "Name" IN (
    'Swedish Massage',
    'Deep Tissue Massage',
    'Hot Stone Massage',
    'Aromatherapy Massage',
    'Couples Massage',
    'Luxury Facial Treatment',
    'Body Scrub & Wrap',
    'Exclusive Spa Day Package'
);

