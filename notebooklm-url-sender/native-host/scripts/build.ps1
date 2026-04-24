param(
  [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$Project = Join-Path $PSScriptRoot "..\src\NotebookLmSender.Host.csproj"
$OutDir = Join-Path $PSScriptRoot "..\dist"

if (Test-Path $OutDir) {
  Remove-Item $OutDir -Recurse -Force
}

Write-Host "Publishing native host..."
dotnet publish $Project -c $Configuration -r win-x64 --self-contained true /p:PublishSingleFile=true -o $OutDir

Write-Host "Build complete: $OutDir"
