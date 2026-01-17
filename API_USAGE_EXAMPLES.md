# SmartHub API Usage Examples

Real-world examples for SmartHub API integration.

## Authentication Flow

### 1. Register User

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "username": "john_doe",
    "password": "SecurePass123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

**JavaScript:**
```javascript
const response = await fetch('https://api.smarthub.example.com/api/auth/register', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({
    email: 'user@example.com',
    username: 'john_doe',
    password: 'SecurePass123!',
    firstName: 'John',
    lastName: 'Doe'
  })
});
const data = await response.json();
console.log(data.token); // JWT token
```

**Python:**
```python
import requests

response = requests.post(
    'https://api.smarthub.example.com/api/auth/register',
    json={
        'email': 'user@example.com',
        'username': 'john_doe',
        'password': 'SecurePass123!',
        'firstName': 'John',
        'lastName': 'Doe'
    }
)
token = response.json()['token']
```

### 2. Login

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePass123!"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600,
  "user": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "email": "user@example.com",
    "username": "john_doe",
    "roles": ["user"]
  }
}
```

## User Management

### Get Current User Profile

**cURL:**
```bash
curl -X GET https://api.smarthub.example.com/api/users/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
async function getProfile(token) {
  const response = await fetch('https://api.smarthub.example.com/api/users/me', {
    headers: {
      'Authorization': `Bearer ${token}`,
      'Content-Type': 'application/json'
    }
  });
  return response.json();
}
```

### Update Profile

**cURL:**
```bash
curl -X PUT https://api.smarthub.example.com/api/users/me \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "Jane",
    "lastName": "Smith",
    "email": "jane@example.com"
  }'
```

### Change Password

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/users/change-password \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "currentPassword": "SecurePass123!",
    "newPassword": "NewSecurePass456!"
  }'
```

## CRUD Operations

### Create Resource

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/resources \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "My Resource",
    "description": "Resource description",
    "type": "document",
    "metadata": {
      "category": "reports",
      "priority": "high"
    }
  }'
```

**Response (201 Created):**
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "name": "My Resource",
  "description": "Resource description",
  "type": "document",
  "createdAt": "2024-01-17T10:30:00Z",
  "updatedAt": "2024-01-17T10:30:00Z",
  "createdBy": "user123"
}
```

### Read Resource

**Get Single:**
```bash
curl -X GET https://api.smarthub.example.com/api/resources/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Get List with Pagination:**
```bash
curl -X GET "https://api.smarthub.example.com/api/resources?page=1&pageSize=10&sort=createdAt:desc" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**Response:**
```json
{
  "data": [
    {
      "id": "550e8400-e29b-41d4-a716-446655440000",
      "name": "Resource 1",
      "type": "document"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 10,
    "totalCount": 25,
    "totalPages": 3
  }
}
```

### Update Resource

**cURL:**
```bash
curl -X PUT https://api.smarthub.example.com/api/resources/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "name": "Updated Resource",
    "description": "Updated description"
  }'
```

### Delete Resource

**cURL:**
```bash
curl -X DELETE https://api.smarthub.example.com/api/resources/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Filtering and Searching

### Filter by Multiple Criteria

```bash
curl -X GET "https://api.smarthub.example.com/api/resources?type=document&status=active&createdAfter=2024-01-01" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

### Search

**cURL:**
```bash
curl -X GET "https://api.smarthub.example.com/api/resources/search?q=keyword&limit=20" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
async function searchResources(token, query) {
  const params = new URLSearchParams({ q: query, limit: 20 });
  const response = await fetch(
    `https://api.smarthub.example.com/api/resources/search?${params}`,
    {
      headers: { 'Authorization': `Bearer ${token}` }
    }
  );
  return response.json();
}
```

## Bulk Operations

### Bulk Create

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/resources/bulk \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "resources": [
      { "name": "Resource 1", "type": "document" },
      { "name": "Resource 2", "type": "document" }
    ]
  }'
```

### Bulk Delete

**cURL:**
```bash
curl -X DELETE https://api.smarthub.example.com/api/resources/bulk \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "ids": [
      "550e8400-e29b-41d4-a716-446655440000",
      "550e8400-e29b-41d4-a716-446655440001"
    ]
  }'
```

## Error Handling

### Handle Errors

**JavaScript:**
```javascript
async function apiCall(url, options = {}) {
  try {
    const response = await fetch(url, options);
    
    if (!response.ok) {
      const error = await response.json();
      throw new Error(`${error.message} (${response.status})`);
    }
    
    return await response.json();
  } catch (error) {
    console.error('API Error:', error.message);
    if (error.status === 401) {
      // Handle unauthorized - refresh token or redirect to login
    }
    throw error;
  }
}

// Usage
try {
  const data = await apiCall('https://api.smarthub.example.com/api/resources');
} catch (error) {
  // Handle error
}
```

### Common Error Responses

**400 Bad Request:**
```json
{
  "statusCode": 400,
  "message": "Invalid input",
  "errors": {
    "name": ["Name is required"],
    "email": ["Invalid email format"]
  }
}
```

**401 Unauthorized:**
```json
{
  "statusCode": 401,
  "message": "Invalid or expired token"
}
```

**404 Not Found:**
```json
{
  "statusCode": 404,
  "message": "Resource not found"
}
```

**429 Too Many Requests:**
```json
{
  "statusCode": 429,
  "message": "Rate limit exceeded",
  "retryAfter": 60
}
```

## File Upload

**JavaScript:**
```javascript
async function uploadFile(token, file) {
  const formData = new FormData();
  formData.append('file', file);
  formData.append('description', 'File description');
  
  const response = await fetch(
    'https://api.smarthub.example.com/api/files/upload',
    {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${token}`
      },
      body: formData
    }
  );
  
  return response.json();
}
```

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/files/upload \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -F "file=@/path/to/file.pdf" \
  -F "description=My document"
```

## Pagination

### Get Page 2, 20 Items

**cURL:**
```bash
curl -X GET "https://api.smarthub.example.com/api/resources?page=2&pageSize=20" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

**JavaScript:**
```javascript
async function getPage(token, page, pageSize = 20) {
  const params = new URLSearchParams({ page, pageSize });
  const response = await fetch(
    `https://api.smarthub.example.com/api/resources?${params}`,
    { headers: { 'Authorization': `Bearer ${token}` } }
  );
  return response.json();
}

// Get all pages
async function getAllItems(token) {
  let page = 1;
  let allItems = [];
  let hasMore = true;
  
  while (hasMore) {
    const result = await getPage(token, page);
    allItems.push(...result.data);
    hasMore = page < result.pagination.totalPages;
    page++;
  }
  
  return allItems;
}
```

## Sorting

**cURL:**
```bash
curl -X GET "https://api.smarthub.example.com/api/resources?sort=name:asc,createdAt:desc" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Rate Limiting

The API returns rate limit information in response headers:

```
X-RateLimit-Limit: 5000
X-RateLimit-Remaining: 4999
X-RateLimit-Reset: 1705510800
```

**JavaScript - Handle Rate Limiting:**
```javascript
async function apiCallWithRetry(url, options = {}, maxRetries = 3) {
  for (let i = 0; i < maxRetries; i++) {
    const response = await fetch(url, options);
    
    if (response.status === 429) {
      const retryAfter = parseInt(response.headers.get('Retry-After') || '60');
      await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));
      continue;
    }
    
    return response;
  }
  
  throw new Error('Max retries exceeded');
}
```

## Webhooks

### Register Webhook

**cURL:**
```bash
curl -X POST https://api.smarthub.example.com/api/webhooks \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "url": "https://your-app.example.com/webhooks/smarthub",
    "events": ["resource.created", "resource.updated", "resource.deleted"],
    "secret": "your-webhook-secret"
  }'
```

### Webhook Payload Example

```json
{
  "id": "evt_123456",
  "type": "resource.created",
  "timestamp": "2024-01-17T10:30:00Z",
  "data": {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "name": "New Resource",
    "type": "document"
  }
}
```

---

Last updated: 2024-01-17
