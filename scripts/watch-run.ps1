param(
    [int]$Port = 5290,
    [switch]$KillAny
)

$lines = netstat -ano | Select-String -Pattern "LISTENING" | Select-String -Pattern ":$Port\\s"
$pids = @()
foreach ($line in $lines) {
    $parts = ($line.ToString().Trim() -split '\\s+')
    if ($parts.Length -ge 5) {
        $pid = 0
        if ([int]::TryParse($parts[-1], [ref]$pid) -and $pid -gt 0) {
            $pids += $pid
        }
    }
}
$pids = $pids | Select-Object -Unique

foreach ($pid in $pids) {
    try {
        $proc = Get-Process -Id $pid -ErrorAction Stop
        $name = $proc.ProcessName
        $canKill = $KillAny.IsPresent -or $name -in @('PcbErpApi', 'dotnet')

        if (-not $canKill) {
            Write-Error "Port $Port is used by '$name' (PID $pid). Re-run with -KillAny to force stop it."
            exit 1
        }

        Write-Host "Stopping $name (PID $pid) on port $Port..."
        Stop-Process -Id $pid -Force
    }
    catch {
        Write-Warning "Failed to stop PID $pid: $($_.Exception.Message)"
    }
}

Write-Host "Starting dotnet watch on port $Port..."
dotnet watch run --no-hot-reload
