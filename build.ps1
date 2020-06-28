#!/usr/bin/env pwsh
[CmdletBinding(PositionalBinding = $false)]
param(
    [ValidateSet('Debug', 'Release')]
    $Configuration = $null,
    [switch]
    $ci
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

if ($ci) {
    $MSBuildArgs += '-p:CI=true'
}

$artifacts = "$PSScriptRoot/artifacts/"

Remove-Item -Recurse $artifacts -ErrorAction Ignore

Invoke-Block {
    & dotnet build `
        @MSBuildArgs
}

Invoke-Block {
    & dotnet pack `
        --no-build `
        -o $artifacts @MSBuildArgs
    }
Invoke-Block {
    & dotnet test `
        --no-build `
        --logger trx `
        @MSBuildArgs
}

write-host -f magenta 'Done'
