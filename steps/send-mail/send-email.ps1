# Copyright (c) Yuko(Yisheng) Zheng. All rights reserved.
# Licensed under the MIT. See LICENSE in the project root for license information.

Function Generate-EmailReceiverString($str)
{
    If ($str -ne $null)
    {
        $arr = $str.Split(',')
        
        For ($i = 0; $i -lt $arr.Count; ++$i)
        {
            $name = 'Dummy'
            $arr[$i] = $name + ' <' + $arr[$i].Trim() + '>'
        }
        Return $arr
    }
    Else
    {
        Return '<>'
    }
}

Function Send-Email($username, $passwordText, $subject, $body, $myname, $to, $cc, $bcc, $attachments = $null, $smtp = 'smtp.pomelo.cloud', $isHtml = $false, $port = 25, $useSsl = $false)
{
    Write-Host ('Sending email ' + $subject + '...')
    $from = $myname + ' <' + $env:SEND_EMAIL_SENDER_ADDR + '>'
    Write-Host ('From: ' + $from)
    $to = Generate-EmailReceiverString $to
    Write-Host ('To: ' + $to)
    $cc = Generate-EmailReceiverString $cc
    Write-Host ('Cc: ' + $cc)
    $bcc = Generate-EmailReceiverString $bcc
    Write-Host ('Bcc: ' + $bcc)
    $password = ConvertTo-SecureString $passwordText -AsPlainText -Force
    $subject = $subject -Replace "[^ -~]", ""
    [pscredential]$credential = New-Object System.Management.Automation.PSCredential ($username, $password)
    If ($attachments -ne $null)
    {
        $attachments = $attachments.Split([Environment]::NewLine)
        If ($isHtml)
        {
            If ($useSsl) {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -BodyAsHtml -Encoding UTF8 -SmtpServer $smtp -Port $port -UseSsl -Attachments $attachments
            } Else {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -BodyAsHtml -Encoding UTF8 -SmtpServer $smtp -Port $port -Attachments $attachments
            }
        }
        Else
        {
            If ($useSsl) {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -Encoding UTF8 -SmtpServer $smtp -Port $port -UseSsl -Attachments $attachments
            } Else {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -Encoding UTF8 -SmtpServer $smtp -Port $port -Attachments $attachments
            }
        }
    }
    Else
    {
        If ($isHtml)
        {
            If ($useSsl) {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -BodyAsHtml -Encoding UTF8 -SmtpServer $smtp -Port $port -UseSsl
            } Else {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -BodyAsHtml -Encoding UTF8 -SmtpServer $smtp -Port $port
            }
        }
        Else
        {
            If ($useSsl) {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -Encoding UTF8 -SmtpServer $smtp -Port $port -UseSsl
            } Else {
                Send-MailMessage -Credential $credential -From $from -To $to -Cc $cc -Bcc $bcc -Subject $subject -Body $body -Encoding UTF8 -SmtpServer $smtp -Port $port
            }
        }
    }
    Write-Host 'Email sent'
}