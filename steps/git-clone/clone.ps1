# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Write-Host '========== Start to clone Git repo =========='
Write-Host ('Repo: ' + $env:GIT_CLONE_REPO)
Write-Host ('Local Path: ' + $env:GIT_CLONE_PATH)
Write-Host ('Git User: ' + $env:GIT_CLONE_USER)
Write-Host '============================================='

git config --global credential.helper store

$server = $env:GIT_CLONE_REPO
if ($server.Substring(8).IndexOf('/') -ge 0) {
    $idx = $server.Substring(8).IndexOf('/') + 8
    $server = $server.Substring(0, $idx)
}
Write-Host ('Git server: ' + $server)

if (-not (Test-Path $env:GIT_CLONE_PATH))
{
    New-Item $env:GIT_CLONE_PATH -ItemType 'Directory'
}

if (-not [string]::IsNullOrEmpty($env:GIT_CLONE_USER)) 
{
    Write-Host 'Adding git credential'
    $cmd = 'cmdkey /generic:"git:' + $server + '" /user:"' + $env:GIT_CLONE_USER + '" /pass:"' + $env:GIT_CLONE_TOKEN + '"';
    Invoke-Expression $cmd
}

Set-Location $env:GIT_CLONE_PATH
$name = [System.IO.Path]::GetFileNameWithoutExtension($env:GIT_CLONE_REPO)
$folder = Join-Path $env:GIT_CLONE_PATH $name
if (Test-Path $folder)
{
    Write-Host $folder + ' is not empty, removing...'
    Remove-Item -Path $folder -Force -Recurse
}

Write-Host 'Cloning...'
git clone $env:GIT_CLONE_REPO

if (-not [string]::IsNullOrEmpty($env:GIT_CLONE_USER)) 
{
    Write-Host 'Removing git credential'
    $cmd = 'cmdkey /delete:"' + $server + '"';
    Invoke-Expression $cmd
}

Write-Host 'Git clone finished'