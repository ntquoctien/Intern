$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot

& (Join-Path $PSScriptRoot "Start-BackendServices.ps1") -NoBuild

Write-Host ""
Write-Host "Starting frontend at http://127.0.0.1:5173 ..."
Set-Location (Join-Path $root "education-system-ui")
npm run dev -- --host 127.0.0.1
