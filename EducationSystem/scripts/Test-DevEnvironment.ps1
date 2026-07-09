param(
    [string]$FrontendOrigin = "http://127.0.0.1:5173"
)

$ErrorActionPreference = "Continue"

$checks = @(
    @{ Name = "IdentityService"; Url = "http://localhost:5001/api/identity/users?pageNumber=1&pageSize=1" },
    @{ Name = "AcademicService"; Url = "http://localhost:5002/api/academic/students?pageNumber=1&pageSize=1" },
    @{ Name = "ExamService"; Url = "http://localhost:5003/api/exam/exam-results?pageNumber=1&pageSize=1" },
    @{ Name = "CommunicationService"; Url = "http://localhost:5004/api/communication/form-requests?pageNumber=1&pageSize=1" }
)

Write-Host "Frontend origin: $FrontendOrigin"
Write-Host ""

foreach ($check in $checks) {
    try {
        $response = Invoke-WebRequest `
            -Uri $check.Url `
            -Headers @{ Origin = $FrontendOrigin } `
            -UseBasicParsing `
            -TimeoutSec 10

        $body = $response.Content | ConvertFrom-Json
        $cors = $response.Headers["Access-Control-Allow-Origin"]
        Write-Host "[OK] $($check.Name)"
        Write-Host "     Url: $($check.Url)"
        Write-Host "     Status: $($response.StatusCode)"
        Write-Host "     Success: $($body.success)"
        Write-Host "     Total: $($body.data.totalItems)"
        Write-Host "     CORS: $cors"
    }
    catch {
        Write-Host "[FAIL] $($check.Name)"
        Write-Host "       Url: $($check.Url)"
        Write-Host "       Error: $($_.Exception.Message)"
        Write-Host "       Check whether the service is running and whether its DB connection string is valid."
    }

    Write-Host ""
}

Write-Host "Listening ports:"
Get-NetTCPConnection -LocalPort 5001, 5002, 5003, 5004 -State Listen -ErrorAction SilentlyContinue |
    Select-Object LocalAddress, LocalPort, OwningProcess |
    Sort-Object LocalPort |
    Format-Table -AutoSize
