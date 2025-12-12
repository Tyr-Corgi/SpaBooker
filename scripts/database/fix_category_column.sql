-- This script adds the Category column to ServiceGroups table
-- Run this in pgAdmin or your PostgreSQL client connected to port 5433

-- Add Category column to ServiceGroups if it doesn't exist
ALTER TABLE "ServiceGroups" 
ADD COLUMN IF NOT EXISTS "Category" VARCHAR(255) NOT NULL DEFAULT 'General';

-- Verify the column was added
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'ServiceGroups' AND column_name = 'Category';

-- Show all ServiceGroups with their categories
SELECT "Id", "Name", "Category" FROM "ServiceGroups";
