$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
Remove-Item -ItemType Directory -Force -Path $PSScriptRoot\tmp