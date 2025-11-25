# Smoke test for SmartHub API
param(
    [string]$BaseUrl = "http://localhost:5000",
    [string]$UserEmail = "smoke.user+$(Get-Random)@example.com",
    [string]$UserPassword = "StrongP@ssw0rd!",
    [string]$AdminEmail = $env:ADMIN_EMAIL,
    [string]$AdminPassword = $env:ADMIN_PASSWORD
)

$ErrorActionPreference = 'Stop'

function Invoke-Json($Method, $Url, $BodyObj, $Headers) {
    $body = if ($BodyObj) { $BodyObj | ConvertTo-Json -Depth 5 } else { $null }
    if ($Headers) {
        return Invoke-RestMethod -Method $Method -Uri $Url -ContentType 'application/json' -Headers $Headers -Body $body
    } else {
        return Invoke-RestMethod -Method $Method -Uri $Url -ContentType 'application/json' -Body $body
    }
}

Write-Host "[INFO] BaseUrl: $BaseUrl"

try {
    # 1) Health check
    Write-Host "[STEP] GET /health"
    $health = Invoke-RestMethod -Method GET -Uri "$BaseUrl/health"
    Write-Host "[OK] Health responded"

    # 2) Register
    Write-Host "[STEP] POST /api/auth/register (email=$UserEmail)"
    $reg = Invoke-Json POST "$BaseUrl/api/auth/register" @{ email=$UserEmail; password=$UserPassword; firstName='Smoke'; lastName='User' } $null
    Write-Host "[OK] Registered user"

    # 3) Login
    Write-Host "[STEP] POST /api/auth/login"
    $login = Invoke-Json POST "$BaseUrl/api/auth/login" @{ email=$UserEmail; password=$UserPassword } $null
    if (-not $login.token) { throw "Login did not return token" }
    $userToken = $login.token
    Write-Host "[OK] Login token acquired"

    # 4) Refresh
    Write-Host "[STEP] POST /api/auth/refresh"
    $refresh = Invoke-Json POST "$BaseUrl/api/auth/refresh" @{ refreshToken = $login.refreshToken } $null
    if (-not $refresh.token) { throw "Refresh did not return token" }
    Write-Host "[OK] Refresh token acquired"

    # 5) Logout
    Write-Host "[STEP] POST /api/auth/logout"
    $logout = Invoke-Json POST "$BaseUrl/api/auth/logout" @{ refreshToken = $login.refreshToken } $null
    Write-Host "[OK] Logout"

    # 6) Optional admin users list
    if ($AdminEmail -and $AdminPassword) {
        Write-Host "[STEP] Admin login & GET /api/users"
        $adminLogin = Invoke-Json POST "$BaseUrl/api/auth/login" @{ email=$AdminEmail; password=$AdminPassword } $null
        $adminHeaders = @{ Authorization = "Bearer $($adminLogin.token)" }
        $users = Invoke-RestMethod -Method GET -Uri "$BaseUrl/api/users" -Headers $adminHeaders
        Write-Host "[OK] Users count: $($users.Count)"
    } else {
        Write-Host "[INFO] Admin credentials not provided. Skipping admin users list."
    }

    Write-Host "[SUCCESS] Smoke test completed"
    exit 0
}
catch {
    Write-Error "[FAIL] $_"
    exit 1
}
