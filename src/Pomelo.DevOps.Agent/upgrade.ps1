Write-Host 'Upgrading agent...'

$global:ProgressPreference = 'SilentlyContinue'

If (Test-Path '../install.ps1') {
	$agentType = 'Service'
} 
Else {
	$agentType = 'Daemon'
}

$appSettings = Get-Content -Path 'appsettings.json' | ConvertFrom-Json
$server = $appSettings.Server
If ([System.String]::IsNullOrEmpty($server)) {
	Write-Host 'Agent has not been registered.'
	Exit 0
}
$server = $server.TrimEnd('/')

$version = ''
If (Test-Path '../build.txt') {
	$version = Get-Content -Path '../build.txt' -Raw
	$version = $version.Trim();
}

$serverBuildVersionUrl = $server + '/agent/build.txt'
$serverBuildVersion = Invoke-WebRequest $serverBuildVersionUrl
$serverBuildVersion = $serverBuildVersion.Content.Trim()

If ($version -Eq $serverBuildVersion) {
	Write-Host 'Current version' $version 'is already up to date'
	Exit 0
}

if (-Not (Test-Path '../arch.txt')) {
	Write-Host 'arch.txt not found.'
	Exit 0
}

$arch = Get-Content -Path '../arch.txt' -Raw
$arch = $arch.Trim();

$agentPackage = $server + '/agent/Agent-' + $agentType + '-' + $arch + '.zip'

If (Test-Path '../tmp') {
	Remove-Item -Path '../tmp' -Recurse -Force
}
New-Item -Path '../tmp' -ItemType Directory
$outFilePath = '../tmp/agent.zip'

Invoke-WebRequest $agentPackage -OutFile $outFilePath
Expand-Archive -Path $outFilePath -DestinationPath '../tmp/Agent' -Force
Remove-Item '../tmp/Agent/agent/appsettings.json' -Force
Copy-Item -Path '../tmp/Agent/agent/*' -Destination './' -Recurse -Force
Copy-Item -Path '../tmp/Agent/build.txt' -Destination '../build.txt' -Recurse -Force
Copy-Item -Path '../tmp/Agent/arch.txt' -Destination '../arch.txt' -Recurse -Force

Remove-Item '../tmp' -Force -Recurse

Write-Host 'Agent upgraded.'