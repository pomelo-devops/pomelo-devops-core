# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

$root = Get-Location

$bin = Join-Path $root 'bin/Pomelo.DevOps.WindowsService'
If (Test-Path $bin) {
    Remove-Item -Path $bin -Force -Recurse
}

Write-Host 'Compiling Pomelo.DevOps.WindowsService'
Set-Location $root
cd .\src\Pomelo.DevOps.WindowsService
$msbuild = 'C:\Program Files\Microsoft Visual Studio\2022\Enterprise\MSBuild\Current\Bin\MsBuild.exe';
If (-Not(Test-Path $msbuild)) {
    $msbuild = 'C:\Program Files\Microsoft Visual Studio\2022\Community\MSBuild\Current\Bin\MsBuild.exe';
}
If (-Not(Test-Path $msbuild)) {
    $msbuild = 'C:\Program Files\Microsoft Visual Studio\2019\Enterprise\MSBuild\Current\Bin\MsBuild.exe';
}
If (-Not(Test-Path $msbuild)) {
    $msbuild = 'C:\Program Files\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\MsBuild.exe';
}

. $msbuild .\Pomelo.DevOps.WindowsService.csproj -p:Configuration=Release
$publishPath = Join-Path $root 'src\Pomelo.DevOps.WindowsService\bin\Release'
Set-Location $root
[System.IO.Directory]::Move($publishPath, $bin)

Set-Location $root