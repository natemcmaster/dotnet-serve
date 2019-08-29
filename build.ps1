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

function Invoke-Block([scriptblock]$cmd) {
    $cmd | Out-String | Write-Verbose
    & $cmd

    # Need to check both of these cases for errors as they represent different items
    # - $?: did the powershell script block throw an error
    # - $lastexitcode: did a windows command executed by the script block end in error
    if ((-not $?) -or ($lastexitcode -ne 0)) {
        if ($error -ne $null)
        {
            Write-Warning $error[0]
        }
        throw "Command failed to execute: $cmd"
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

try {
    $commitId = git rev-parse HEAD
    $MSBuildArgs += "-p:SourceRevisionId=$commitId"
    $commitHeight = git rev-list --count HEAD
    $MSBuildArgs += "-p:BuildNumber=$commitHeight"
}
catch { }

$CodeSign = $sign -or ($ci -and ($env:System_PullRequest_IsFork -ne 'True') -and $IsWindows)

if ($CodeSign) {
    $MSBuildArgs += '-p:CodeSign=true'
}

if ($ci) {
    $MSBuildArgs += '-p:CI=true'

    Invoke-Block {
        & dotnet msbuild src/dotnet-serve/ -nologo -t:UpdateCiSettings @MSBuildArgs
    }
}

if ($CodeSign) {
    $toolsDir = "$PSScriptRoot/.build/tools"

    $AzureSignToolPath = "$toolsDir/azuresigntool"
    if ($IsWindows) {
        $AzureSignToolPath += ".exe"
    }

    if (-not (Test-Path $AzureSignToolPath)) {
        Invoke-Block {
            & dotnet tool install --tool-path $toolsDir `
                AzureSignTool `
                --version 2.0.17
        }
    }

    $nstDir = "$toolsDir/nugetsigntool/1.1.4"
    $NuGetKeyVaultSignToolPath = "$nstDir/tools/net471/NuGetKeyVaultSignTool.exe"
    if (-not (Test-Path $NuGetKeyVaultSignToolPath)) {
        New-Item $nstDir -ItemType Directory -ErrorAction Ignore | Out-Null
        Invoke-WebRequest https://github.com/onovotny/NuGetKeyVaultSignTool/releases/download/v1.1.4/NuGetKeyVaultSignTool.1.1.4.nupkg `
            -OutFile "$nstDir/NuGetKeyVaultSignTool.zip"
        Expand-Archive "$nstDir/NuGetKeyVaultSignTool.zip" -DestinationPath $nstDir
    }

    $MSBuildArgs += "-p:AzureSignToolPath=$AzureSignToolPath"
    $MSBuildArgs += "-p:NuGetKeyVaultSignToolPath=$NuGetKeyVaultSignToolPath"
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

# Make restore quiet
Invoke-Block {
    & dotnet restore '-v:q'
}

Invoke-Block {
    & dotnet build `
        --no-restore `
        @MSBuildArgs
}

Invoke-Block {
    & dotnet pack `
        --no-build `
        -o $artifacts @MSBuildArgs
    }
Invoke-Block {
    & dotnet test `
        "$PSScriptRoot/test/dotnet-serve.Tests/" `
        --no-build `
        --logger trx `
        @MSBuildArgs
}

write-host -f magenta 'Done'
