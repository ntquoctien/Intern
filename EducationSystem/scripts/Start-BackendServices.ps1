param(
    [switch]$NoBuild
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$runDir = Join-Path $root ".run"
New-Item -ItemType Directory -Path $runDir -Force | Out-Null

# Development services require one shared signing key. Keep it ephemeral when
# the caller has not supplied a real secret; never write it to the repository.
if ([string]::IsNullOrWhiteSpace($env:StudentJwt__SigningKey)) {
    $jwtBytes = New-Object byte[] 48
    $randomNumberGenerator = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    try {
        $randomNumberGenerator.GetBytes($jwtBytes)
    }
    finally {
        $randomNumberGenerator.Dispose()
    }
    $env:StudentJwt__SigningKey = [Convert]::ToBase64String($jwtBytes)
    Write-Host "Generated an ephemeral Student JWT signing key for this development run."
}

$services = @(
    @{ Name = "IdentityService"; Port = 5001 },
    @{ Name = "AcademicService"; Port = 5002 },
    @{ Name = "ExamService"; Port = 5003 },
    @{ Name = "CommunicationService"; Port = 5004 }
)

foreach ($service in $services) {
    $pidFile = Join-Path $runDir "$($service.Name).pid"
    if (Test-Path $pidFile) {
        $existingPid = Get-Content $pidFile -ErrorAction SilentlyContinue
        if ($existingPid) {
            $existingProcess = Get-CimInstance Win32_Process -Filter "ProcessId=$existingPid" -ErrorAction SilentlyContinue
            if ($existingProcess -and $existingProcess.CommandLine -like "*$($service.Name).csproj*") {
                try {
                    Invoke-WebRequest -Uri "http://localhost:$($service.Port)/swagger/v1/swagger.json" -UseBasicParsing -TimeoutSec 2 | Out-Null
                    Write-Host "$($service.Name) is already running on port $($service.Port)."
                    continue
                }
                catch {
                    Stop-Process -Id $existingPid -Force -ErrorAction SilentlyContinue
                }
            }
        }

        Remove-Item $pidFile -Force -ErrorAction SilentlyContinue
    }

    $project = Join-Path $root "src\Services\$($service.Name)\$($service.Name).csproj"
    $outLog = Join-Path $runDir "$($service.Name).out.log"
    $errLog = Join-Path $runDir "$($service.Name).err.log"

    if (-not $NoBuild) {
        Write-Host "Building $($service.Name)..."
        & dotnet build $project --no-restore
        if ($LASTEXITCODE -ne 0) {
            throw "Build failed for $($service.Name)."
        }
    }

    # Build explicitly above so startup never performs an implicit restore and
    # never launches an outdated service after new endpoints were added.
    $arguments = @("run", "--project", $project, "--launch-profile", "http", "--no-build")

    $process = Start-Process -FilePath "dotnet" `
        -ArgumentList $arguments `
        -RedirectStandardOutput $outLog `
        -RedirectStandardError $errLog `
        -PassThru `
        -WindowStyle Hidden

    Set-Content -Path $pidFile -Value $process.Id
    Write-Host "Started $($service.Name) on http://localhost:$($service.Port) (PID $($process.Id))."
}

Write-Host ""
Write-Host "Backend services are starting. Swagger URLs:"
foreach ($service in $services) {
    Write-Host "  http://localhost:$($service.Port)/swagger"
}
