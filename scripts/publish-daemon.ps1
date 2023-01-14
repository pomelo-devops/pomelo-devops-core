# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

$platforms = "win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64"
$root = Get-Location
$bin = Join-Path $root 'bin'
$binDaemon = Join-Path $bin 'Pomelo.DevOps.Daemon'

For ($i = 0; $i -lt $platforms.Count; ++$i) {
    $agent = Join-Path $bin ('Pomelo.DevOps.Agent-' + $platforms[$i])
    $agentDaemon = Join-Path $bin ('Agent-Daemon-' + $platforms[$i])
    If (Test-Path $agentDaemon) {
        Remove-Item -Path $agentDaemon -Recurse -Force
    }

    Copy-Item -Path ($binDaemon + '-' + $platforms[$i]) -Destination $agentDaemon -Recurse

    $agentSvcBin = Join-Path $agentDaemon 'agent'
    Copy-Item -Path $agent -Destination $agentSvcBin -Recurse
    Remove-Item -Path (Join-Path $agentDaemon 'process.json')
    Copy-Item -Path (Join-Path $root 'scripts/process-agent.json') -Destination (Join-Path $agentDaemon 'process.json')
    Set-Content -Path (Join-Path $agentDaemon 'build.txt') -Value $env:BUILD_VERSION -Force
    Set-Content -Path (Join-Path $agentDaemon 'arch.txt') -Value 'win-x64' -Force   

    Compress-Archive -Path ($agentDaemon + '/**') -DestinationPath ($agentDaemon + ".zip")
}
Set-Location $root