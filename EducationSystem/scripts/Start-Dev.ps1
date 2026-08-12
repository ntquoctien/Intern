param(
    [switch]$Restart
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "Start-VectorMatchService.ps1") -Restart:$Restart

# Let dotnet build services that are not already running. Using --no-build here
# makes a clean checkout (or a cleaned bin folder) fail silently because the
# service executable does not exist, and can also keep newly-added endpoints
# out of the running application.
& (Join-Path $PSScriptRoot "Start-BackendServices.ps1")

Write-Host ""
Write-Host "Starting frontend at http://127.0.0.1:5173 ..."
Set-Location (Join-Path $root "education-system-ui")
npm run dev -- --host 127.0.0.1
