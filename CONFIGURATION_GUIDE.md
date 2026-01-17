# SmartHub Configuration Guide

Complete configuration reference for SmartHub API.

## Environment Variables

### Core Settings

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:5000
DATABASE_URL=Server=localhost;Database=smarthub;User Id=sa;Password=<password>
JWT_SECRET=<your-secret-key-min-32-chars>
JWT_EXPIRATION_MINUTES=60
```

### Database

```
DATABASE_HOST=localhost
DATABASE_PORT=5432
DATABASE_NAME=smarthub
DATABASE_USER=postgres
DATABASE_PASSWORD=<password>
DATABASE_POOL_SIZE=20
CONNECTION_STRING=Host=localhost;Port=5432;Database=smarthub;Username=postgres;Password=<password>
```

### Authentication

```
JWT_SECRET=your-256-bit-secret-key-here-min-32-chars
JWT_ISSUER=https://smarthub.example.com
JWT_AUDIENCE=smarthub-app
JWT_EXPIRATION_MINUTES=60
REFRESH_TOKEN_EXPIRATION_DAYS=7
PASSWORD_HASH_ITERATIONS=10000
```

### Email Configuration

```
SMTP_HOST=smtp.gmail.com
SMTP_PORT=587
SMTP_USERNAME=your-email@gmail.com
SMTP_PASSWORD=<app-specific-password>
SMTP_FROM_NAME=SmartHub
SMTP_FROM_EMAIL=noreply@smarthub.example.com
SMTP_USE_TLS=true
```

### API Rate Limiting

```
RATE_LIMIT_REQUESTS=5000
RATE_LIMIT_WINDOW_MINUTES=60
RATE_LIMIT_PER_IP=100
RATE_LIMIT_PER_USER=500
```

### Logging

```
LOG_LEVEL=Information
LOG_FILE_PATH=/var/log/smarthub/app.log
LOG_FILE_SIZE_LIMIT_MB=100
LOG_FILE_RETENTION_DAYS=30
LOG_DETAILED_ERRORS=false
```

### Security

```
CORS_ALLOWED_ORIGINS=https://example.com,https://app.example.com
CORS_ALLOW_CREDENTIALS=true
HTTPS_REDIRECT=true
SECURITY_HEADERS_ENABLED=true
HELMET_ENABLED=true
```

### Third-Party Services

```
# Stripe (if payment processing)
STRIPE_API_KEY=sk_live_<key>
STRIPE_WEBHOOK_SECRET=whsec_<secret>

# Slack (if notifications)
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/...
SLACK_NOTIFICATIONS_ENABLED=true

# SendGrid (alternative email)
SENDGRID_API_KEY=SG.<key>
```

### Cache Settings

```
CACHE_TYPE=Redis
REDIS_CONNECTION_STRING=localhost:6379
REDIS_DATABASE=0
REDIS_PASSWORD=<password>
CACHE_EXPIRATION_MINUTES=30
CACHE_SLIDING_EXPIRATION=true
```

### File Storage

```
STORAGE_TYPE=Local|S3|Azure
STORAGE_PATH=/var/data/uploads
AWS_S3_BUCKET=smarthub-uploads
AWS_S3_REGION=us-east-1
AWS_ACCESS_KEY_ID=<key>
AWS_SECRET_ACCESS_KEY=<secret>
```

## Configuration Files

### appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "System": "Warning"
    }
  },
  "Jwt": {
    "Secret": "your-256-bit-secret-key-here",
    "Issuer": "https://smarthub.example.com",
    "Audience": "smarthub-app",
    "ExpirationMinutes": 60
  },
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=smarthub;Username=postgres;Password=password"
  },
  "Cors": {
    "AllowedOrigins": [
      "https://example.com",
      "https://app.example.com"
    ]
  },
  "RateLimit": {
    "Requests": 5000,
    "WindowMinutes": 60
  }
}
```

### appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "Https": {
    "Port": 443
  }
}
```

### appsettings.Development.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Debug"
    }
  },
  "DebugMode": true
}
```

## Docker Configuration

### docker-compose.yml

```yaml
version: '3.8'

services:
  app:
    build: .
    ports:
      - "5000:5000"
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      DATABASE_URL: Host=db;Port=5432;Database=smarthub;Username=postgres;Password=postgres
      JWT_SECRET: your-secret-key-here
    depends_on:
      - db
      - cache
    networks:
      - smarthub

  db:
    image: postgres:15
    environment:
      POSTGRES_DB: smarthub
      POSTGRES_PASSWORD: postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - smarthub

  cache:
    image: redis:7
    ports:
      - "6379:6379"
    networks:
      - smarthub

volumes:
  postgres_data:

networks:
  smarthub:
    driver: bridge
```

## Dockerfile Configuration

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SmartHub.csproj", "."]
RUN dotnet restore "SmartHub.csproj"
COPY . .
RUN dotnet build "SmartHub.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build /app/build .
EXPOSE 5000
ENV ASPNETCORE_ENVIRONMENT=Production
ENTRYPOINT ["dotnet", "SmartHub.dll"]
```

## Nginx Configuration

### nginx.conf

```nginx
upstream smarthub {
    server app:5000;
}

server {
    listen 80;
    server_name smarthub.example.com;
    
    return 301 https://$server_name$request_uri;
}

server {
    listen 443 ssl http2;
    server_name smarthub.example.com;
    
    ssl_certificate /etc/nginx/ssl/cert.pem;
    ssl_certificate_key /etc/nginx/ssl/key.pem;
    
    location / {
        proxy_pass http://smarthub;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
```

## Configuration by Environment

### Development

- Debug mode: ON
- HTTPS: Optional
- Rate limiting: Higher (for testing)
- Logging: Verbose (Debug level)
- CORS: Allow all origins
- Database: Local PostgreSQL
- Cache: In-memory or local Redis

### Staging

- Debug mode: OFF
- HTTPS: Required
- Rate limiting: Production levels
- Logging: Information level
- CORS: Staging domain only
- Database: Staging PostgreSQL
- Cache: Redis on staging server

### Production

- Debug mode: OFF
- HTTPS: Required with modern TLS
- Rate limiting: Full enforcement
- Logging: Warning level
- CORS: Production domain only
- Database: Production PostgreSQL with replication
- Cache: Redis cluster
- Monitoring: All metrics enabled
- Backups: Automated hourly

## Security Configuration

### Password Requirements

```
Minimum Length: 12 characters
Require Uppercase: Yes
Require Lowercase: Yes
Require Numbers: Yes
Require Special Characters: Yes
Expiration: 90 days
History: Last 5 passwords not allowed
```

### JWT Configuration

```
Algorithm: HS256
Secret Min Length: 32 characters
Expiration: 60 minutes (default)
Refresh Token: 7 days
Issuer: https://smarthub.example.com
Audience: smarthub-app
```

### CORS Settings

```
Allowed Methods: GET, POST, PUT, DELETE, PATCH
Allowed Headers: Content-Type, Authorization
Exposed Headers: X-Total-Count, X-Page-Number
Allow Credentials: true
Max Age: 3600 seconds
```

## Performance Configuration

### Database Connection Pool

```
Min Pool Size: 5
Max Pool Size: 20
Connection Timeout: 30 seconds
Command Timeout: 30 seconds
```

### Cache Settings

```
Default TTL: 30 minutes
Sliding Expiration: true
Cache Key Prefix: smarthub:
Cache Size Limit: 100 MB
```

### API Rate Limiting

```
Authenticated Users: 5000 requests/hour
Unauthenticated: 100 requests/hour
Per IP: 1000 requests/hour
Burst Size: 50 requests
```

## Monitoring Configuration

### Logging

```
Log Level: Information
Minimum: Warning
File Rotation: Daily
Retention: 30 days
Max File Size: 100 MB
```

### Metrics

```
Enable Prometheus: true
Enable Health Checks: true
Enable Request Logging: true
Sample Rate: 100%
```

## Configuration Validation

On startup, SmartHub validates:

1. Database connection
2. JWT secret length (min 32 chars)
3. Required email settings
4. CORS origins format
5. Cache connectivity
6. File storage path accessibility
7. Log file path permissions
8. Rate limit values (positive integers)

If validation fails, startup logs error and application exits.

---

Last updated: 2024-01-17
