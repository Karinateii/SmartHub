# scripts/install-hooks.ps1
# Installs local git hooks (copy files in scripts/ to .git/hooks)
param(
    [string]$HookPath = ".git/hooks"
)

Write-Host "[INFO] Installing Git hooks from scripts/ to $HookPath"

if (-not (Test-Path $HookPath)) {
    Write-Host "[ERROR] Hook path $HookPath not found. Make sure you're in the repository root."
    exit 1
}

$preCommitScript = Join-Path $PSScriptRoot "pre-commit.ps1"
$preCommitHook = Join-Path $HookPath "pre-commit"

# Use forward-slash path to avoid sh escaping backslashes
$scriptPathForSh = $preCommitScript -replace '\\','/'

if (Test-Path $preCommitScript) {
    # Create a shim that runs the PowerShell script
    $shim = @"
#!/bin/sh
if command -v pwsh >/dev/null 2>&1; then
    pwsh -NoProfile -ExecutionPolicy Bypass -File '$scriptPathForSh' || exit 1
elif command -v powershell >/dev/null 2>&1; then
    powershell -NoProfile -ExecutionPolicy Bypass -File '$scriptPathForSh' || exit 1
else
    echo 'PowerShell not found. Please install PowerShell (https://aka.ms/pwsh) or run scripts/pre-commit.ps1 manually before committing.'
    exit 1
fi
"@
    try {
        Set-Content -Path $preCommitHook -Value $shim -NoNewline
        git update-index --add --chmod=+x $preCommitHook 2>$null
        if (Test-Path $preCommitHook) {
            Write-Host "[SUCCESS] Pre-commit hook installed to $preCommitHook."
        } else {
            Write-Host "[ERROR] Failed to install pre-commit hook. Please check permissions and try again."
            exit 1
        }
    } catch {
        Write-Host "[ERROR] Exception during hook installation: $_"
        exit 1
    }
} else {
    Write-Host "[ERROR] pre-commit script not found at $preCommitScript"
    exit 1
}

Write-Host "[INFO] Hook installation process complete."
