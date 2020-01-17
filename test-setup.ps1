# NATS Server Download and Setup
$NATSVersion = "v2.1.2"
$NATSBuild = "nats-server-"+$NATSVersion+"-windows-386"
$NATSUri = "https://github.com/nats-io/nats-server/releases/download/" + $NATSVersion + "/" + $NATSBuild + ".zip"
$PSScriptRoot = Split-Path -Parent -Path $MyInvocation.MyCommand.Definition
New-Item -ItemType Directory -Force -Path $PSScriptRoot\tmp
Invoke-WebRequest -Uri $NATSUri -OutFile $PSScriptRoot\tmp\$NATSBuild.zip
Expand-Archive -Path $PSScriptRoot\tmp\$NATSBuild.zip -DestinationPath $PSScriptRoot\tmp\
Remove-Item $PSScriptRoot\tmp\$NATSBuild.zip
    