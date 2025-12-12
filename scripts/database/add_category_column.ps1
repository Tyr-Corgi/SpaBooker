$env:PGPASSWORD = 'password123'
$result = & psql -U postgres -p 5433 -d spabooker -c "ALTER TABLE `"SpaServices`" ADD COLUMN IF NOT EXISTS `"Category`" VARCHAR(50) NOT NULL DEFAULT 'Other';" 2>&1
Write-Host "ALTER TABLE result: $result"
$result2 = & psql -U postgres -p 5433 -d spabooker -c "CREATE INDEX IF NOT EXISTS `"IX_SpaServices_Category`" ON `"SpaServices`" (`"Category`");" 2>&1
Write-Host "CREATE INDEX result: $result2"
$result3 = & psql -U postgres -p 5433 -d spabooker -c "SELECT column_name, data_type FROM information_schema.columns WHERE table_name = 'SpaServices' AND column_name = 'Category';" 2>&1
Write-Host "Verification result:"
Write-Host $result3
