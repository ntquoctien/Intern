param(
    [switch]$Restart
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

try {
    & (Join-Path $PSScriptRoot "Start-VectorMatchService.ps1") -Restart:$Restart
}
catch {
    Write-Warning "VectorMatchService (AI Vector) not ready ($($_.Exception.Message)). Auto-fallback to course grade matching."
}

# Let dotnet build services that are not already running.
& (Join-Path $PSScriptRoot "Start-BackendServices.ps1")

Write-Host ""
Write-Host "Starting frontend at http://127.0.0.1:5173 ..."
Set-Location (Join-Path $root "education-system-ui")
npm run dev -- --host 127.0.0.1
