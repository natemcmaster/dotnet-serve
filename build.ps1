#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci,
	[switch]
	$sign
)

Set-StrictMode -Version 1
$ErrorActionPreference = 'Stop'

if (-not (Test-Path Variable:/IsCoreCLR)) {
    $IsCoreCLR = $false
    $IsWindows = $true
}

function exec([string]$_cmd) {
    write-host -ForegroundColor DarkGray ">>> $_cmd $args"
    $ErrorActionPreference = 'Continue'
    & $_cmd @args
    $ErrorActionPreference = 'Stop'
    if ($LASTEXITCODE -ne 0) {
        write-error "Failed with exit code $LASTEXITCODE"
        exit 1
    }
}

#
# Main
#

if ($env:CI -eq 'true') {
    $ci = $true
}

if (!$Configuration) {
    $Configuration = if ($ci) { 'Release' } else { 'Debug' }
}

[string[]] $MSBuildArgs = @("-p:Configuration=$Configuration")

if ($ci) {
    $MSBuildArgs += '-p:CI=true'
}

$CodeSign = $sign -or ($ci -and -not $env:APPVEYOR_PULL_REQUEST_HEAD_COMMIT -and $IsWindows)

if ($CodeSign) {
    $toolsDir = "$PSScriptRoot/.build/tools"

    $AzureSignToolPath = "$toolsDir/azuresigntool"
    if ($IsWindows) {
        $AzureSignToolPath += ".exe"
    }

    if (-not (Test-Path $AzureSignToolPath)) {
        exec dotnet tool install --tool-path $toolsDir `
        AzureSignTool `
        --version 2.0.17
    }

    $nstDir = "$toolsDir/nugetsigntool/1.1.4"
    $NuGetKeyVaultSignToolPath = "$nstDir/tools/net471/NuGetKeyVaultSignTool.exe"
    if (-not (Test-Path $NuGetKeyVaultSignToolPath)) {
        New-Item $nstDir -ItemType Directory -ErrorAction Ignore | Out-Null
        Invoke-WebRequest https://github.com/onovotny/NuGetKeyVaultSignTool/releases/download/v1.1.4/NuGetKeyVaultSignTool.1.1.4.nupkg `
            -OutFile "$nstDir/NuGetKeyVaultSignTool.zip"
        Expand-Archive "$nstDir/NuGetKeyVaultSignTool.zip" -DestinationPath $nstDir
    }

    $MSBuildArgs += '-p:CodeSign=true'
    $MSBuildArgs += "-p:AzureSignToolPath=$AzureSignToolPath"
    $MSBuildArgs += "-p:NuGetKeyVaultSignToolPath=$NuGetKeyVaultSignToolPath"
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

exec dotnet build @MSBuildArgs

exec dotnet pack `
    --no-build `
    -o $artifacts @MSBuildArgs

exec dotnet test `
    "$PSScriptRoot/test/dotnet-serve.Tests/" `
    --no-build `
     @MSBuildArgs

write-host -f magenta 'Done'
