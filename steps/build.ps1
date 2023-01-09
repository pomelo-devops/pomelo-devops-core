# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

param(
    [string] $source = $null,
    [string] $dest = '.\bin'
)

Function Get-PackageId([string]$yml)
{
    $text = Get-Content -Path $yml -Encoding UTF8
    $splited = $text.Split([Environment]::NewLine)
    $find = [System.Linq.Enumerable]::Where($splited, [Func[object,bool]]{ param($x) $x.ToString().StartsWith('id: ') })
    return $find[0].Substring(3).Trim()
}

Function Get-PackageVersion([string]$yml)
{
    $text = Get-Content -Path $yml -Encoding UTF8
    $splited = $text.Split([Environment]::NewLine)
    $query = [System.Linq.Enumerable]::Where($splited, [Func[object,bool]]{ param($x) $x.ToString().StartsWith('version:') })
    $find = [System.Linq.Enumerable]::ToArray($query)
    return $find[0].ToString().Substring(8).Trim()
}

Function Pack-SinglePackage([string]$yml)
{
    if (-not (Test-Path $dest))
    {
        New-Item -Path $dest -ItemType Directory
    }
    $path = $yml.Substring(0, $yml.Length - "step.yml".Length);
    $name = (Get-PackageId $yml) + '-' + (Get-PackageVersion $yml)
    $dest = Join-Path $dest ($name + '.zip')
    Compress-Archive -Path ($path + '\*') -DestinationPath $dest
    $pdo = ($dest.Substring(0, $dest.Length - 4) + '.pdo')
    if (Test-Path $pdo)
    {
        Remove-Item -Path $pdo -Force
    }
    Rename-Item -Path $dest -NewName ($name + '.pdo') -Force
    Write-Host ('Generated ' + $pdo)
}

if ($source -eq $null) {
    $source = '.\'
}

$packages = Get-ChildItem -Path $source -Include "step.yml" -Recurse
Write-Host ('Found ' + $packages.Length + ' raw packages')
for ($i = 0; $i -le $packages.Length; ++$i)
{
    if ($packages[$i] -and -not $packages[$i].FullName.Contains(".need-refactor"))
    {
        Pack-SinglePackage $packages[$i]
    }
}

Write-Host 'Done'