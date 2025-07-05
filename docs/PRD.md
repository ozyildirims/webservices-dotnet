# Product Requirements Document (PRD)

## 1. Executive Summary

The Limit Kurs Backend is a modular, scalable backend system for a mobile education platform that serves students, teachers, parents, guests, and administrators. The platform enables scheduling and reservations for study sessions, tracking exam results, managing lesson programs, handling attendance, and delivering system-wide announcements and notifications.

## 2. Product Overview

### 2.1 Product Vision

To provide a robust, modular, and secure backend system that empowers all stakeholders in a private education setting with clear communication, personalized tracking, and seamless digital services.

### 2.2 Product Goals

* Enable multi-role authentication and secure access for users (students, parents, teachers, admins, guests)
* Support all educational operations including exams, study sessions, attendance, and lesson programs
* Provide a notification system to keep all users informed and engaged
* Offer a clean and API-driven architecture suitable for mobile and admin panel integration

### 2.3 Success Metrics

* 99.9% API uptime
* Sub-500ms average response time per API request
* 100% functional coverage for core modules
* Admin ability to create/track system-wide events and messages

## 3. Target Audience

### 3.1 Primary Users

* Students: Participate in sessions, view grades, track attendance
* Teachers: Manage lessons, exams, and student attendance
* Parents: Monitor children’s academic progress and attendance
* Admins: Oversee all modules and ensure system consistency

### 3.2 Secondary Users

* Guests: Can request private lessons without full registration

## 4. Functional Requirements

### 4.1 Core Features

* Multi-role User Management & Authentication
* Announcements & Notifications
* Study Sessions & Reservations
* Exams & Results Tracking
* Attendance Management
* Lesson Scheduling & Timetables
* Guest Lesson Requests
* Admin Panel Operations

### 4.2 User Stories

* As a student, I want to reserve study sessions so I can participate actively.
* As a teacher, I want to assign lessons and track student attendance.
* As a parent, I want to view my child’s grades and attendance records.
* As an admin, I want to send announcements and define system-level configurations.
* As a guest, I want to request a private lesson easily.

## 5. Non-Functional Requirements

### 5.1 Performance

* Each endpoint should respond within 500ms
* Study sessions and exam result listings must paginate efficiently

### 5.2 Security

* JWT-based role authentication
* Passwords stored with bcrypt hashing
* Secure access only to authorized roles per module

### 5.3 Scalability

* MSSQL based relational data modeling
* Modular services support independent deployment and scaling
* Background job processing for notifications and scheduling

## 6. Technical Requirements

### 6.1 Technology Stack

* .NET 9
* MSSQL
* EF Core (code-first migrations)
* JWT Authentication
* Next Js - Admin Panel
* Optional: Redis, Hangfire/Quartz for background tasks

### 6.2 Integration Points

* Firebase Cloud Messaging (push notifications)
* Excel import/export for exam results (admin panel)

## 7. User Experience

### 7.1 User Interface

* Consumed primarily via React Native mobile app
* Admin panel accessed via web (next js)

### 7.2 User Flow

* Guest can browse and request lessons
* Student logs in to view profile, results, sessions, attendance
* Teacher manages schedules, exam definitions, and student progress
* Parent logs in to monitor child’s academic activity

## 8. Constraints and Assumptions

### 8.1 Constraints

* Users must have internet access to interact with backend services
* Multi-role design must not allow role escalation (e.g., student becoming admin)

### 8.2 Assumptions

* Users have verified accounts
* Mobile app handles local session storage and token management
* Admins will seed initial roles and setup data

## 9. Timeline and Milestones

### 9.1 Development Phases

* **Phase 1**: User Management + JWT Auth (Week 1)
* **Phase 2**: Announcements & Notifications (Week 2)
* **Phase 3**: Study Sessions & Reservations (Week 3)
* **Phase 4**: Exam Definitions & Results (Week 4)
* **Phase 5**: Attendance Tracking + Lesson Programs (Week 5)
* **Phase 6**: Guest Lesson Requests + Admin Tools (Week 6)

## 10. Risk Assessment

### 10.1 Technical Risks

* Risk: Token expiration mismanagement
  Mitigation: Add token refresh flow or short expiration with auto-renew

* Risk: Race conditions on study session reservations
  Mitigation: Use database-level transactions or row locks

### 10.2 Business Risks

* Risk: Admin panel development delays
  Mitigation: Prioritize core APIs; admin panel can be decoupled

## 11. Appendix

### 11.1 Glossary

* **Session**: Scheduled study event a student can join
* **Deneme**: Mock exam
* **Guest**: Unregistered user browsing or requesting lessons

### 11.2 References

* OpenAPI Specification 3.0
* OWASP JWT Best Practices
* EF Core Documentation
* Firebase Cloud Messaging Docs
