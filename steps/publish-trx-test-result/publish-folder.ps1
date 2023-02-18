# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Write-Host 'Publishing test result...'
Write-Host 'Test result folder:' $env:PUBLISH_FOLDER_PATH
$endpoint = '/extensions/Pomelo.DevOps.JobExtensions.TRX/api/'

Set-Location $env:PUBLISH_FOLDER_PATH
$items = Get-ChildItem
$supportedExtensions = '.json', '.trx'
$failed = $false
For ($i = 0; $i -lt $items.Length; ++$i) {
    $file = $items[$i];
    If (-not ($supportedExtensions.Contains([System.IO.Path]::GetExtension($file.Name)))) {
        Continue;
    }

    $mode = 'trx';
    If ($file.Name.EndsWith('.json')) {
        $mode = 'simple'
    }

    Add-Type -AssemblyName System.Net.Http
    $form = New-Object System.Net.Http.MultipartFormDataContent
    $fs = [System.IO.FileStream]::New($file.FullName, [System.IO.FileMode]::Open)
    $streamContent = New-Object System.Net.Http.StreamContent $fs
    $form.Add($streamContent, "file", "file")
    $client = New-Object System.Net.Http.HttpClient
    $url = $env:AGENT_API_HOST + $env:AGENT_API_JOB + $endpoint + $mode + '/' + $file.Name
    $response = $client.PostAsync($url, $form).Result
    Write-Host $response.StatusCode
    If (-not $response.IsSuccessStatusCode) {
        $failed = $true
        Write-Host 'Publish' $file.Name 'failed'
    }
    Else {
        Write-Host 'Publish' $file.Name 'succeeded'
    }
}

Write-Host 'Test result published'

If ($failed) {
    Exit 1
}