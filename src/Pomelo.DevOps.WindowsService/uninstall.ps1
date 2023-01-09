$location = Get-Location
$service = Join-Path $location 'Pomelo.DevOps.WindowsService.exe'
$path = [Environment]::GetFolderPath("Windows") + '\Microsoft.NET\Framework64\v4.0.30319'
Set-Location $path
.\installutil.exe /u $service
Set-Location $location