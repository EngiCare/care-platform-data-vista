<#
.SYNOPSIS
    Launches the Care Platform demo: Mock VistA RPC server + Example Web Frontend.
.DESCRIPTION
    Builds and starts two services, waits for each to become healthy,
    then opens the browser to http://localhost:5253.

    The web frontend connects directly to Mock VistA over the XWB broker
    protocol -- no microservice layer required.

    Login:  Site = "Mock VistA (localhost)"
            Access Code = cprs    Verify Code = cprs1234

    Press Ctrl+C to stop all services.
.PARAMETER SkipBuild
    Skip building and use existing binaries.
.PARAMETER NoBrowser
    Do not open the browser automatically.
#>
param(
    [switch]$SkipBuild,
    [switch]$NoBrowser
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

function Write-Step($msg) { Write-Host "`n> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "  [FAIL] $msg" -ForegroundColor Red }
function Write-Info($msg) { Write-Host "  $msg" -ForegroundColor DarkGray }

function Wait-ForPort([int]$Port, [int]$TimeoutSec = 20, [System.Diagnostics.Process]$Proc = $null) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        if ($Proc -and $Proc.HasExited) {
            Write-Info "Process exited with code $($Proc.ExitCode)"
            return $false
        }
        try {
            $tcp = New-Object Net.Sockets.TcpClient
            $tcp.Connect("127.0.0.1", $Port)
            $tcp.Close()
            return $true
        } catch { Start-Sleep -Milliseconds 500 }
    }
    return $false
}

function Wait-ForHttp([string]$Url, [int]$TimeoutSec = 20, [System.Diagnostics.Process]$Proc = $null) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        if ($Proc -and $Proc.HasExited) {
            Write-Info "Process exited with code $($Proc.ExitCode)"
            return $false
        }
        try {
            $r = Invoke-WebRequest -Uri $Url -TimeoutSec 3 -UseBasicParsing -ErrorAction SilentlyContinue
            if ($r.StatusCode -lt 500) { return $true }
        } catch { }
        Start-Sleep -Milliseconds 500
    }
    return $false
}

$Procs = @()

function Stop-All {
    Write-Host "`nShutting down..." -ForegroundColor Yellow
    foreach ($p in $script:Procs) {
        if ($p -and !$p.HasExited) {
            try { $p.Kill() } catch { }
            Write-Info "Stopped PID $($p.Id)"
        }
    }
    Write-Host "Done." -ForegroundColor Green
}

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "   Care Platform -- Local Demo                          " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

if (-not $SkipBuild) {
    Write-Step "Building demo projects"
    $slnx = "$Root\care-platform-data-vista.slnx"
    $out = & dotnet build $slnx --verbosity quiet 2>&1
    if ($out | Select-String "Build FAILED") {
        Write-Fail "Build failed:"
        $out | Select-String "error CS" | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok "Build succeeded"
} else {
    Write-Info 'Skipping build'
}

Write-Step 'Starting Mock VistA RPC Server on tcp://localhost:9200'
$mockProj = "$Root\demo\CarePlatform.MockVistA\CarePlatform.MockVistA.csproj"
$mockProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--no-build","--project",$mockProj `
    -WorkingDirectory "$Root\demo\CarePlatform.MockVistA" `
    -PassThru -WindowStyle Minimized
$Procs += $mockProc

if (-not (Wait-ForPort 9200 -TimeoutSec 15 -Proc $mockProc)) {
    Write-Fail "Mock VistA failed to start on port 9200"
    Stop-All; exit 1
}
Write-Ok "Mock VistA running"

Write-Step 'Starting Example Web Frontend on http://localhost:5253'
$webProj = "$Root\demo\CarePlatform.Web.Example\CarePlatform.Web.Example.csproj"
$webProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--no-build","--project",$webProj `
    -WorkingDirectory "$Root\demo\CarePlatform.Web.Example" `
    -PassThru -WindowStyle Minimized
$Procs += $webProc

if (-not (Wait-ForHttp "http://localhost:5253" -TimeoutSec 20 -Proc $webProc)) {
    Write-Fail "Web frontend failed to start on port 5253"
    Stop-All; exit 1
}
Write-Ok "Web frontend running"

Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "           Care Platform Demo Ready                      " -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "  Mock VistA      tcp://localhost:9200" -ForegroundColor Green
Write-Host "  Web Frontend    http://localhost:5253" -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "  Login:" -ForegroundColor Green
Write-Host '    Site:        Mock VistA (localhost)' -ForegroundColor Green
Write-Host "    Access Code: cprs" -ForegroundColor Green
Write-Host "    Verify Code: cprs1234" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Press Ctrl+C to stop all services." -ForegroundColor DarkGray
Write-Host ""

if (-not $NoBrowser) {
    Start-Process "http://localhost:5253"
}

try {
    $mockProc.WaitForExit()
} finally {
    Stop-All
}
