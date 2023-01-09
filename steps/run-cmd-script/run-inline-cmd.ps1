# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Write-Host 'Run Inline Cmd Script Step Started.'
$timestamp = Get-Date
$scriptPath = '.\' + $timestamp.Ticks.ToString() + '.cmd'
Invoke-WebRequest ('http://localhost:5500/api/variable/' + $env:STAGE_ID + '/CMD_SCRIPT_CONTENT') -OutFile $scriptPath
cmd /c $scriptPath
$code = $LASTEXITCODE
Remove-Item $scriptPath -Force
Write-Host 'Run Inline Cmd Script Step Finished.'
Exit $code