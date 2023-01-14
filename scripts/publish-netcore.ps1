# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

$platforms = "win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64"

$root = Get-Location
$bin = Join-Path $root 'bin'
$pdb = Join-Path $root 'pdb'
$src = Join-Path $root 'src'
$cliProject = 'Pomelo.DevOps.Agent.CLI'
$cli = Join-Path $src $cliProject
$daemonProject = 'Pomelo.DevOps.Daemon'
$daemon = Join-Path $src $daemonProject

If (Test-Path $bin) {
    Remove-Item -Path $bin -Force -Recurse
}
New-Item -Path $bin -ItemType Directory

Set-Location $cli
For ($j = 0; $j -lt $platforms.Length; ++$j) {
    $outputDir = Join-Path $cli ("bin/Release/net6.0/" + $platforms[$j] + "/publish")
    If (Test-Path $outputDir) {
        Remove-Item -Path $outputDir -Force -Recurse
    }
    dotnet publish -c Release -r $platforms[$j] --self-contained -p:PublishTrimmed=true -p:PublishSingleFile=true
    $dest = Join-Path $bin ($cliProject + "-" + $platforms[$j])
    [System.IO.Directory]::Move($outputDir, $dest);
}

Set-Location $daemon
For ($j = 0; $j -lt $platforms.Length; ++$j) {
    $outputDir = Join-Path $daemon ("bin/Release/net6.0/" + $platforms[$j] + "/publish")
    If (Test-Path $outputDir) {
        Remove-Item -Path $outputDir -Force -Recurse
    }
    dotnet publish -c Release -r $platforms[$j] --self-contained -p:PublishTrimmed=true -p:PublishSingleFile=true
    $dest = Join-Path $bin ($daemonProject + "-" + $platforms[$j])
    [System.IO.Directory]::Move($outputDir, $dest);
}

Set-Location $src
$items = Get-ChildItem
For($i = 0; $i -lt $items.Length; ++$i) {
    If ($items[$i].Name.Contains("Models") -or $items[$i].Name.Contains("Shared") -or $items[$i].Name.Contains(".CLI") -or $items[$i].Name.Contains(".WindowsService") -or $items[$i].Name.Contains(".Daemon")) {
        Continue;
    }

    Set-Location $items[$i].Name;
    $currentDir = Get-Location
    $platforms = "win-x64", "win-arm64", "linux-x64", "linux-arm64", "osx-x64", "osx-arm64"
    For ($j = 0; $j -lt $platforms.Length; ++$j) {
        $outputDir = Join-Path $currentDir ("bin/Release/net6.0/" + $platforms[$j] + "/publish")
        If (Test-Path $outputDir) {
            Remove-Item -Path $outputDir -Force -Recurse
        }
        
        $dest = Join-Path $bin ($items[$i].Name + "-" + $platforms[$j])
        dotnet publish -c Release -r $platforms[$j] --self-contained
        
        [System.IO.Directory]::Move($outputDir, $dest);
        If ($items[$i].Name.EndsWith(".Agent")) {
            $cliBin = Join-Path $bin ($cliProject + "-" + $platforms[$j])
            $cliSingleFile = 'pomelo.exe'
            $cliExecutable = Join-Path $cliBin $cliSingleFile
            If (-not (Test-Path $cliExecutable)) {
                $cliSingleFile = 'pomelo'
                $cliExecutable = Join-Path $cliBin $cliSingleFile
            }
            $destCli = Join-Path $dest $cliSingleFile
            Copy-Item -Path $cliExecutable -Destination $destCli
        }

        Set-Content -Path (Join-Path $dest 'build.txt') -Value $env:BUILD_VERSION -Force

        # Compress-Archive -Path $dest -DestinationPath ($dest + ".zip")
    }
    Set-Location $src
}

Set-Location $root