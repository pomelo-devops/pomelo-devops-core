# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Write-Host 'Publishing test result...'
Write-Host 'Test result file:' $env:PUBLISH_TRX_PATH
$endpoint = '/extensions/Pomelo.DevOps.JobExtensions.TRX/api/'
$mode = 'trx';
If ($env:PUBLISH_TRX_PATH.EndsWith('.json')) {
    $mode = 'simple'
}
$endpoint = $endpoint + $mode + '/' + $env:PUBLISH_TRX_SUITE
$url = $env:AGENT_API_HOST + $env:AGENT_API_JOB + $endpoint
Write-Host $url
Add-Type -AssemblyName System.Net.Http
$form = New-Object System.Net.Http.MultipartFormDataContent
$fs = [System.IO.FileStream]::New($env:PUBLISH_TRX_PATH, [System.IO.FileMode]::Open)
$streamContent = New-Object System.Net.Http.StreamContent $fs
$form.Add($streamContent, "file", "file")
$client = New-Object System.Net.Http.HttpClient
$response = $client.PostAsync($url, $form).Result
Write-Host $response.StatusCode
If (-not $response.IsSuccessStatusCode) {
    Exit 1
}
Write-Host 'Test result published'