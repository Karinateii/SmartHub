# SmartHub Screenshots Guide

This guide helps you capture screenshots to showcase the SmartHub project on LinkedIn, GitHub, and your portfolio.

## Essential screenshots

### 1. Swagger API documentation

What to capture: the Swagger UI showing your API endpoints

Steps:
1. Run the application: `dotnet run --project SmartHub.Api`
2. Open the Swagger page: `https://localhost:7xxx/swagger`
3. Expand the main sections (Auth, Users, etc.)
4. Capture a full-page screenshot that shows endpoints, methods, and descriptions

Why: demonstrates API documentation and endpoint design

Save as: `screenshots/swagger-overview.png`

---

### 2. Swagger Request Example

**What to capture:** Expanded POST /api/auth/register endpoint showing request body schema

**Steps:**
1. In Swagger, click on `POST /api/auth/register`
2. Click "Try it out"
3. Show the example request body with schema validation
4. Capture the request parameters and example values

**Why:** Demonstrates your API's request/response structure

**Save as:** `screenshots/swagger-register-endpoint.png`

---

### 3. Postman collection

Capture a Postman collection or API client showing successful requests and responses. Include register, login, refresh token, and profile requests.

Save as: `screenshots/postman-collection.png`

---

### 4. Database Schema (Entity Framework)

**What to capture:** Database tables showing your domain model

**Option A - Visual Studio:**
1. Open Server Explorer / SQL Server Object Explorer
2. Expand your database connection
3. Show Tables folder with your entities (Users, etc.)
4. Right-click a table → "View Designer" to show columns

**Option B - SQL Server Management Studio:**
1. Connect to your LocalDB: `(localdb)\mssqllocaldb`
2. Expand Databases → SmartHubDb → Tables
3. Right-click Users table → "Design" to show schema
4. Capture columns with data types

**Option C - dbdiagram.io:**
1. Create a database diagram at https://dbdiagram.io
2. Define your schema:
```sql
Table Users {
  Id uniqueidentifier [primary key]
  FirstName nvarchar(50)
  LastName nvarchar(50)
  Email nvarchar(256) [unique]
  PasswordHash nvarchar(max)
  Role int
  EmailVerified bit
  RefreshToken nvarchar(max)
  RefreshTokenExpiry datetime2
  CreatedAt datetime2
  UpdatedAt datetime2
}
```
3. Export as image

**Why:** Demonstrates database design and data modeling skills

**Save as:** `screenshots/database-schema.png`

---

### 5. Solution structure

Capture the solution explorer showing project structure (Api, Application, Infrastructure, Domain, Tests) to demonstrate the Clean Architecture layout.

Save as: `screenshots/solution-structure.png`

---

### 6. Code Quality (Optional but Impressive)

**What to capture:** Clean, well-documented code

**Steps:**
1. Open a key file like `AuthService.cs` or `AuthController.cs`
2. Show:
   - XML documentation comments
   - Dependency injection
   - Clean method signatures
   - Error handling
3. Use a nice VS Code or Visual Studio theme

**Why:** Shows code quality and documentation practices

**Save as:** `screenshots/code-example.png`

---

### 7. Test results

Capture test output that shows passing tests. This can be a screenshot of Test Explorer or terminal output from `dotnet test`.

Save as: `screenshots/tests-passing.png`

---

### 8. GitHub Actions CI/CD (Highly Recommended!)

**What to capture:** GitHub Actions workflow running successfully

**Steps:**
1. Push your code to GitHub (if not already)
2. Go to your repository → "Actions" tab
3. Show a successful build with green checkmarks
4. Expand to show:
   - Restore dependencies
   - Build
   - Test steps
   - Both Ubuntu and Windows runners

**Why:** Shows DevOps knowledge and professional deployment practices

**Save as:** `screenshots/github-actions-ci.png`

---

### 9. API response example (terminal or client)

Capture a real API request and response from Postman or a terminal client to show the API in action.

Example PowerShell snippet (for local testing):

```powershell
# Register request
$body = @{
   firstName = "Demo"
   lastName = "User"
   email = "demo@example.com"
   password = "SecurePass123!"
   confirmPassword = "SecurePass123!"
} | ConvertTo-Json

$response = Invoke-RestMethod -Uri "https://localhost:7xxx/api/auth/register" -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck

$response | ConvertTo-Json -Depth 10
```

Save as: `screenshots/api-response-example.png`

---

### 10. Logs (Serilog)

**What to capture:** Structured logging output

**Steps:**
1. Run the application
2. Make several API requests
3. Open `logs/smarthub.log`
4. Show formatted log entries with:
   - Timestamps
   - Log levels (Information, Warning, Error)
   - Structured data
   - Request/response logging

**Why:** Demonstrates observability and production-ready practices

**Save as:** `screenshots/structured-logs.png`

---

## 📁 Folder Structure

Create this structure in your repository:

```
SmartHub/
├── screenshots/
│   ├── swagger-overview.png
│   ├── swagger-register-endpoint.png
│   ├── postman-collection.png
│   ├── database-schema.png
│   ├── solution-structure.png
│   ├── code-example.png
│   ├── tests-passing.png
│   ├── github-actions-ci.png
│   ├── api-response-example.png
│   └── structured-logs.png
└── README.md
```

---

## Screenshot best practices

Image quality
- Use sufficient resolution so text remains readable (for example 1920x1080)
- Prefer PNG for screenshots with text

Visual themes
- Use a consistent theme across screenshots

Content guidelines
- Remove or redact sensitive information (real emails, passwords, connection strings, personal file paths)
- Use clearly labeled demo data (for example: demo@example.com)

### Tools for Screenshots

**Windows:**
- Snipping Tool (Win + Shift + S)
- ShareX (free, advanced features)
- Greenshot (free, with annotations)

**Mac:**
- Cmd + Shift + 4 (built-in)
- Cmd + Shift + 5 (screenshot toolbar)

**Extensions:**
- Awesome Screenshot (Chrome/Edge) - for full page captures

---

## Using Screenshots on LinkedIn

### LinkedIn Project Section

1. **Navigate to your profile → Add section → Featured → Add media**
2. **Upload 3-5 key screenshots:**
   - Swagger UI (must have)
   - GitHub Actions CI/CD (impressive)
   - Database schema (shows design skills)
   - Postman/API response (shows functionality)
   - Code example (shows quality)

3. **Add captions to each screenshot:**
   - "RESTful API with comprehensive Swagger documentation"
   - "Automated CI/CD pipeline with GitHub Actions"
   - "Clean Architecture with separate domain layers"
   - "JWT authentication with refresh token support"

### LinkedIn Post

Create a carousel post:

```
Excited to share my latest project: SmartHub

SmartHub is a production-ready .NET 8 Web API demonstrating clean architecture, secure authentication, and automated testing and CI workflows.

Add screenshots showing the API, architecture, CI status, and tests.

GitHub: https://github.com/Karinateii/SmartHub
```

---

## Priority Order

If you can only capture a few, prioritize these:

1. **Swagger UI Overview** (Essential - shows your API is documented)
2. **GitHub Actions CI/CD** (Very Impressive - shows DevOps skills)
3. **Solution Structure** (Shows architecture understanding)
4. **Tests Passing** (Shows quality focus)
5. **Database Schema** (Shows data modeling)

---

## 🔄 Updating Screenshots

**When to update:**
- After adding new features/endpoints
- After significant UI changes
- Before applying for jobs
- When API structure changes significantly

**Version control:**
- Don't commit large images to Git (use Git LFS or link to external hosting)
- Or: Keep file sizes under 500KB each
- Compress with tools like TinyPNG

---

## Next Steps

1. Run your application locally
2. Capture the recommended screenshots
3. Create the `screenshots/` folder in your repo
4. Update `README.md` to reference screenshots
5. Upload selected images to LinkedIn Featured section
6. Create a LinkedIn post showcasing the work

---

**Need help?** Open an issue or check the [README.md](./README.md) for setup instructions.

**Ready to showcase?** Follow this guide to prepare images for LinkedIn and your portfolio.
