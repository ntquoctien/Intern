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

# A failed start can overwrite a PID file while the previous apphost still owns
# the service port. Stop only the four exact development service process names
# so stale processes cannot keep DLLs locked or hide a newly-added endpoint.
@("IdentityService", "AcademicService", "ExamService", "CommunicationService") |
    ForEach-Object {
        $serviceName = $_
        Get-Process -Name $serviceName -ErrorAction SilentlyContinue |
            ForEach-Object {
                Stop-Process -Id $_.Id -Force
                Write-Host "Stopped stale $serviceName process (PID $($_.Id))."
            }
    }
