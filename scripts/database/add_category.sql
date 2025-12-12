ALTER TABLE "SpaServices" ADD COLUMN IF NOT EXISTS "Category" VARCHAR(50) NOT NULL DEFAULT 'Other';
CREATE INDEX IF NOT EXISTS "IX_SpaServices_Category" ON "SpaServices" ("Category");
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'SpaServices' AND column_name = 'Category';
