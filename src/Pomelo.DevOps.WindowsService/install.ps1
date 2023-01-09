$location = Get-Location
$service = Join-Path $location 'Pomelo.DevOps.WindowsService.exe'
$path = [Environment]::GetFolderPath("Windows") + '\Microsoft.NET\Framework64\v4.0.30319'
Set-Location $path
.\installutil.exe /i $service
Set-Service -Name "Pomelo DevOps Windows Service" -StartupType Automatic
Start-Service -Name "Pomelo DevOps Windows Service"
Set-Location $location