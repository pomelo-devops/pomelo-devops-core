# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

$root = Get-Location
$bin = Join-Path $root 'bin'
$binSvc = Join-Path $bin 'Pomelo.DevOps.WindowsService'
$agent = Join-Path $bin 'Pomelo.DevOps.Agent-win-x64'
$agentSvc = Join-Path $bin 'Agent-Service-win-x64'
If (Test-Path $agentSvc) {
    Remove-Item -Path $agentSvc -Recurse -Force
}

Copy-Item -Path $binSvc -Destination $agentSvc -Recurse

$agentSvcBin = Join-Path $agentSvc 'agent'
Copy-Item -Path $agent -Destination $agentSvcBin -Recurse
Remove-Item -Path (Join-Path $agentSvc 'process.json')
Copy-Item -Path (Join-Path $root 'scripts/process-agent.json') -Destination (Join-Path $agentSvc 'process.json')
Set-Content -Path (Join-Path $agentSvc 'build.txt') -Value $env:BUILD_VERSION -Force
Set-Content -Path (Join-Path $agentSvc 'arch.txt') -Value 'win-x64' -Force
Set-Location $root