# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Write-Host 'Run Inline Powershell Script Step Started.'
$timestamp = Get-Date
$scriptPath = '.\' + $timestamp.Ticks.ToString() + '.ps1'
Invoke-WebRequest ('http://localhost:5500/api/variable/' + $env:STAGE_ID + '/POWERSHELL_SCRIPT_CONTENT') -OutFile $scriptPath
powershell -File $scriptPath
$code = $LASTEXITCODE
Remove-Item $scriptPath -Force
Write-Host 'Run Inline Powershell Script Step Finished.'
Exit $code