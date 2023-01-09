# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

. .\send-email.ps1

Write-Host $env:SEND_EMAIL_TO + '...'
$html = (Get-Content $env:SEND_EMAIL_BODY_PATH -Raw).ToString()
Write-Host $html
Send-Email $env:SEND_EMAIL_SENDER_USER $env:SEND_EMAIL_SENDER_PASS $env:SEND_EMAIL_SUBJECT $html $env:SEND_EMAIL_SENDER_NAME $env:SEND_EMAIL_TO $env:SEND_EMAIL_CC $env:SEND_EMAIL_BCC $env:SEND_EMAIL_ATTACHMENTS $env:SEND_EMAIL_SMTP ($env:SEND_EMAIL_BODY_TYPE -eq 'HTML') $env:SEND_EMAIL_SMTP_PORT $env:SEND_EMAIL_USE_SSL
Write-Host 'Send email finished'