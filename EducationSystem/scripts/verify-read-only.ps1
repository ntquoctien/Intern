[CmdletBinding()]
param(
    [switch]$VerifyDatabase
)

$ErrorActionPreference = "Stop"
$repositoryRoot = (Resolve-Path (Join-Path $PSScriptRoot "..")).Path
$baselinePath = Join-Path $repositoryRoot "docs\migration-baseline.txt"

$expectedMigrations = Get-Content -LiteralPath $baselinePath |
    Where-Object { $_ -and -not $_.StartsWith("#") } |
    Sort-Object
$actualMigrations = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot "src\Services") -Recurse -File |
    Where-Object { $_.DirectoryName -like "*\Migrations" } |
    ForEach-Object { $_.FullName.Substring($repositoryRoot.Length + 1).Replace("\", "/") } |
    Sort-Object

$migrationDiff = Compare-Object $expectedMigrations $actualMigrations
if ($migrationDiff) {
    throw "Migration baseline changed. This read-only change must not add, remove, or rename migration/schema artifacts.`n$($migrationDiff | Out-String)"
}

$featureFiles = Get-ChildItem -LiteralPath (Join-Path $repositoryRoot "src") -Recurse -Include *.cs -File |
    Where-Object {
        $_.FullName -match "Student|Management|ReadOnly" -and
        $_.FullName -notmatch "\\Migrations\\"
    }
$forbiddenWritePattern = '\b(SaveChanges|SaveChangesAsync|ExecuteUpdate|ExecuteUpdateAsync|ExecuteDelete|ExecuteDeleteAsync|ExecuteSqlRaw|ExecuteSqlInterpolated|FromSqlRaw)\s*\(|\b_dbContext(?:\.\w+)?\s*\.\s*(Add|AddAsync|AddRange|Update|UpdateRange|Remove|RemoveRange)\s*\('
$writeHits = $featureFiles | Select-String -Pattern $forbiddenWritePattern
if ($writeHits) {
    throw "Forbidden EF/database write API found in the read-only feature scope:`n$($writeHits | Out-String)"
}

$controllerFiles = $featureFiles | Where-Object { $_.DirectoryName -like "*\Controllers*" }
$forbiddenVerbHits = $controllerFiles | Select-String -Pattern '\[(HttpPut|HttpPatch|HttpDelete)(Attribute)?\b|\[HttpPost(?!\("login"\))'
if ($forbiddenVerbHits) {
    throw "Forbidden HTTP mutation action found in the read-only feature scope:`n$($forbiddenVerbHits | Out-String)"
}

Write-Host "Read-only verification passed: migration baseline unchanged and no forbidden write path found."

if ($VerifyDatabase) {
    $connectionFile = Join-Path $repositoryRoot "connectionstrings.Development.json"
    $configuration = Get-Content -LiteralPath $connectionFile -Raw | ConvertFrom-Json
    $connectionString = [string]$configuration.ConnectionStrings.IdentityDb
    $server = [regex]::Match($connectionString, '(?:Server|Data Source)=([^;]+)', 'IgnoreCase').Groups[1].Value
    $database = [regex]::Match($connectionString, '(?:Database|Initial Catalog)=([^;]+)', 'IgnoreCase').Groups[1].Value
    if (-not $server -or -not $database) {
        throw "Cannot resolve Server and Database from the development connection string."
    }

    $query = "SET NOCOUNT ON; SELECT s.name, t.name, SUM(p.rows) FROM sys.tables t JOIN sys.schemas s ON s.schema_id=t.schema_id JOIN sys.partitions p ON p.object_id=t.object_id AND p.index_id IN (0,1) WHERE s.name IN ('identity','academic','exam','communication') GROUP BY s.name,t.name ORDER BY s.name,t.name;"
    $actualRows = & sqlcmd -S $server -d $database -E -C -h -1 -W -s '|' -Q $query
    if ($LASTEXITCODE -ne 0) {
        throw "SELECT-only row-count query failed."
    }

    $expectedRows = Get-Content -LiteralPath (Join-Path $repositoryRoot "docs\database-row-baseline.txt") |
        Where-Object { $_ -and -not $_.StartsWith("#") }
    $rowDiff = Compare-Object ($expectedRows | Sort-Object) ($actualRows | Where-Object { $_ } | Sort-Object)
    if ($rowDiff) {
        throw "Database row counts differ from the immutable baseline.`n$($rowDiff | Out-String)"
    }

    Write-Host "Database verification passed: all 36 table row counts match the immutable baseline."
}
