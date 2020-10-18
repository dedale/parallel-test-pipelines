
<#
.SYNOPSIS
Run all tests.

.DESCRIPTION
Run .NET Core 3.1 and .NET Framework 4.8 NUnit tests for all assemblies in artifact.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function ExitWithCode
{
    param
    (
        [Parameter()]
        $exitCode
    )

    $host.SetShouldExit($exitCode)
    exit
}

if (-not (Test-Path Env:BUILD_SOURCESDIRECTORY)) {
    Write-Error "BUILD_SOURCESDIRECTORY undefined"
    ExitWithCode 1
}
if (-not (Test-Path Env:SYSTEM_ARTIFACTSDIRECTORY)) {
    Write-Error "SYSTEM_ARTIFACTSDIRECTORY undefined"
    ExitWithCode 1
}

$TestsPath = "${Env:BUILD_SOURCESDIRECTORY}\.tests"
if (Test-Path $TestsPath) {
    Remove-Item $TestsPath -Recurse
}

try {
    $Command = "${Env:SYSTEM_ARTIFACTSDIRECTORY}\adapter\netcoreapp3.1\NUnitAdapter.exe ${Env:SYSTEM_ARTIFACTSDIRECTORY}\bin\netcoreapp3.1"
    Write-Host $Command
    Invoke-Expression $Command
} catch {
    ExitWithCode 1
}

try {
    $Command = "${Env:SYSTEM_ARTIFACTSDIRECTORY}\adapter\net48\NUnitAdapter.exe ${Env:SYSTEM_ARTIFACTSDIRECTORY}\bin\net48"
    Write-Host $Command
    Invoke-Expression $Command
} catch {
    ExitWithCode 1
}

ExitWithCode 0