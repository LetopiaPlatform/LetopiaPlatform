# Auth & User Management - Software Requirements Specification (SRS)
## LetopiaPlatform Learning Platform - Phase 1

**Module:** Authentication & User Management  
**Priority:** Critical - Foundation Module  

---

## 1. Module Overview

### 1.1 Purpose
This document specifies the functional and non-functional requirements for the Authentication and User Management module.

### 1.2 Module Scope
- User registration with email verification
- Email/password authentication
- Google OAuth 2.0 integration
- Password reset functionality
- User profile management (bio, avatar, skills, interests)
- Role-based access control (Learner, Guide, Architect)
- Session management
- Account security

---

## 2. Functional Requirements

### 2.1 User Registration

#### FR-AUTH-1.1: Email Registration
**Priority:** Critical  

**User Story:**  
_"As a new user, I want to register for an account using my email and password so that I can access the platform."_

**Acceptance Criteria:**

**Registration Form:**
- Display fields: FirstName, LastName, Email, Password, Confirm Password
- All fields are required
- Password visibility toggle (eye icon)
- "Terms of Service" and "Privacy Policy" checkbox required
- "Already have an account? Log in" link

**Registration Process:**
1. User submits registration form
2. System validates all inputs
3. System checks email uniqueness
4. System creates user account with unverified email status
5. System sends verification email
6. User is automatically logged in
7. User is redirected to home page
8. User can verify email later from profile page

---

#### FR-AUTH-1.2: Email Verification
**Priority:** Critical  

**User Story:**  
_"As a registered user, I want to verify my email address so that I can activate my account and start using the platform."_

**Acceptance Criteria:**

**Verification Email:**
- Subject: "Verify your LetopiaPlatform account"
- Contains: Welcome message, verification link, expiry time (24 hours)
- Branded email template with logo
- Plain text alternative included
- "Didn't request this?" note with ignore instructions

**Verification Process:**
1. User clicks verification link from email
2. System validates verification token
3. If valid:
  - Email is marked as verified
  - Success message is displayed
  - User is redirected to profile page
4. If invalid/expired:
  - Error message is displayed
  - "Resend verification email" option is provided

**Resend Verification:**
- "Resend verification email" button in user profile page (only visible if unverified)
- Rate limit: Max 3 resend requests per hour per email
- New token generated each time (old tokens invalidated)
- Toast notification: "Verification email sent. Check your inbox."
- Button disabled for 60 seconds after sending (shows countdown)

**Unverified Account Handling:**
- User has full access to all platform features
- Verification status shown only in user profile page
- "Verify Email" button visible in profile when unverified
- Optional: Single email reminder sent 7 days after registration
- No banners or persistent notifications

---

#### FR-AUTH-1.3: Google OAuth Registration
**Priority:** High  

**User Story:**  
_"As a new user, I want to sign up using my Google account so that I don't need to create another password."_

**Acceptance Criteria:**

**OAuth Flow:**
1. User clicks "Continue with Google" button
2. User is redirected to Google consent screen
3. User approves permissions (email, profile, picture)
4. System receives user information from Google
5. System checks if email already exists:
  - **If exists with password account:**
    - Google OAuth is automatically linked to existing account
    - Email is marked as verified
    - Message shown: "Your Google account has been linked!"
    - User can now login with either email/password OR Google
  - **If exists with OAuth:**
    - User is logged in normally
  - **If new email:**
    - New account is created with Google information
    - Email is marked as verified
    - Profile picture is imported from Google
6. User is logged in and redirected to home page

**OAuth Permissions Requested:**
- `email` (required)
- `profile` (name, picture)
- `openid` (standard OAuth)

---

### 2.2 User Login

#### FR-AUTH-2.1: Email/Password Login
**Priority:** Critical  

**User Story:**  
_"As a registered user, I want to log in with my email and password so that I can access my account."_

**Acceptance Criteria:**

**Login Form:**
- Fields: Email, Password
- "Forgot password?" link
- "Don't have an account? Sign up" link
- Password visibility toggle
- Submit button with loading state

**Login Process:**
1. User enters credentials (email and password)
2. System validates credentials:
  - Verifies email exists (case-insensitive)
  - Verifies password is correct
  - Checks account is not banned/suspended
3. If valid:
  - Login session is created (30-minute expiry)
  - Last login timestamp is updated
  - Daily streak counter is updated
  - Daily login points are awarded if applicable
  - User is redirected to home page
4. If invalid:
  - Generic error message is displayed

**Failed Login Handling:**
- Invalid credentials: "Invalid email or password"
- Generic error message (don't reveal which field is wrong)
- Increment failed login counter
- After 5 failed attempts: Temporary lockout (15 minutes)
- Email notification on lockout (security alert)

---

#### FR-AUTH-2.2: Google OAuth Login
**Priority:** High  

**User Story:**  
_"As a user with a Google account, I want to log in using Google so that I don't need to remember another password."_

**Acceptance Criteria:**

**OAuth Login Flow:**
1. User clicks "Continue with Google"
2. User is redirected to Google consent screen
3. System receives user information from Google
4. System searches for existing account:
  - First by Google OAuth ID
  - Then by email address (for auto-linking)
5. If found by OAuth ID:
  - User is logged in
  - Last login timestamp is updated
  - Streak counter is updated
  - Profile picture is updated if changed in Google
6. If found by email only (not yet linked):
  - Google OAuth is automatically linked
  - Email is marked as verified
  - Message shown: "Your Google account has been linked!"
  - User is logged in
7. If not found:
  - New account is created (registration flow)

---

#### FR-AUTH-2.3: Session Management
**Priority:** High  

**User Story:**  
_"As a logged-in user, I want my session to be secure and manageable so that my account stays protected."_

**Acceptance Criteria:**

**Session Management:**
- Session duration: 30 minutes
- Session automatically extended on activity
- User remains logged in across browser tabs
- Session validated on every protected action

**Session Persistence:**
- Auto-login if valid session exists
- Session automatically refreshed before expiry

**Session Termination:**
- Logout clears session completely
- User is redirected to home page
- Session cannot be reused after logout

---

### 2.3 Password Management

#### FR-AUTH-3.1: Password Reset
**Priority:** High  

**User Story:**  
_"As a user who forgot my password, I want to reset it so that I can regain access to my account."_

**Acceptance Criteria:**

**Request Password Reset:**
1. User clicks "Forgot password?" on login page
2. User enters email address
3. System processes request:
  - Password reset email is sent if email exists
  - Generic success message shown (security: don't reveal if email exists)
4. Message displayed: "If email exists, reset link sent"

**Password Reset Email:**
- Subject: "Reset your LetopiaPlatform password"
- Contains: Reset link, expiry time (1 hour)
- "Didn't request this?" note included
- Security note: "Link expires in 1 hour"

**Reset Password Form:**
1. User clicks link in email
2. Reset form is displayed: New Password, Confirm Password
3. User enters new password
4. System validates reset token
5. If valid:
  - Password is updated
  - All existing sessions are terminated
  - Confirmation email is sent
  - User is redirected to login
6. If invalid/expired:
  - Error message is displayed
  - User can request new reset link

---

#### FR-AUTH-3.2: Change Password
**Priority:** Medium  

**User Story:**  
_"As a logged-in user, I want to change my password so that I can update my account security."_

**Acceptance Criteria:**

**Change Password Form (Settings Page):**
- Fields: Current Password, New Password, Confirm New Password
- All fields required
- "Save Changes" button

**Change Process:**
1. User enters current password and new password
2. System validates current password
3. System ensures new password is different from current
4. Password is updated
5. Optionally, other sessions are terminated (user choice)
6. Confirmation email is sent
7. Success message is displayed
8. Current session remains active

---

### 2.4 User Profile Management

#### FR-AUTH-4.1: View Profile
**Priority:** High  

**User Story:**  
_"As a user, I want to view my profile and other users' profiles so that I can see information about community members."_

**Acceptance Criteria:**

**Own Profile View:**
- URL: `/profile` or `/profile/me`
- Display:
  - Profile picture (avatar)
  - Full Name (FirstName + LastName)
  - Bio
  - Email (only to self) with verification badge:
   - Green checkmark if verified
   - Yellow warning if unverified with "Verify Email" button
  - Role badge (Learner/Guide/Architect)
  - Skills (tags)
  - Learning interests (tags)
  - Joined communities (list with links)
  - Statistics: Total points, Level, Streak
  - Recent activity
- "Edit Profile" button
- "Settings" link

**Other User Profile View:**
- URL: `/profile/{userId}` or `/users/{userId}`
- Display same info except:
  - Email hidden
  - Email verification status hidden
  - No edit button
  - No settings link
- "Send Message" button (future)

**Profile Sections:**
1. **Header:** Avatar, full name, level badge, points
2. **About:** Bio, email with verification status (own profile only), member since, last active
3. **Skills & Interests:** Tag clouds
4. **Activity:** Recent posts and comments (paginated)
5. **Communities:** List of joined communities
6. **Achievements:** Badges earned (future)

---

#### FR-AUTH-4.2: Edit Profile
**Priority:** High  

**User Story:**  
_"As a user, I want to edit my profile information so that I can keep it up to date."_

**Acceptance Criteria:**

**Edit Profile Form:**
- Accessible from profile page or settings
- Fields:
  - Profile picture (upload)
  - First Name (editable, required)
  - Last Name (editable, required)
  - Bio (textarea, max 500 chars, character counter)
  - Skills (multi-select, max 20)
  - Learning interests (multi-select, max 15)
- "Save Changes" button
- "Cancel" button

**Name Changes:**
- First Name and Last Name always editable
- No uniqueness requirement (multiple users can have same name)
- Changes reflected immediately across platform

**Bio Editing:**
- Rich text editor (bold, italic, links) or plain text
- Character counter (500 max)
- Preview mode
- Support for line breaks

**Skills & Interests Selection:**
- Search/filter dropdown
- Display selected as removable tags
- Add custom skills
- Categorized suggestions

**Profile Picture Upload:**
- Click avatar to upload
- Drag & drop support
- Crop/resize tool (square aspect ratio)
- Preview before save
- Upload to File Storage
- Update avatar_url in database

**Save Process:**
1. User clicks "Save Changes"
2. System uploads new profile picture if changed
3. System updates user profile
4. System deletes old profile picture if replaced
5. If profile is 100% complete, award one-time completion bonus (20 points)
6. Success notification is displayed
7. Profile view is updated with new data

**Profile Completion:**
- Track completion percentage
- Award 20 points for 100% profile completion (one-time)
- Criteria: Avatar, bio, 3+ skills, 3+ interests

---

#### FR-AUTH-4.3: Profile Picture Management
**Priority:** Medium  

**User Story:**  
_"As a user, I want to upload and change my profile picture so that I can personalize my account."_

**Acceptance Criteria:**

**Upload Profile Picture:**
- Click on avatar/placeholder to open upload dialog
- Drag & drop support on avatar area
- File input: Accept image/* (JPEG, PNG, GIF, WebP)
- Max file size: 5MB

**Image Processing:**
1. User selects image
2. Crop/resize modal is displayed
3. User adjusts image (reposition, zoom)
4. Preview is shown in real-time
5. User confirms with "Save" button
6. Image is uploaded and optimized
7. Profile picture is updated
8. Previous image is deleted (if not default)

**Default Avatars:**
- Use initials-based avatar generator if no upload
- First initial from FirstName + first initial from LastName
- Color based on user ID hash (consistent per user)
- Format: Circular with 2 initials (e.g., "JD" for John Doe)
- Fallback: Single initial if LastName missing

**Remove Profile Picture:**
- "Remove photo" option in edit menu
- Confirmation dialog
- Revert to default avatar
- Delete from File Storage

**Performance:**
- Progressive upload with progress bar
- Thumbnail generation for fast loading

---

### 2.5 Role-Based Access Control (RBAC)

#### FR-AUTH-5.1: User Roles
**Priority:** High  

**User Story:**  
_"As the system, I want to assign roles to users so that I can control access to different features and empower community creators."_

**Acceptance Criteria:**

**Roles Defined:**

1. **Learner (Default Role)** 🎓
  - Default role assigned on registration
  - Can join communities
  - Can create posts in joined communities
  - Can comment and vote on content
  - Can create own profile and manage settings
  - Cannot create new communities
  - Cannot moderate content
  - Cannot access platform administration

2. **Guide (Community Creator/Moderator)** 🌟
  - All Learner permissions PLUS:
  - **Can create unlimited communities**
  - Automatically becomes moderator of created communities
  - Can moderate content within their communities:
    - Delete posts/comments in their communities
    - Pin important posts
    - Edit community settings
    - Invite/remove members (future)
    - Ban users from their communities (future)
  - Can assign other users as co-moderators (future)
  - Special "Guide" badge visible on profile
  - Enhanced profile visibility
  - Access to community analytics dashboard
  - Elevated to Guide status after:
    - Account age > 30 days AND
    - Total points > 500 AND
    - Active in 3+ communities AND
    - No moderation violations
  - OR manually promoted by Architect

3. **Architect (Platform Administrator)** 🏛️
  - All Guide permissions PLUS:
  - Full platform administration access
  - Can delete any content across all communities
  - Can ban/suspend any user
  - Can promote users to Guide or Architect
  - Can demote Guide users to Learner
  - Access to admin dashboard
  - Can manage platform-wide settings
  - Can view all user data and analytics
  - Can override community moderator decisions
  - Can delete or archive any community
  - Manually assigned only (no auto-promotion)

**Role Assignment:**
- Default role on registration: **Learner**
- **Guide Promotion:**
  - Auto-promotion when criteria met (system checks daily)
  - Manual promotion by Architect via admin panel
  - Email notification on promotion with Guide benefits
  - Badge and profile banner unlocked
- **Architect Assignment:**
  - Only manual assignment by existing Architect
  - Requires approval from 2+ existing Architects (future)
  - Full audit trail maintained
- All role changes are:
  - Logged with timestamp and actor
  - Notified to user via email

**Role Display:**
- Badge/icon next to username on all content
- Color coding:
  - Learner: Light blue (subtle)
  - Guide: Gold (prominent)
  - Architect: Purple (distinguished)
- Visible on: profile header, posts, comments, leaderboards
- Hover tooltip explains role and privileges
- Special frame/border on Guide and Architect avatars

**Role Enforcement:**
- Permissions checked on every protected action
- Community-specific moderation checks
- UI adapts based on user role:
  - "Create Community" button only for Guide and Architect
  - Moderation tools only for community moderators
  - Admin panel only for Architect
- All permissions double-checked on backend

---

#### FR-AUTH-5.2: Permission System
**Priority:** High  

**User Story:**  
_"As the system, I want to enforce permissions based on user roles so that security is maintained and community creators are empowered."_

**Acceptance Criteria:**

**Permission Matrix:**

| Resource | Action | Learner | Guide | Architect |
|----------|--------|---------|-------|----------|
| **Own Profile** | View | ✅ | ✅ | ✅ |
| **Own Profile** | Edit | ✅ | ✅ | ✅ |
| **Other Profiles** | View | ✅ | ✅ | ✅ |
| **Other Profiles** | Edit | ❌ | ❌ | ✅ |
| **Own Content** | Create/Edit/Delete | ✅ | ✅ | ✅ |
| **Other Content** | View/Vote/Comment | ✅ | ✅ | ✅ |
| **Other Content** | Delete (Own Community) | ❌ | ✅* | ✅ |
| **Other Content** | Delete (Any Community) | ❌ | ❌ | ✅ |
| **Communities** | Join/Leave | ✅ | ✅ | ✅ |
| **Communities** | Create | ❌ | ✅ | ✅ |
| **Communities** | Edit (Own) | ❌ | ✅* | ✅ |
| **Communities** | Edit (Any) | ❌ | ❌ | ✅ |
| **Communities** | Delete (Own) | ❌ | ✅* | ✅ |
| **Communities** | Delete (Any) | ❌ | ❌ | ✅ |
| **Moderation** | Moderate Own Communities | ❌ | ✅ | ✅ |
| **Moderation** | Moderate Any Community | ❌ | ❌ | ✅ |
| **User Management** | View All Users | ❌ | ❌ | ✅ |
| **User Management** | Promote to Guide | ❌ | ❌ | ✅ |
| **User Management** | Ban/Suspend | ❌ | ❌ | ✅ |
| **Platform Settings** | Access Admin Panel | ❌ | ❌ | ✅ |
| **Platform Settings** | Change Settings | ❌ | ❌ | ✅ |

**\* Guide permissions only apply to communities they created or co-moderate**

**Authorization Enforcement:**
- Every action validates user permissions
- Unauthorized actions return error (403 Forbidden)
- UI hides actions user cannot perform
- Backend always validates permissions (never trusts client)

**Special Cases:**
- **Community Ownership:**
  - Guides can fully manage communities they created
  - Guides can be assigned as co-moderators of other communities
  - Community creator has ultimate authority (unless Architect intervenes)
  
- **Content Moderation:**
  - Post authors can edit/delete their own posts
  - Comment authors can edit/delete their own comments
  - Guides can delete any post/comment in their communities
  - Architects can delete any content platform-wide
  
- **Community-Specific Permissions:**
  - Only community moderators (creator + co-moderators) can:
   - Delete posts in that community
   - Pin/unpin posts
   - Edit community description and settings
   - Manage community tags

---

## 3. Non-Functional Requirements

### 3.1 Performance

**NFR-AUTH-1: Response Time**
- Login request: < 500ms (95th percentile)
- Registration: < 1 second
- Profile load: < 300ms
- Token validation: < 50ms

**NFR-AUTH-2: Scalability**
- Support 10,000 concurrent login requests
- Handle 50,000 daily active users
- System scales horizontally as user base grows
- Database and cache optimized for high performance

### 3.2 Security

**NFR-AUTH-3: Data Protection**
- Passwords encrypted using industry-standard hashing
- All authentication endpoints require encrypted connections (HTTPS)
- Session tokens have short expiry (30 minutes)
- Protection against CSRF, XSS, and SQL injection attacks
- Input validation and sanitization on all forms

**NFR-AUTH-4: Privacy**
- GDPR compliant data handling
- User consent for data collection
- Privacy policy linked

**NFR-AUTH-5: Session Management**
- Secure session storage
- Sessions protected from tampering
- Revoked sessions cannot be reused
- Automatic session renewal for active users

### 3.3 Reliability

**NFR-AUTH-6: Availability**
- Authentication service uptime: 99.9%
- System continues functioning if external OAuth providers are unavailable
- Email delivery system has retry mechanism
- Database has automatic failover capability

**NFR-AUTH-7: Error Handling**
- User-friendly error messages (no technical jargon)
- Error messages never reveal sensitive information
- All errors logged for troubleshooting
- Multiple authentication methods available (email, OAuth)

### 3.4 Usability

**NFR-AUTH-8: User Experience**
- Mobile-responsive auth forms
- Clear validation messages
- Progressive form enhancement
- Accessibility compliance (WCAG 2.1 AA)
- Loading states for all actions

**NFR-AUTH-9: Documentation**
- API documentation
- User help articles (future)
- Password requirements clearly stated
- Error messages actionable

---

## 4. Related Documentation

---