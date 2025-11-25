# scripts/install-hooks.ps1
# Installs local git hooks (copy files in scripts/ to .git/hooks)
param(
    [string]$HookPath = ".git/hooks"
)

Write-Host "Installing Git hooks from scripts/ to $HookPath"

if (-not (Test-Path $HookPath)) {
    Write-Error "Hook path $HookPath not found. Make sure you're in the repository root."
    exit 1
}

$preCommitScript = Join-Path $PSScriptRoot "pre-commit.ps1"
$preCommitHook = Join-Path $HookPath "pre-commit"

if (Test-Path $preCommitScript) {
    # Create a shim that runs the PowerShell script
    $shim = @"#!/bin/sh
pwsh -NoProfile -ExecutionPolicy Bypass -File \"$preCommitScript\" || exit 1
"@
    Set-Content -Path $preCommitHook -Value $shim -NoNewline
    git update-index --add --chmod=+x $preCommitHook
    Write-Host "Installed pre-commit hook to $preCommitHook"
} else {
    Write-Error "pre-commit script not found at $preCommitScript"
    exit 1
}

Write-Host "Hook installation complete."
