# scripts/pre-commit.ps1
# Scans staged files for common secret patterns (Jwt keys, env var assignments) and aborts the commit if found.

$ErrorActionPreference = 'Stop'

$staged = git diff --cached --name-only
if (-not $staged) { exit 0 }

$forbidden = @()
$patternJwtKeyJson = '"Key"\s*:\s*"[A-Za-z0-9\-_.]{16,}"'
$patternEnvJwt = 'JWT_KEY\s*=\s*.+'
$patternEnvOther = '(ADMIN_PASSWORD|ADMIN_EMAIL)\s*=\s*.+|\bPASSWORD\b\s*[:=]\s*".+"'

foreach ($file in $staged) {
    if (-not (Test-Path $file)) { continue }
    $content = Get-Content -Raw -Path $file -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }

    if ($content -match $patternJwtKeyJson -or $content -match $patternEnvJwt -or $content -match $patternEnvOther) {
        $forbidden += $file
    }
}

if ($forbidden.Count -gt 0) {
    Write-Error "Potential secrets found in staged files:"
    $forbidden | ForEach-Object { Write-Error " - $_" }
    Write-Error "Aborting commit. Remove secrets from staged files and use environment variables or appsettings.example.json instead."
    exit 1
}

Write-Host "No obvious secrets detected in staged files. Proceeding with commit."
exit 0
