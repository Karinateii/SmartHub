# SmartHub Security Guide

Comprehensive security documentation for SmartHub.

## Authentication & Authorization

### JWT Implementation

SmartHub uses JSON Web Tokens (JWT) for stateless authentication:

**Token Structure:**
```
Header.Payload.Signature
```

**Payload Example:**
```json
{
  "sub": "550e8400-e29b-41d4-a716-446655440000",
  "email": "user@example.com",
  "roles": ["user"],
  "iat": 1705503000,
  "exp": 1705506600
}
```

**Security Measures:**
- Algorithm: HS256 (HMAC SHA-256)
- Secret: 32+ character random string
- Expiration: 60 minutes
- Refresh tokens: 7-day expiration
- Tokens stored in httpOnly cookies when possible

### Password Security

**Requirements:**
- Minimum 12 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character (!@#$%^&*)

**Hashing:**
- Algorithm: bcrypt
- Cost factor: 10
- Salt: Generated per password
- Never store plaintext passwords
- Hash comparison timing-safe

**Password Best Practices:**
1. Use unique passwords per service
2. Change passwords every 90 days
3. Don't reuse last 5 passwords
4. Use password manager (Bitwarden, 1Password)
5. Enable 2FA for critical accounts

### Role-Based Access Control (RBAC)

**Defined Roles:**

| Role | Permissions |
|------|------------|
| admin | Full access, user management, settings |
| editor | Create/edit/delete own resources |
| viewer | Read-only access |
| moderator | Edit/delete any resources, user moderation |

**Role Assignment:**
```json
{
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "roles": ["editor"],
  "assignedAt": "2024-01-17T10:30:00Z",
  "assignedBy": "admin-user-id"
}
```

## Input Validation & Sanitization

### Request Validation

All API inputs validated server-side:

```csharp
public class CreateResourceValidator : AbstractValidator<CreateResourceRequest>
{
    public CreateResourceValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .Length(1, 200)
            .Matches(@"^[a-zA-Z0-9\s\-._@()]+$");
            
        RuleFor(x => x.Email)
            .EmailAddress();
            
        RuleFor(x => x.Description)
            .MaximumLength(5000);
    }
}
```

### Output Sanitization

HTML/JavaScript stripped from user inputs:

```csharp
public static string Sanitize(string input)
{
    var sanitizer = new HtmlSanitizer();
    return sanitizer.Sanitize(input);
}
```

## SQL Injection Prevention

**Safe Practices:**

❌ **DON'T (Vulnerable):**
```csharp
string query = $"SELECT * FROM users WHERE email = '{email}'";
var result = db.Users.FromSql(query).ToList();
```

✅ **DO (Parameterized):**
```csharp
var result = db.Users
    .FromSqlInterpolated($"SELECT * FROM users WHERE email = {email}")
    .ToList();

// OR using LINQ
var result = db.Users
    .Where(u => u.Email == email)
    .ToList();
```

## XSS (Cross-Site Scripting) Prevention

**Content Security Policy Header:**
```
Content-Security-Policy: 
  default-src 'self';
  script-src 'self' trusted-cdn.com;
  style-src 'self' 'unsafe-inline';
  img-src 'self' data: https:;
  font-src 'self' data:
```

**HTML Escaping:**
```csharp
public static string HtmlEncode(string input)
{
    return System.Net.WebUtility.HtmlEncode(input);
}
```

## CSRF (Cross-Site Request Forgery) Prevention

**Anti-CSRF Tokens:**
```csharp
[ValidateAntiForgeryToken]
[HttpPost]
public IActionResult Create([FromForm] ResourceRequest model)
{
    // Process request
}
```

**Token in Response:**
```html
<form method="post" action="/api/resources">
    <input type="hidden" name="__RequestVerificationToken" 
           value="CfDJ8A-token-value">
</form>
```

## Rate Limiting

**Configuration:**
```json
{
  "RateLimit": {
    "Requests": 5000,
    "WindowMinutes": 60,
    "PerIp": 1000,
    "PerUser": 5000
  }
}
```

**Response Headers:**
```
X-RateLimit-Limit: 5000
X-RateLimit-Remaining: 4999
X-RateLimit-Reset: 1705510800
```

**Handling Rate Limits:**
```javascript
if (response.status === 429) {
    const retryAfter = response.headers.get('Retry-After');
    console.warn(`Rate limited. Retry after ${retryAfter} seconds`);
}
```

## HTTPS/TLS Configuration

**Enforce HTTPS:**
```csharp
app.UseHsts();
app.UseHttpsRedirection();
```

**Security Headers:**
```
Strict-Transport-Security: max-age=31536000; includeSubDomains
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
```

**Certificate Management:**
- Use Let's Encrypt for free certificates
- Auto-renewal every 90 days
- TLS 1.2 minimum
- Strong cipher suites only

## CORS Security

**Configuration:**
```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowSpecific", policy =>
    {
        policy
            .WithOrigins("https://example.com", "https://app.example.com")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Total-Count", "X-Page-Number");
    });
});
```

**Never use wildcards in production:**
```csharp
// ❌ DON'T DO THIS
policy.AllowAnyOrigin().AllowCredentials();
```

## Data Encryption

### In Transit
- HTTPS/TLS 1.2+
- Certificate pinning for mobile apps
- Encrypted VPN for sensitive operations

### At Rest
**Sensitive Fields:**
- Passwords (bcrypt hash)
- API keys (encrypted with AES-256)
- Payment info (never stored, use Stripe/PayPal)
- PII (encrypted)

```csharp
public class EncryptionService
{
    public string Encrypt(string plaintext)
    {
        using (var aes = Aes.Create())
        {
            aes.Key = _encryptionKey;
            aes.GenerateIV();
            
            using (var encryptor = aes.CreateEncryptor())
            using (var ms = new MemoryStream())
            {
                ms.Write(aes.IV, 0, aes.IV.Length);
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                {
                    using (var sw = new StreamWriter(cs))
                    {
                        sw.Write(plaintext);
                    }
                }
                return Convert.ToBase64String(ms.ToArray());
            }
        }
    }
}
```

## API Key Security

**Generation:**
```csharp
public class ApiKeyService
{
    public string GenerateKey()
    {
        const int keyLength = 32;
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] tokenBuffer = new byte[keyLength];
            rng.GetBytes(tokenBuffer);
            return Convert.ToBase64String(tokenBuffer);
        }
    }
}
```

**Storage:**
- Never store plaintext API keys
- Store hashed version using bcrypt
- Generate expiration dates
- Support key rotation

**Usage:**
```
Authorization: Bearer <your-api-key-here>
```

## Session Management

**Session Timeout:**
- Inactivity: 30 minutes
- Maximum: 8 hours
- Renewal: Automatic with activity

**Session Data:**
```json
{
  "sessionId": "sess_123456",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "createdAt": "2024-01-17T10:30:00Z",
  "expiresAt": "2024-01-17T11:30:00Z"
}
```

**Logout:**
```csharp
[Authorize]
[HttpPost]
public IActionResult Logout()
{
    _sessionService.InvalidateSession(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
    return Ok();
}
```

## Audit Logging

Track all sensitive operations:

```json
{
  "id": "audit_123456",
  "userId": "550e8400-e29b-41d4-a716-446655440000",
  "action": "LOGIN",
  "resource": "User",
  "changes": {
    "lastLoginAt": "2024-01-17T10:30:00Z"
  },
  "ipAddress": "192.168.1.1",
  "userAgent": "Mozilla/5.0...",
  "timestamp": "2024-01-17T10:30:00Z"
}
```

**Audit Events:**
- User login/logout
- Permission changes
- Resource creation/modification/deletion
- Password changes
- API key creation
- Failed login attempts

## Vulnerability Management

### Security Updates

1. Monitor security advisories
2. Test updates on staging
3. Apply patches within 48 hours for critical
4. Update dependencies monthly
5. Automated dependency scanning (Dependabot)

### Vulnerability Scanning

```bash
# .NET security analyzer
dotnet tool install -g security-scan

# Node.js vulnerability check
npm audit
npm audit fix

# Docker image scanning
trivy image our-registry/smarthub:latest
```

### Responsible Disclosure

Security researchers should email: security@smarthub.example.com

**No public disclosure without 30-day notice**

## Common Vulnerabilities Checklist

- [ ] No hardcoded secrets (API keys, passwords)
- [ ] No debug mode in production
- [ ] No verbose error messages to users
- [ ] No direct database access from frontend
- [ ] No directory listing enabled
- [ ] No outdated dependencies
- [ ] No default credentials
- [ ] No unvalidated redirects
- [ ] No external entity expansion (XXE)
- [ ] No deserialization of untrusted data

## Compliance

### Standards
- OWASP Top 10
- CWE Top 25
- GDPR (data protection)
- CCPA (California privacy)

### Certifications (Planned)
- SOC 2 Type II
- GDPR compliance
- ISO 27001

## Security Contacts

**Report Issues:** security@smarthub.example.com  
**Security Policy:** [SECURITY.md](SECURITY.md)  
**Responsible Disclosure:** 30-day notice required  

---

Last updated: 2024-01-17
