# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

If ([System.String]::IsNullOrEmpty($env:JOB_NUMBER)) {
    $env:JOB_NUMBER = [System.DateTime]::Now.ToString("yyyyMMddHHmmss")
}

$versionPrefix = Get-Content -Path (Join-Path (Get-Location) 'version.txt') -Raw
$env:BUILD_VERSION = $versionPrefix + '-' + $env:JOB_NUMBER

.\scripts\publish-netcore.ps1
.\scripts\publish-winsvc.ps1
.\scripts\publish-agent-service-win-x64.ps1
.\scripts\publish-daemon.ps1

Set-Content -Path (Join-Path (Get-Location) 'bin/build.txt') -Value $env:BUILD_VERSION -Force