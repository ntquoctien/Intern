[CmdletBinding()]
param(
    [ValidateRange(0, 10000)]
    [int]$Tail = 100,

    [ValidateSet("Error", "Warning", "All")]
    [string]$Level = "Error",

    [string]$Filter,

    [switch]$NoSessionLog
)

$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$runDir = Join-Path $root ".run"
$outLog = Join-Path $runDir "CareerService.out.log"
$errLog = Join-Path $runDir "CareerService.err.log"
$pidFile = Join-Path $runDir "CareerService.pid"
$sessionLog = Join-Path $runDir ("CareerService.monitor.{0}.log" -f (Get-Date -Format "yyyyMMdd-HHmmss"))

function Test-CareerServiceProcess {
    if (-not (Test-Path -LiteralPath $pidFile -PathType Leaf)) {
        return $false
    }

    $pidValue = Get-Content -LiteralPath $pidFile -ErrorAction SilentlyContinue | Select-Object -First 1
    if (-not $pidValue -or $pidValue -notmatch '^\d+$') {
        return $false
    }

    return $null -ne (Get-Process -Id ([int]$pidValue) -ErrorAction SilentlyContinue)
}

function Get-LogColor {
    param(
        [string]$Source,
        [string]$Message
    )

    if ($Source -eq "ERR" -or $Message -match '(?i)\b(critical|fatal|fail(?:ed|ure)?|error|exception)\b') {
        return "Red"
    }

    if ($Message -match '(?i)\bwarn(?:ing)?\b') {
        return "Yellow"
    }

    if ($Message -match '(?i)\b(started|listening|healthy|succeeded|success)\b') {
        return "Green"
    }

    return "Gray"
}

function Write-LogLine {
    param(
        [string]$Source,
        [AllowEmptyString()]
        [string]$Message
    )

    if ($Filter -and $Message -notmatch $Filter) {
        return
    }

    $record = "[{0}] [{1}] {2}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss.fff"), $Source, $Message
    Write-Host $record -ForegroundColor (Get-LogColor -Source $Source -Message $Message)

    if (-not $NoSessionLog) {
        Add-Content -LiteralPath $sessionLog -Value $record -Encoding UTF8
    }
}

function Test-LogLineVisible {
    param(
        [string]$Source,
        [string]$Message,
        [hashtable]$StreamState
    )

    if ($Level -eq "All") {
        return $true
    }

    # ASP.NET Core starts each log event with one of these level prefixes.
    # Continuation lines (SQL, exception messages and stack traces) inherit the
    # visibility of their event until the next prefix is encountered.
    $levelMatch = [regex]::Match($Message, '^\s*(trace|dbug|debug|info|warn|fail|crit|critical):\s', 'IgnoreCase')
    if ($levelMatch.Success) {
        $eventLevel = $levelMatch.Groups[1].Value.ToLowerInvariant()
        if ($Level -eq "Warning") {
            $StreamState[$Source] = $eventLevel -in @("warn", "fail", "crit", "critical")
        }
        else {
            $StreamState[$Source] = $eventLevel -in @("fail", "crit", "critical")
        }
    }
    elseif ($Source -eq "ERR" -or $Message -match '(?i)\b(unhandled exception|exception|fatal|failed|failure|error)\b') {
        # Some hosts write unhandled errors without an ASP.NET Core level prefix.
        $StreamState[$Source] = $true
    }

    return [bool]$StreamState[$Source]
}

New-Item -ItemType Directory -Path $runDir -Force | Out-Null

Write-Host "CareerService log monitor" -ForegroundColor Cyan
Write-Host "  stdout: $outLog"
Write-Host "  stderr: $errLog"
Write-Host "  level  : $Level"
if (-not $NoSessionLog) {
    Write-Host "  session: $sessionLog"
}
if ($Filter) {
    Write-Host "  filter : $Filter"
}
Write-Host "Press Ctrl+C to stop monitoring (the service will keep running)." -ForegroundColor DarkGray
Write-Host ""

if (-not (Test-CareerServiceProcess)) {
    Write-Warning "CareerService is not running yet. Waiting for Start-Dev.ps1 to create its log files..."
}

while (-not (Test-Path -LiteralPath $outLog -PathType Leaf) -or
       -not (Test-Path -LiteralPath $errLog -PathType Leaf)) {
    Start-Sleep -Milliseconds 500
}

$jobs = @()
$lastRunningState = Test-CareerServiceProcess
$streamState = @{ OUT = $false; ERR = $true }

try {
    $jobs += Start-Job -Name "CareerService-stdout" -ArgumentList $outLog, $Tail -ScriptBlock {
        param($Path, $TailCount)
        Get-Content -LiteralPath $Path -Tail $TailCount -Wait
    }
    $jobs += Start-Job -Name "CareerService-stderr" -ArgumentList $errLog, $Tail -ScriptBlock {
        param($Path, $TailCount)
        Get-Content -LiteralPath $Path -Tail $TailCount -Wait
    }

    while ($true) {
        foreach ($job in $jobs) {
            $source = if ($job.Name -eq "CareerService-stderr") { "ERR" } else { "OUT" }
            $lines = @(Receive-Job -Job $job -ErrorAction SilentlyContinue)
            foreach ($line in $lines) {
                $message = [string]$line
                if (Test-LogLineVisible -Source $source -Message $message -StreamState $streamState) {
                    Write-LogLine -Source $source -Message $message
                }
            }

            if ($job.State -eq "Failed") {
                throw "Log reader '$($job.Name)' stopped unexpectedly: $($job.ChildJobs[0].JobStateInfo.Reason)"
            }
        }

        $isRunning = Test-CareerServiceProcess
        if ($isRunning -ne $lastRunningState) {
            if ($isRunning) {
                Write-Host "[STATUS] CareerService is running." -ForegroundColor Green
            }
            else {
                Write-Host "[STATUS] CareerService stopped. Still waiting for new log entries..." -ForegroundColor Yellow
            }
            $lastRunningState = $isRunning
        }

        Start-Sleep -Milliseconds 200
    }
}
finally {
    if ($jobs.Count -gt 0) {
        $jobs | Stop-Job -ErrorAction SilentlyContinue
        $jobs | Remove-Job -Force -ErrorAction SilentlyContinue
    }

    Write-Host ""
    Write-Host "Stopped monitoring CareerService. The service was not stopped." -ForegroundColor Cyan
    if (-not $NoSessionLog) {
        Write-Host "Session log: $sessionLog"
    }
}
