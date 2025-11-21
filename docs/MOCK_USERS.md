# Mock User Accounts for Testing

The database is automatically seeded with mock users for testing purposes. These accounts are created when the application first runs.

## 🔐 Mock User Credentials

### Admin Account
- **Email:** `admin@spabooker.com`
- **Password:** `Admin123!`
- **Name:** System Administrator
- **Access:** Full administrative access to all features
- **Can do:**
  - Manage users, services, banners
  - View all bookings and schedules
  - Manage gift certificates
  - Access analytics (coming soon)
  - Configure inventory
  - Everything a therapist can do

### Therapist Account
- **Email:** `therapist@spabooker.com`
- **Password:** `Therapist123!`
- **Name:** Sarah Johnson
- **Phone:** (555) 987-6543
- **Specialties:** Swedish, Deep Tissue, Hot Stone
- **Experience:** 8 years
- **Access:** Therapist features
- **Can do:**
  - View their schedule
  - Manage availability
  - View assigned bookings
  - Update inventory
  - View client information for their bookings

### Client Account
- **Email:** `client@spabooker.com`
- **Password:** `Client123!`
- **Name:** Emma Williams
- **Phone:** (555) 123-9876
- **Access:** Client features
- **Can do:**
  - Browse services
  - Book appointments
  - View/cancel their bookings
  - Purchase gift certificates
  - View gift certificates
  - Subscribe to membership plans
  - Manage profile

## 🔄 How to Use

1. **Start the application:** `dotnet run --project src/SpaBooker.Web`
2. **Navigate to login:** Click "Login" in the navigation
3. **Choose an account:** Use any of the credentials above
4. **Explore features:** Each role sees different navigation options and has different permissions

## 🎨 What's New - Aesthetic Features

### Home Page
- Beautiful full-screen carousel with animated banners
- Default banners with New Zealand nature themes (placeholder images)
- Smooth transitions and hover effects
- Mobile responsive

### Services Page
- Service cards now display images
- Hover animations with rose gold shadows
- Graceful placeholder for services without images
- Beautiful gradient backgrounds

### Admin Features
- **Banner Management** (`/admin/banners`):
  - Create, edit, delete, and reorder banners
  - Upload image URLs for carousel
  - Set titles, subtitles, descriptions
  - Add call-to-action buttons
  - Activate/deactivate banners
  
- **Service Management** (updated):
  - Now includes Image URL field
  - Add images to each service
  - Beautiful display on services page

## 📝 Tips for Testing

1. **Test as different roles:** Log out and log back in with different accounts
2. **Admin can see everything:** Best account for exploring all features
3. **Therapist sees schedule view:** Check out the therapist-specific features
4. **Client sees booking flow:** Experience the customer journey
5. **Add your own images:** Use Unsplash for free spa/nature images
   - Home banners: 1920x1080px recommended
   - Service images: 800x600px recommended

## 🔧 Database Seeding

Mock users are automatically created by `DbSeeder.cs` on first run. The seeder also creates:
- 2 spa locations (Downtown & Waterfront)
- 3 membership plans (Bronze, Silver, Gold)
- 8 spa services per location

## 🚫 Important Notes

- **DO NOT** use these credentials in production
- **DO NOT** commit real passwords to Git
- Mock users are for **development/testing only**
- Change the default seeding logic before deploying to production

