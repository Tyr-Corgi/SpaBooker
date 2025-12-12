@echo off
SET PGPASSWORD=password123
"C:\Program Files\PostgreSQL\17\bin\psql" -U postgres -h localhost -p 5433 -d spabooker -f C:\Mac\Home\Desktop\Repos\SpaBooker\create_service_group_rooms_table.sql
pause

