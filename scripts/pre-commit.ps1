# scripts/pre-commit.ps1
# Scans staged files for common secret patterns (Jwt keys, env var assignments) and aborts the commit if found.

$ErrorActionPreference = 'Stop'

$staged = git diff --cached --name-only
if (-not $staged) { exit 0 }

# Ignore documentation files that contain example passwords/tokens but not real secrets
$ignoredExact = @('README.md', '.github/workflows/ci.yml', 'scripts/setup-dev.ps1', 'API_DOCUMENTATION.md', 'ARCHITECTURE.md', 'CONTRIBUTING.md', 'SCREENSHOTS.md', 'LINKEDIN_GUIDE.md', 'PORTFOLIO_READY.md')
$ignoredPrefix = @('.github/workflows/', 'bin/', 'obj/', 'screenshots/')

$forbidden = @()
$patternJwtKeyJson = '"Key"\s*:\s*"[A-Za-z0-9\-_.]{16,}"'
$patternEnvJwt = 'JWT_KEY\s*=\s*.+'
$patternEnvOther = '(ADMIN_PASSWORD|ADMIN_EMAIL)\s*=\s*.+|\bPASSWORD\b\s*[:=]\s*".+"'

foreach ($file in $staged) {
    # Ignore some known safe files and generated folders
    if ($ignoredExact -contains $file) { continue }
    foreach ($prefix in $ignoredPrefix) { if ($file.StartsWith($prefix)) { continue 2 } }
    if (-not (Test-Path $file)) { continue }
    $content = Get-Content -Raw -Path $file -ErrorAction SilentlyContinue
    if ($null -eq $content) { continue }

    # Allow referencing env/secret variables without hardcoding values
    $allowedEnvRef = $content -match 'JWT_KEY\s*=\s*\$\{?{?\s*secrets\.JWT_KEY\s*}?}?|JWT_KEY\s*=\s*\$JWT_KEY'

    if ($content -match $patternJwtKeyJson -or (($content -match $patternEnvJwt) -and -not $allowedEnvRef) -or $content -match $patternEnvOther) {
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
