$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$runDir = Join-Path $root ".run"

if (-not (Test-Path $runDir)) {
    Write-Host "No .run directory found."
    return
}

Get-ChildItem -Path $runDir -Filter "*.pid" | ForEach-Object {
    $pidValue = Get-Content $_.FullName -ErrorAction SilentlyContinue
    if ($pidValue) {
        $process = Get-Process -Id $pidValue -ErrorAction SilentlyContinue
        if ($process) {
            Stop-Process -Id $process.Id -Force
            Write-Host "Stopped $($_.BaseName) (PID $pidValue)."
        }
    }

    Remove-Item $_.FullName -Force -ErrorAction SilentlyContinue
}
