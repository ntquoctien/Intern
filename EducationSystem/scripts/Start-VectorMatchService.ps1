param(
    [switch]$Install,
    [switch]$Restart
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$serviceRoot = Join-Path $root "src\Services\VectorMatchService"
$venvRoot = Join-Path $serviceRoot ".venv"
$python = Join-Path $venvRoot "Scripts\python.exe"
$requirements = Join-Path $serviceRoot "requirements.txt"
$requirementsStamp = Join-Path $venvRoot ".requirements.sha256"
$environmentFile = Join-Path $serviceRoot ".env"
$developmentConnectionsFile = Join-Path $root "connectionstrings.Development.json"
$runDir = Join-Path $root ".run"
$pidFile = Join-Path $runDir "VectorMatchService.pid"
$outLog = Join-Path $runDir "VectorMatchService.out.log"
$errLog = Join-Path $runDir "VectorMatchService.err.log"
$healthUrl = "http://127.0.0.1:5006/health"

function Test-VectorMatchServiceHealth {
    try {
        $health = Invoke-RestMethod -Uri $healthUrl -TimeoutSec 2
        return $health.status -eq "Healthy"
    }
    catch {
        return $false
    }
}

New-Item -ItemType Directory -Path $runDir -Force | Out-Null

if (-not (Test-Path $environmentFile)) {
    throw "Create src\Services\VectorMatchService\.env from .env.example first."
}

function Convert-ToVectorOdbcConnectionString {
    param([Parameter(Mandatory = $true)][string]$ConnectionString)

    $sourceBuilder = [System.Data.Common.DbConnectionStringBuilder]::new()
    $sourceBuilder.set_ConnectionString($ConnectionString)

    $odbcBuilder = [System.Data.Odbc.OdbcConnectionStringBuilder]::new()
    $odbcBuilder.Driver = "ODBC Driver 18 for SQL Server"
    $serverVal = if ($sourceBuilder.ContainsKey("Server")) { $sourceBuilder["Server"] } else { "127.0.0.1" }
    $odbcBuilder["Server"] = $serverVal
    $odbcBuilder["Database"] = if ($sourceBuilder.ContainsKey("Database")) { $sourceBuilder["Database"] } else { "TayDoV2" }

    if ($sourceBuilder.ContainsKey("User Id") -and $sourceBuilder.ContainsKey("Password")) {
        $odbcBuilder["Uid"] = $sourceBuilder["User Id"]
        $odbcBuilder["Pwd"] = $sourceBuilder["Password"]
    } else {
        $odbcBuilder["Trusted_Connection"] = "yes"
    }
    $odbcBuilder["Encrypt"] = "no"
    $odbcBuilder["TrustServerCertificate"] = "yes"
    return $odbcBuilder.ConnectionString
}

# Each existing service login keeps its least-privilege schema access. Explicit
# process environment variables still take precedence in deployed environments.
if (Test-Path $developmentConnectionsFile) {
    $developmentConnections = Get-Content -LiteralPath $developmentConnectionsFile -Raw |
        ConvertFrom-Json
    if ([string]::IsNullOrWhiteSpace(
        $env:VECTOR_ACADEMIC_DB_CONNECTION_STRING
    )) {
        $env:VECTOR_ACADEMIC_DB_CONNECTION_STRING =
            Convert-ToVectorOdbcConnectionString `
                $developmentConnections.ConnectionStrings.AcademicDb
    }
    if ([string]::IsNullOrWhiteSpace(
        $env:VECTOR_CAREER_DB_CONNECTION_STRING
    )) {
        $env:VECTOR_CAREER_DB_CONNECTION_STRING =
            Convert-ToVectorOdbcConnectionString `
                $developmentConnections.ConnectionStrings.CareerDb
    }
}

if ($Restart) {
    if (Test-Path $pidFile) {
        $recordedPid = Get-Content -LiteralPath $pidFile -ErrorAction SilentlyContinue
        if ($recordedPid) {
            Stop-Process -Id $recordedPid -Force -ErrorAction SilentlyContinue
        }
    }

    # A Windows virtual-environment launcher can leave the actual Python child
    # owning the port after its recorded parent exits. Only clean up port 5006
    # when an explicit restart was requested.
    Get-NetTCPConnection -LocalPort 5006 -State Listen -ErrorAction SilentlyContinue |
        Select-Object -ExpandProperty OwningProcess -Unique |
        ForEach-Object { Stop-Process -Id $_ -Force -ErrorAction SilentlyContinue }

    Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
    Start-Sleep -Milliseconds 500
}

if (Test-Path $pidFile) {
    $existingPid = Get-Content $pidFile -ErrorAction SilentlyContinue
    if ($existingPid -and (Get-Process -Id $existingPid -ErrorAction SilentlyContinue)) {
        if (Test-VectorMatchServiceHealth) {
            Write-Host "VectorMatchService is ready on http://127.0.0.1:5006 (PID $existingPid)."
            Write-Host "  Swagger: http://127.0.0.1:5006/docs"
            return
        }

        Write-Host "VectorMatchService process $existingPid is not healthy; restarting it."
        Stop-Process -Id $existingPid -Force -ErrorAction SilentlyContinue
    }
    Remove-Item -LiteralPath $pidFile -Force -ErrorAction SilentlyContinue
}

if (-not (Test-Path $python) -or -not (Test-Path (Join-Path $venvRoot "pyvenv.cfg"))) {
    $pythonCandidates = @(
        (Join-Path $env:LOCALAPPDATA "Programs\Python\Python311\python.exe"),
        (Get-Command python3.11 -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue),
        (Get-Command python -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Source -ErrorAction SilentlyContinue)
    ) | Where-Object { $_ -and (Test-Path $_) }
    $basePython = $pythonCandidates | Where-Object {
        & $_ -c "import sys; raise SystemExit(0 if sys.version_info[:2] == (3, 11) else 1)" 2>$null
        $LASTEXITCODE -eq 0
    } | Select-Object -First 1
    if (-not $basePython) {
        throw "Python 3.11 was not found. Install it before starting VectorMatchService."
    }
    if (Test-Path $venvRoot) {
        $resolvedServiceRoot = (Resolve-Path $serviceRoot).Path
        $resolvedVenvRoot = (Resolve-Path $venvRoot).Path
        if (-not $resolvedVenvRoot.StartsWith($resolvedServiceRoot + [IO.Path]::DirectorySeparatorChar)) {
            throw "Refusing to replace a virtual environment outside VectorMatchService."
        }
        Remove-Item -LiteralPath $resolvedVenvRoot -Recurse -Force
    }
    Write-Host "Creating VectorMatchService virtual environment..."
    & $basePython -m venv $venvRoot
    if ($LASTEXITCODE -ne 0) {
        throw "Could not create the Python virtual environment."
    }
    $Install = $true
}

$requirementsHash = (Get-FileHash -LiteralPath $requirements -Algorithm SHA256).Hash
if (-not (Test-Path $requirementsStamp) -or
    (Get-Content -LiteralPath $requirementsStamp -Raw).Trim() -ne $requirementsHash) {
    $Install = $true
}

if ($Install) {
    Write-Host "Installing VectorMatchService dependencies..."
    & $python -m pip install -r $requirements
    if ($LASTEXITCODE -ne 0) {
        throw "Could not install VectorMatchService dependencies."
    }
    Set-Content -LiteralPath $requirementsStamp -Value $requirementsHash
}

$process = Start-Process -FilePath $python `
    -ArgumentList @("run.py") `
    -WorkingDirectory $serviceRoot `
    -RedirectStandardOutput $outLog `
    -RedirectStandardError $errLog `
    -PassThru `
    -WindowStyle Hidden

Set-Content -LiteralPath $pidFile -Value $process.Id
Write-Host "Started VectorMatchService process (PID $($process.Id)); waiting for health check..."

$ready = $false
foreach ($attempt in 1..120) {
    if (Test-VectorMatchServiceHealth) {
        $ready = $true
        break
    }
    Start-Sleep -Milliseconds 500
}

if (-not $ready) {
    throw "VectorMatchService did not become healthy. Check .run\VectorMatchService.err.log."
}

Write-Host "VectorMatchService is ready on http://127.0.0.1:5006."
Write-Host "  Swagger: http://127.0.0.1:5006/docs"
