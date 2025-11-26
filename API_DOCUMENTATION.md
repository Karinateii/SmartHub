# SmartHub API Documentation

## Base URL

```
Local Development: https://localhost:7xxx
Production: https://api.smarthub.com
```

## Authentication

SmartHub uses **JWT (JSON Web Token)** authentication with refresh token support.

### Token Format

Include the access token in the `Authorization` header:

```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

### Token Lifecycle

- **Access Token:** Valid for 60 minutes (configurable)
- **Refresh Token:** Valid for 7 days
- **Rotation:** Refresh tokens are rotated on each use

---

## Authentication Endpoints

### 1. Register New User

Create a new user account.

**Endpoint:** `POST /api/auth/register`  
**Rate Limit:** 3 requests per hour per IP  
**Authentication:** Not required

#### Request Body

```json
{
  "firstName": "string",
  "lastName": "string", 
  "email": "string",
  "password": "string",
  "confirmPassword": "string",
  "profileImageUrl": "string" // optional
}
```

#### Validation Rules

- `firstName`: Required, 2-50 characters
- `lastName`: Required, 2-50 characters
- `email`: Required, valid email format, unique
- `password`: Required, min 8 chars, must contain uppercase, lowercase, digit, and special character
- `confirmPassword`: Must match password

#### Success Response (201 Created)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "CfDJ8P+5K8mZ...",
  "expiresAt": "2025-11-26T15:30:00Z",
  "refreshTokenExpiry": "2025-12-03T14:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "User",
  "profileImageUrl": "https://example.com/image.jpg"
}
```

#### Error Responses

**400 Bad Request** - Validation failed
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Email": ["Email is required"],
    "Password": ["Password must be at least 8 characters"]
  }
}
```

**409 Conflict** - Email already exists
```json
{
  "error": "A user with the provided email already exists."
}
```

**429 Too Many Requests** - Rate limit exceeded
```json
{
  "error": "Too many requests"
}
```

---

### 2. Login

Authenticate with email and password.

**Endpoint:** `POST /api/auth/login`  
**Rate Limit:** 5 requests per minute per IP  
**Authentication:** Not required

#### Request Body

```json
{
  "email": "john.doe@example.com",
  "password": "SecurePass123!"
}
```

#### Validation Rules

- `email`: Required, valid email format
- `password`: Required

#### Success Response (200 OK)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "CfDJ8P+5K8mZ...",
  "expiresAt": "2025-11-26T15:30:00Z",
  "refreshTokenExpiry": "2025-12-03T14:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "User",
  "profileImageUrl": null
}
```

#### Error Responses

**400 Bad Request** - Invalid credentials
```json
{
  "error": "Invalid credentials."
}
```

**429 Too Many Requests**
```json
{
  "error": "Too many requests"
}
```

---

### 3. Refresh Token

Get a new access token using a refresh token.

**Endpoint:** `POST /api/auth/refresh`  
**Rate Limit:** 30 requests per minute per IP  
**Authentication:** Not required

#### Request Body

```json
{
  "refreshToken": "CfDJ8P+5K8mZ..."
}
```

#### Success Response (200 OK)

```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "NewRefreshToken...",
  "expiresAt": "2025-11-26T16:30:00Z",
  "refreshTokenExpiry": "2025-12-03T15:30:00Z",
  "userId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "fullName": "John Doe",
  "role": "User",
  "profileImageUrl": null
}
```

#### Error Responses

**400 Bad Request** - Invalid or expired token
```json
{
  "error": "Invalid or expired refresh token."
}
```

---

### 4. Logout

Revoke a refresh token (invalidates it).

**Endpoint:** `POST /api/auth/logout`  
**Rate Limit:** None  
**Authentication:** Required (Bearer token)

#### Request Body

```json
{
  "refreshToken": "CfDJ8P+5K8mZ..."
}
```

#### Success Response (204 No Content)

No response body.

#### Error Responses

**400 Bad Request** - Invalid token
```json
{
  "error": "Invalid refresh token."
}
```

**401 Unauthorized** - Missing or invalid access token
```json
{
  "error": "Unauthorized"
}
```

---

## Authentication Flow Diagram

```
┌─────────┐                                    ┌─────────┐
│ Client  │                                    │   API   │
└────┬────┘                                    └────┬────┘
     │                                              │
     │  1. POST /api/auth/register                 │
     │─────────────────────────────────────────────>│
     │     { email, password, ... }                 │
     │                                              │
     │  2. 201 Created                              │
     │<─────────────────────────────────────────────│
     │     { token, refreshToken, ... }             │
     │                                              │
     │  3. POST /api/protected-resource             │
     │─────────────────────────────────────────────>│
     │     Header: Authorization: Bearer <token>    │
     │                                              │
     │  4. 200 OK                                   │
     │<─────────────────────────────────────────────│
     │     { data }                                 │
     │                                              │
     │  5. [Token expires after 60 minutes]         │
     │                                              │
     │  6. POST /api/auth/refresh                   │
     │─────────────────────────────────────────────>│
     │     { refreshToken }                         │
     │                                              │
     │  7. 200 OK                                   │
     │<─────────────────────────────────────────────│
     │     { token: new, refreshToken: new }        │
     │                                              │
     │  8. POST /api/auth/logout                    │
     │─────────────────────────────────────────────>│
     │     { refreshToken }                         │
     │                                              │
     │  9. 204 No Content                           │
     │<─────────────────────────────────────────────│
     │                                              │
```

---

## JWT Token Structure

### Header
```json
{
  "alg": "HS256",
  "typ": "JWT"
}
```

### Payload (Claims)
```json
{
  "nameid": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "john.doe@example.com",
  "name": "John Doe",
  "role": "User",
  "nbf": 1732631400,
  "exp": 1732635000,
  "iss": "SmartHub",
  "aud": "SmartHubClient"
}
```

### Claims Explanation

| Claim | Description |
|-------|-------------|
| `nameid` | User's unique identifier (GUID) |
| `email` | User's email address |
| `name` | User's full name |
| `role` | User's role (User, Admin) |
| `nbf` | Not before timestamp |
| `exp` | Expiration timestamp |
| `iss` | Token issuer |
| `aud` | Token audience |

---

## User Endpoints

### 1. Get Current User

Get the authenticated user's profile.

**Endpoint:** `GET /api/users/me`  
**Authentication:** Required  
**Authorization:** Any authenticated user

#### Request Headers
```http
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
```

#### Success Response (200 OK)
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "role": "User",
  "profileImageUrl": null,
  "emailVerified": false,
  "createdAt": "2025-11-24T10:30:00Z",
  "updatedAt": "2025-11-24T10:30:00Z"
}
```

#### Error Responses

**401 Unauthorized**
```json
{
  "error": "Unauthorized"
}
```

---

## Health Check Endpoint

### Check API Health

**Endpoint:** `GET /health`  
**Authentication:** Not required

#### Success Response (200 OK)
```
Healthy
```

---

## Error Handling

### Global Error Response Format

All errors follow a consistent structure:

```json
{
  "error": "Error message description",
  "details": "Optional detailed error information"
}
```

### HTTP Status Codes

| Status Code | Description |
|-------------|-------------|
| 200 | OK - Request successful |
| 201 | Created - Resource created successfully |
| 204 | No Content - Request successful, no content to return |
| 400 | Bad Request - Invalid request data or validation failed |
| 401 | Unauthorized - Missing or invalid authentication |
| 403 | Forbidden - User doesn't have permission |
| 404 | Not Found - Resource not found |
| 409 | Conflict - Resource already exists |
| 429 | Too Many Requests - Rate limit exceeded |
| 500 | Internal Server Error - Server error occurred |

---

## Rate Limiting

SmartHub implements IP-based rate limiting to prevent abuse.

### Global Limits

**Default:** 100 requests per 15 minutes per IP

### Endpoint-Specific Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| POST /api/auth/register | 3 requests | 1 hour |
| POST /api/auth/login | 5 requests | 1 minute |
| POST /api/auth/refresh | 30 requests | 1 minute |

### Rate Limit Response Headers

```http
X-RateLimit-Limit: 5
X-RateLimit-Remaining: 3
X-RateLimit-Reset: 1732635000
```

### Rate Limit Exceeded Response (429)

```json
{
  "error": "Too many requests"
}
```

**Retry After:** Client should wait until the window resets (check `X-RateLimit-Reset` header).

---

## CORS Configuration

### Allowed Origins
- `https://localhost:3000` (Development)
- `https://app.smarthub.com` (Production)

### Allowed Methods
- GET
- POST
- PUT
- DELETE
- PATCH

### Allowed Headers
- Content-Type
- Authorization
- X-Requested-With

---

## Postman Collection

Import our Postman collection for easy API testing:

1. **Download:** [SmartHub.postman_collection.json](./postman/SmartHub.postman_collection.json)
2. **Import** into Postman
3. **Set environment variables:**
   - `base_url`: `https://localhost:7xxx`
   - `access_token`: (auto-populated after login)
   - `refresh_token`: (auto-populated after login)

---

## Example Integration (JavaScript)

### Register & Login

```javascript
// Register
const registerResponse = await fetch('https://localhost:7001/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    firstName: 'John',
    lastName: 'Doe',
    email: 'john.doe@example.com',
    password: 'SecurePass123!',
    confirmPassword: 'SecurePass123!'
  })
});

const authData = await registerResponse.json();
localStorage.setItem('accessToken', authData.token);
localStorage.setItem('refreshToken', authData.refreshToken);
```

### Authenticated Request

```javascript
const accessToken = localStorage.getItem('accessToken');

const response = await fetch('https://localhost:7001/api/users/me', {
  method: 'GET',
  headers: {
    'Authorization': `Bearer ${accessToken}`,
    'Content-Type': 'application/json'
  }
});

if (response.status === 401) {
  // Token expired, refresh it
  const refreshToken = localStorage.getItem('refreshToken');
  const refreshResponse = await fetch('https://localhost:7001/api/auth/refresh', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ refreshToken })
  });
  
  const newAuth = await refreshResponse.json();
  localStorage.setItem('accessToken', newAuth.token);
  localStorage.setItem('refreshToken', newAuth.refreshToken);
  
  // Retry original request
}

const userData = await response.json();
```

---

## Swagger UI

Interactive API documentation is available at:

```
https://localhost:7xxx/swagger
```

Features:
- ✅ Try out endpoints directly
- ✅ View request/response schemas
- ✅ Test authentication flow
- ✅ Export OpenAPI specification

---

## Versioning

**Current Version:** v1  
**API Version Header:** Not currently implemented  
**Future:** API versioning via URL (`/api/v1/`, `/api/v2/`) or header

---

## Support

- **Issues:** [GitHub Issues](https://github.com/Karinateii/SmartHub/issues)
- **Documentation:** [README.md](./README.md)
- **Architecture:** [ARCHITECTURE.md](./ARCHITECTURE.md)
- **Contributing:** [CONTRIBUTING.md](./CONTRIBUTING.md)

---

**Last Updated:** November 26, 2025  
**API Version:** 1.0
