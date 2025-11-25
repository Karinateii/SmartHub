# setup-dev.ps1
# Generates a local appsettings.json from appsettings.example.json and optionally sets local environment variables

param(
    [string]$ExamplePath = "SmartHub.Api/appsettings.example.json",
    [string]$DestPath = "SmartHub.Api/appsettings.json",
    [switch]$SetEnvVars
)

Write-Host "Creating local appsettings.json from $ExamplePath -> $DestPath"
Copy-Item -Path $ExamplePath -Destination $DestPath -Force

if ($SetEnvVars) {
    $json = Get-Content -Raw -Path $ExamplePath | ConvertFrom-Json
    if ($json.Jwt -ne $null) {
        if ($json.Jwt.Key -ne $null -and $json.Jwt.Key -ne "REPLACE_WITH_SECURE_KEY") {
            $env:JWT_KEY = $json.Jwt.Key
            Write-Host "Set JWT_KEY from example (not recommended for production)"
        }
    }
    if ($json.Admin -ne $null) {
        if ($json.Admin.Email -ne $null) { $env:ADMIN_EMAIL = $json.Admin.Email }
        if ($json.Admin.Password -ne $null) { $env:ADMIN_PASSWORD = $json.Admin.Password }
    }
    Write-Host "Environment variables set in current PowerShell session (only)."
}

Write-Host "Developer setup complete. Remember to set secure env vars for production."