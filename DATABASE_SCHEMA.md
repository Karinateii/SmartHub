# SmartHub Database Schema

Complete database schema documentation for SmartHub.

## Overview

SmartHub uses PostgreSQL with the following core tables:

- **Users** - Application users
- **Roles** - User roles and permissions
- **Entities** - Generic business entities
- **Audit Logs** - Change tracking and audit trail
- **API Keys** - Authentication tokens
- **Sessions** - User sessions

## Entity Relationship Diagram

```
┌─────────────┐
│   Users     │
├─────────────┤
│ id (PK)     │
│ email       │──────┐
│ password    │      │
│ created_at  │      │
└─────────────┘      │
       │             │
       └─────────────┼──────┐
                     │      │
              ┌──────┴─────┐│
              │    Roles   ││
              ├────────────┤│
              │ id (PK)    ││
              │ name       ││
              │ created_at ││
              └────────────┘│
                            │
                    ┌───────┴────────┐
                    │  API Keys      │
                    ├────────────────┤
                    │ id (PK)        │
                    │ user_id (FK)   │
                    │ key_hash       │
                    │ created_at     │
                    └────────────────┘
```

## Tables

### Users Table

Stores user account information.

```sql
CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(255) NOT NULL UNIQUE,
  username VARCHAR(100) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  avatar_url VARCHAR(512),
  is_active BOOLEAN DEFAULT true,
  email_verified BOOLEAN DEFAULT false,
  email_verified_at TIMESTAMP,
  last_login_at TIMESTAMP,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
  deleted_at TIMESTAMP,
  
  CONSTRAINT email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$'),
  INDEX idx_users_email (email),
  INDEX idx_users_created_at (created_at DESC)
);
```

**Columns:**
- `id` - Unique identifier (UUID)
- `email` - User email (must be unique)
- `username` - Display name (must be unique)
- `password_hash` - Hashed password (bcrypt)
- `first_name` - User's first name
- `last_name` - User's last name
- `avatar_url` - Profile picture URL
- `is_active` - Account status
- `email_verified` - Email confirmation status
- `email_verified_at` - When email was verified
- `last_login_at` - Last login timestamp
- `created_at` - Account creation date
- `updated_at` - Last modification date
- `deleted_at` - Soft delete timestamp

### Roles Table

Stores user roles and permissions.

```sql
CREATE TABLE roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  name VARCHAR(100) NOT NULL UNIQUE,
  description TEXT,
  is_system BOOLEAN DEFAULT false,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
  
  INDEX idx_roles_name (name)
);
```

**Pre-defined Roles:**
- `admin` - Full system access
- `user` - Standard user access
- `viewer` - Read-only access
- `moderator` - Moderation capabilities

### User Roles Junction Table

```sql
CREATE TABLE user_roles (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  role_id UUID NOT NULL REFERENCES roles(id) ON DELETE CASCADE,
  assigned_at TIMESTAMP NOT NULL DEFAULT NOW(),
  assigned_by UUID REFERENCES users(id),
  
  UNIQUE(user_id, role_id),
  INDEX idx_user_roles_user_id (user_id),
  INDEX idx_user_roles_role_id (role_id)
);
```

### API Keys Table

Stores authentication tokens for API access.

```sql
CREATE TABLE api_keys (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  key_hash VARCHAR(64) NOT NULL UNIQUE,
  name VARCHAR(100),
  description TEXT,
  last_used_at TIMESTAMP,
  expires_at TIMESTAMP,
  is_active BOOLEAN DEFAULT true,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
  
  CONSTRAINT key_length CHECK (LENGTH(key_hash) = 64),
  INDEX idx_api_keys_user_id (user_id),
  INDEX idx_api_keys_expires_at (expires_at),
  INDEX idx_api_keys_active (is_active)
);
```

**Columns:**
- `id` - Unique identifier
- `user_id` - Reference to user
- `key_hash` - SHA-256 hash of actual key
- `name` - Friendly name
- `description` - Usage description
- `last_used_at` - Last API call timestamp
- `expires_at` - Expiration date (null = never)
- `is_active` - Enable/disable status

### Sessions Table

```sql
CREATE TABLE sessions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash VARCHAR(64) NOT NULL,
  ip_address INET,
  user_agent TEXT,
  expires_at TIMESTAMP NOT NULL,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  
  INDEX idx_sessions_user_id (user_id),
  INDEX idx_sessions_expires_at (expires_at),
  INDEX idx_sessions_token_hash (token_hash)
);
```

### Audit Logs Table

```sql
CREATE TABLE audit_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID REFERENCES users(id) ON DELETE SET NULL,
  entity_type VARCHAR(100) NOT NULL,
  entity_id UUID NOT NULL,
  action VARCHAR(50) NOT NULL,
  changes JSONB,
  ip_address INET,
  user_agent TEXT,
  created_at TIMESTAMP NOT NULL DEFAULT NOW(),
  
  INDEX idx_audit_logs_user_id (user_id),
  INDEX idx_audit_logs_entity (entity_type, entity_id),
  INDEX idx_audit_logs_created_at (created_at DESC)
);
```

**Columns:**
- `id` - Log entry ID
- `user_id` - Who made the change
- `entity_type` - Type of changed entity
- `entity_id` - ID of changed entity
- `action` - Action type (CREATE, UPDATE, DELETE)
- `changes` - JSONB with before/after values
- `ip_address` - Request IP
- `user_agent` - Browser/client info
- `created_at` - When change occurred

**Changes JSONB Example:**
```json
{
  "before": {
    "email": "old@example.com",
    "is_active": true
  },
  "after": {
    "email": "new@example.com",
    "is_active": true
  }
}
```

## Indexes

### Performance Indexes

```sql
-- Frequently queried fields
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_username ON users(username);
CREATE INDEX idx_users_is_active ON users(is_active);

-- Sorting
CREATE INDEX idx_users_created_at ON users(created_at DESC);
CREATE INDEX idx_sessions_expires_at ON sessions(expires_at);

-- Composite indexes for common queries
CREATE INDEX idx_user_roles_composite ON user_roles(user_id, role_id);
CREATE INDEX idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
```

### Foreign Key Indexes

Automatically created for foreign key columns.

## Constraints

### Primary Keys

All tables have UUID primary keys:
```sql
id UUID PRIMARY KEY DEFAULT gen_random_uuid()
```

### Unique Constraints

```sql
-- Email must be unique
UNIQUE(email)

-- Username must be unique
UNIQUE(username)

-- Role name must be unique
UNIQUE(name)

-- API key hash must be unique
UNIQUE(key_hash)

-- User can't have same role twice
UNIQUE(user_id, role_id)
```

### Foreign Keys

```sql
-- User roles reference users and roles
user_id UUID REFERENCES users(id) ON DELETE CASCADE
role_id UUID REFERENCES roles(id) ON DELETE CASCADE

-- API keys reference users
user_id UUID REFERENCES users(id) ON DELETE CASCADE

-- Sessions reference users
user_id UUID REFERENCES users(id) ON DELETE CASCADE

-- Audit logs reference users (nullable)
user_id UUID REFERENCES users(id) ON DELETE SET NULL
```

### Check Constraints

```sql
-- Email format validation
CONSTRAINT email_format CHECK (email ~* '^[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Z|a-z]{2,}$')

-- API key length
CONSTRAINT key_length CHECK (LENGTH(key_hash) = 64)
```

## Typical Queries

### Find User with Roles

```sql
SELECT 
  u.id, 
  u.email, 
  u.username,
  ARRAY_AGG(r.name) as roles
FROM users u
LEFT JOIN user_roles ur ON u.id = ur.user_id
LEFT JOIN roles r ON ur.role_id = r.id
WHERE u.id = $1
GROUP BY u.id;
```

### Get User's API Keys

```sql
SELECT *
FROM api_keys
WHERE user_id = $1
  AND is_active = true
  AND (expires_at IS NULL OR expires_at > NOW())
ORDER BY created_at DESC;
```

### Audit Trail for Entity

```sql
SELECT *
FROM audit_logs
WHERE entity_type = 'users'
  AND entity_id = $1
ORDER BY created_at DESC
LIMIT 50;
```

### Active Sessions

```sql
SELECT *
FROM sessions
WHERE user_id = $1
  AND expires_at > NOW()
ORDER BY created_at DESC;
```

## Data Types

- **UUID** - Unique identifiers (gen_random_uuid())
- **VARCHAR** - Text with length limit
- **TEXT** - Unlimited text
- **BOOLEAN** - True/false values
- **TIMESTAMP** - Date and time with timezone
- **INET** - IPv4/IPv6 addresses
- **JSONB** - JSON binary format (indexable)

## Migrations

Create migrations for schema changes:

```bash
npm run migrate:create -- add_users_table
```

Migration file: `migrations/YYYYMMDDHHMMSS_add_users_table.js`

```sql
-- Up migration
CREATE TABLE users (
  ...
);

-- Down migration (rollback)
DROP TABLE users;
```

## Maintenance

### Vacuum (Cleanup)

```sql
-- Full vacuum
VACUUM ANALYZE users;

-- Simple vacuum
VACUUM;
```

### Reindex

```sql
-- Reindex table
REINDEX TABLE users;

-- Reindex all
REINDEX DATABASE codexray;
```

### Backup and Restore

```bash
# Backup
pg_dump codexray_prod > backup.sql

# Restore
psql codexray_prod < backup.sql
```

## Security

### Encryption

- Passwords stored as bcrypt hashes
- API keys stored as SHA-256 hashes
- Sensitive data encrypted at rest

### Row-Level Security (RLS)

```sql
-- Enable RLS
ALTER TABLE users ENABLE ROW LEVEL SECURITY;

-- Only users can see their own data
CREATE POLICY user_isolation ON users
  FOR SELECT
  USING (id = current_user_id());
```

## Performance Tips

1. **Use UUIDs** - Better for distributed systems
2. **Index frequently queried fields** - Speeds up queries
3. **Soft deletes with deleted_at** - Preserve data
4. **JSONB for audit logs** - Flexible schema
5. **Timestamps for auditing** - Track changes
6. **Foreign keys with CASCADE** - Data integrity

## Monitoring

### Query Performance

```sql
-- Enable query logging
SET log_min_duration_statement = 1000;  -- Log slow queries (1s+)

-- Analyze query plan
EXPLAIN ANALYZE SELECT ...;
```

### Table Sizes

```sql
-- Table sizes
SELECT 
  table_name,
  pg_size_pretty(pg_total_relation_size(table_name::regclass)) AS size
FROM information_schema.tables
WHERE table_schema = 'public'
ORDER BY pg_total_relation_size(table_name::regclass) DESC;
```

---

Last updated: 2024-01-17
