-- =============================================
-- Delete All Filler/Demo Services from Database
-- =============================================
-- This script removes all the test/demo services that were seeded
-- Run this BEFORE importing real Moss Spa services
-- =============================================

-- Start transaction for safety
BEGIN;

PRINT 'Starting deletion of filler services...';

-- Step 1: Delete related ServiceTherapist assignments first (foreign key)
DELETE FROM "ServiceTherapists"
WHERE "ServiceId" IN (
    SELECT "Id" FROM "SpaServices"
    WHERE "Name" IN (
        'Swedish Massage',
        'Deep Tissue Massage',
        'Hot Stone Massage',
        'Aromatherapy Massage',
        'Couples Massage',
        'Luxury Facial Treatment',
        'Body Scrub & Wrap',
        'Exclusive Spa Day Package'
    )
);

PRINT 'Deleted ServiceTherapist assignments';

-- Step 2: Delete RoomServiceCapabilities (foreign key)
DELETE FROM "RoomServiceCapabilities"
WHERE "ServiceId" IN (
    SELECT "Id" FROM "SpaServices"
    WHERE "Name" IN (
        'Swedish Massage',
        'Deep Tissue Massage',
        'Hot Stone Massage',
        'Aromatherapy Massage',
        'Couples Massage',
        'Luxury Facial Treatment',
        'Body Scrub & Wrap',
        'Exclusive Spa Day Package'
    )
);

PRINT 'Deleted RoomServiceCapabilities';

-- Step 3: Delete any bookings for these services
-- WARNING: This will delete booking history. If you want to preserve bookings,
-- comment out this section or modify to set status to 'Cancelled' instead
DELETE FROM "Bookings"
WHERE "ServiceId" IN (
    SELECT "Id" FROM "SpaServices"
    WHERE "Name" IN (
        'Swedish Massage',
        'Deep Tissue Massage',
        'Hot Stone Massage',
        'Aromatherapy Massage',
        'Couples Massage',
        'Luxury Facial Treatment',
        'Body Scrub & Wrap',
        'Exclusive Spa Day Package'
    )
);

PRINT 'Deleted Bookings (if any)';

-- Step 4: Finally, delete the services themselves
DELETE FROM "SpaServices"
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

PRINT 'Deleted SpaServices';

-- Show summary
SELECT 'Remaining services:' AS Status, COUNT(*) AS Count FROM "SpaServices";

-- Commit the transaction
-- COMMIT;

-- OR if you want to review first without committing:
-- ROLLBACK;

PRINT 'Deletion complete! Review the results and COMMIT or ROLLBACK as needed.';

