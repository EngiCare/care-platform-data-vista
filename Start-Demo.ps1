<#
.SYNOPSIS
    Launches the full Care Platform demo stack locally.

.DESCRIPTION
    Starts three services in order, waits for each to become healthy,
    then opens the example web frontend in a browser:

        1. Mock VistA RPC Server   (tcp://localhost:9200, admin http://localhost:9201)
        2. Data Service            (https://localhost:5001)
        3. Example Web Frontend    (http://localhost:5253)

    Login credentials:  Site = "VistA RPC Server (Test)"
                        Access Code = cprs
                        Verify Code = cprs1234

    No STS, Entra, certificates, or external network required.
    Press Ctrl+C to shut everything down.

.PARAMETER SkipBuild
    Skip building projects (use existing binaries).

.PARAMETER NoBrowser
    Don't open the browser automatically.

.PARAMETER MockVistaPath
    Path to the mock-vista repo.  Default: ..\care-platform-core-mock-vista

.EXAMPLE
    .\Start-Demo.ps1
    .\Start-Demo.ps1 -SkipBuild
    .\Start-Demo.ps1 -MockVistaPath C:\repos\care-platform-core-mock-vista
#>
param(
    [switch]$SkipBuild,
    [switch]$NoBrowser,
    [string]$MockVistaPath = "$PSScriptRoot\..\care-platform-core-mock-vista"
)

$ErrorActionPreference = "Stop"
$Root = $PSScriptRoot

# ── Helpers ──────────────────────────────────────────────────────────────

function Write-Step($msg) { Write-Host "`n> $msg" -ForegroundColor Cyan }
function Write-Ok($msg)   { Write-Host "  [OK] $msg" -ForegroundColor Green }
function Write-Fail($msg) { Write-Host "  [FAIL] $msg" -ForegroundColor Red }
function Write-Info($msg) { Write-Host "  $msg" -ForegroundColor DarkGray }

function Wait-ForPort([int]$Port, [int]$TimeoutSec = 20) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    while ((Get-Date) -lt $deadline) {
        try {
            $tcp = New-Object Net.Sockets.TcpClient
            $tcp.Connect("127.0.0.1", $Port)
            $tcp.Close()
            return $true
        } catch {
            Start-Sleep -Milliseconds 300
        }
    }
    return $false
}

function Wait-ForHttp([string]$Url, [int]$TimeoutSec = 20) {
    $deadline = (Get-Date).AddSeconds($TimeoutSec)
    # Allow self-signed certs
    $cb = [System.Net.ServicePointManager]::ServerCertificateValidationCallback
    [System.Net.ServicePointManager]::ServerCertificateValidationCallback = { $true }
    try {
        while ((Get-Date) -lt $deadline) {
            try {
                $r = Invoke-WebRequest -Uri $Url -TimeoutSec 3 -UseBasicParsing -ErrorAction SilentlyContinue
                if ($r.StatusCode -lt 500) { return $true }
            } catch { }
            Start-Sleep -Milliseconds 500
        }
        return $false
    } finally {
        [System.Net.ServicePointManager]::ServerCertificateValidationCallback = $cb
    }
}

$Procs = @()

function Stop-All {
    Write-Host "`nShutting down..." -ForegroundColor Yellow
    foreach ($p in $script:Procs) {
        if ($p -and !$p.HasExited) {
            $p.Kill()
            Write-Info "Stopped PID $($p.Id)"
        }
    }
    Write-Host "Done." -ForegroundColor Green
}

# ── Validate prerequisites ──────────────────────────────────────────────

Write-Host ""
Write-Host "========================================================" -ForegroundColor Cyan
Write-Host "   Care Platform — Local Demo Launcher                  " -ForegroundColor Cyan
Write-Host "========================================================" -ForegroundColor Cyan

$MockVistaPath = (Resolve-Path $MockVistaPath -ErrorAction SilentlyContinue).Path
if (-not $MockVistaPath -or -not (Test-Path "$MockVistaPath\care-platform-core-mock-vista.sln")) {
    Write-Fail "Mock VistA repo not found. Expected at: $MockVistaPath"
    Write-Info "Clone it or pass -MockVistaPath <path>"
    exit 1
}
Write-Ok "Mock VistA repo: $MockVistaPath"

# ── Build ────────────────────────────────────────────────────────────────

if (-not $SkipBuild) {
    Write-Step "Building Mock VistA"
    $out = & dotnet build "$MockVistaPath\care-platform-core-mock-vista.sln" --verbosity quiet 2>&1
    if ($out | Select-String "Build FAILED") {
        Write-Fail "Mock VistA build failed:"
        $out | Select-String "error CS" | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok "Mock VistA built"

    Write-Step "Building Data Service + Example Frontend"
    $out = & dotnet build "$Root\dotnet\care-platform-data-vista.slnx" --verbosity quiet 2>&1
    if ($out | Select-String "Build FAILED") {
        Write-Fail "Data service build failed:"
        $out | Select-String "error CS" | ForEach-Object { Write-Host "    $_" -ForegroundColor Red }
        exit 1
    }
    Write-Ok "Data service + example built"
} else {
    Write-Info "Skipping build (-SkipBuild)"
}

# ── 1. Mock VistA RPC Server ────────────────────────────────────────────

Write-Step "Starting Mock VistA RPC Server (tcp://localhost:9200)"
$rpcProj = "$MockVistaPath\CarePlatform.VistA.RpcServer\CarePlatform.VistA.RpcServer.csproj"
$rpcProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--no-build","--project",$rpcProj `
    -WorkingDirectory "$MockVistaPath\CarePlatform.VistA.RpcServer" `
    -PassThru -WindowStyle Minimized
$Procs += $rpcProc

if (-not (Wait-ForPort 9200 -TimeoutSec 20)) {
    Write-Fail "Mock VistA failed to start on port 9200"
    Stop-All; exit 1
}
Write-Ok "Mock VistA running (PID $($rpcProc.Id))"

# ── 2. Data Service ─────────────────────────────────────────────────────

Write-Step "Starting Data Service (https://localhost:5001)"
$dataProj = "$Root\dotnet\CarePlatform.Data.CPRS\CarePlatform.Data.CPRS.csproj"
$dataProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--no-build","--project",$dataProj,"--launch-profile","https" `
    -WorkingDirectory "$Root\dotnet\CarePlatform.Data.CPRS" `
    -PassThru -WindowStyle Minimized
$Procs += $dataProc

if (-not (Wait-ForHttp "https://localhost:5001/api/connection/sites" -TimeoutSec 25)) {
    Write-Fail "Data Service failed to start on port 5001"
    Stop-All; exit 1
}
Write-Ok "Data Service running (PID $($dataProc.Id))"

# ── 3. Example Web Frontend ─────────────────────────────────────────────

Write-Step "Starting Example Web Frontend (http://localhost:5253)"
$webProj = "$Root\dotnet\CarePlatform.Web.Example\CarePlatform.Web.Example.csproj"
$webProc = Start-Process -FilePath "dotnet" `
    -ArgumentList "run","--no-build","--project",$webProj `
    -WorkingDirectory "$Root\dotnet\CarePlatform.Web.Example" `
    -PassThru -WindowStyle Minimized
$Procs += $webProc

if (-not (Wait-ForHttp "http://localhost:5253" -TimeoutSec 20)) {
    Write-Fail "Example frontend failed to start on port 5253"
    Stop-All; exit 1
}
Write-Ok "Example frontend running (PID $($webProc.Id))"

# ── Ready ────────────────────────────────────────────────────────────────

Write-Host ""
Write-Host "========================================================" -ForegroundColor Green
Write-Host "           Care Platform Demo Ready                      " -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "  Mock VistA      tcp://localhost:9200   PID $($rpcProc.Id)" -ForegroundColor Green
Write-Host "  Data Service    https://localhost:5001  PID $($dataProc.Id)" -ForegroundColor Green
Write-Host "  Web Frontend    http://localhost:5253   PID $($webProc.Id)" -ForegroundColor Green
Write-Host "--------------------------------------------------------" -ForegroundColor Green
Write-Host "  Login:" -ForegroundColor Green
Write-Host "    Site:        VistA RPC Server (Test)" -ForegroundColor Green
Write-Host "    Access Code: cprs" -ForegroundColor Green
Write-Host "    Verify Code: cprs1234" -ForegroundColor Green
Write-Host "========================================================" -ForegroundColor Green
Write-Host ""
Write-Host "  Press Ctrl+C to stop all services." -ForegroundColor DarkGray
Write-Host ""

if (-not $NoBrowser) {
    Start-Process "http://localhost:5253"
}

# Keep running until user presses Ctrl+C
try {
    $rpcProc.WaitForExit()
} finally {
    Stop-All
}
