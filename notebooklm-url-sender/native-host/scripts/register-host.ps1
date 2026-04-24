param(
  [string]$ExtensionId,
  [string]$InstallRoot = "C:\Program Files\NotebookLmUrlSender"
)

$ErrorActionPreference = "Stop"
if ([string]::IsNullOrWhiteSpace($ExtensionId)) {
  throw "Provide -ExtensionId from chrome://extensions"
}

$manifestTemplate = Join-Path $PSScriptRoot "..\manifest\com.example.notebooklm_sender.json"
$manifestOutput = Join-Path $InstallRoot "com.example.notebooklm_sender.json"
$exePath = Join-Path $InstallRoot "NotebookLmSender.Host.exe"

New-Item -ItemType Directory -Path $InstallRoot -Force | Out-Null
Copy-Item (Join-Path $PSScriptRoot "..\dist\NotebookLmSender.Host.exe") $exePath -Force

$json = Get-Content $manifestTemplate -Raw
$json = $json.Replace("C:\\Program Files\\NotebookLmUrlSender\\NotebookLmSender.Host.exe", $exePath.Replace("\", "\\"))
$json = $json.Replace("__EXTENSION_ID__", $ExtensionId)
Set-Content -Path $manifestOutput -Value $json -Encoding UTF8

$regPath = "HKCU:\Software\Google\Chrome\NativeMessagingHosts\com.example.notebooklm_sender"
New-Item -Path $regPath -Force | Out-Null
Set-ItemProperty -Path $regPath -Name "(default)" -Value $manifestOutput

Write-Host "Registered native host at $manifestOutput"
