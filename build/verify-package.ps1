#requires -Version 5.1
[CmdletBinding()]
param(
    [string]$Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = Split-Path -Parent $PSScriptRoot
Push-Location $root

try {
    [xml]$props = Get-Content (Join-Path $root 'Directory.Build.props')
    $propertyGroup = @($props.Project.PropertyGroup)[0]
    $version = [string]$propertyGroup.VersionPrefix
    if (-not [string]::IsNullOrEmpty([string]$propertyGroup.VersionSuffix)) {
        $version = $version + '-' + [string]$propertyGroup.VersionSuffix
    }

    $packages = Join-Path $root 'artifacts\packages'
    $consumer = Join-Path $root 'tests\PackageConsumer\PackageConsumer.csproj'
    $consumerOutput = Join-Path $root 'tests\PackageConsumer\bin'
    $consumerPackages = Join-Path $root 'artifacts\package-consumer\packages'
    $resultFile = Join-Path $root 'artifacts\package-consumer\result.txt'

    Write-Host "Packing Community.Phone.Controls.Toolkit.WPF $version..."
    dotnet pack (Join-Path $root 'src\Community.Phone.Controls.Toolkit.WPF\Community.Phone.Controls.Toolkit.WPF.csproj') `
        -c $Configuration -o $packages -p:PackageVersion=$version --nologo
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet pack failed with exit code $LASTEXITCODE."
    }

    Remove-Item (Join-Path $root 'artifacts\package-consumer') -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Path $consumerPackages -Force | Out-Null
    New-Item -ItemType Directory -Path (Split-Path -Parent $resultFile) -Force | Out-Null

    $oldNuGetPackages = $env:NUGET_PACKAGES
    $oldResult = $env:PACKAGE_CONSUMER_RESULT
    $env:NUGET_PACKAGES = $consumerPackages
    $env:PACKAGE_CONSUMER_RESULT = $resultFile

    try {
        Write-Host 'Restoring the fresh consumer from the local package feed...'
        dotnet restore $consumer --configfile (Join-Path $root 'tests\PackageConsumer\NuGet.config') `
            -p:ToolkitVersion=$version --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Consumer restore failed with exit code $LASTEXITCODE."
        }

        Write-Host 'Building the fresh consumer...'
        dotnet build $consumer -c $Configuration --no-restore -p:ToolkitVersion=$version --nologo
        if ($LASTEXITCODE -ne 0) {
            throw "Consumer build failed with exit code $LASTEXITCODE."
        }

        $exe = Join-Path $consumerOutput "$Configuration\net10.0-windows\Community.Phone.Controls.Toolkit.WPF.PackageConsumer.exe"
        if (-not (Test-Path $exe)) {
            throw "Consumer executable was not found at $exe."
        }

        Write-Host 'Launching the consumer smoke test...'
        $process = Start-Process -FilePath $exe -PassThru
        if (-not $process.WaitForExit(30000)) {
            Stop-Process -Id $process.Id -Force
            throw 'Consumer did not exit within 30 seconds.'
        }

        if ($process.ExitCode -ne 0) {
            throw "Consumer exited with code $($process.ExitCode)."
        }

        if (-not (Test-Path $resultFile) -or (Get-Content $resultFile -Raw).Trim() -ne 'ok') {
            throw 'Consumer did not report a successful load of the package resources and controls.'
        }

        Write-Host 'Package verification passed: the consumer loaded the toolkit controls and theme resources.' -ForegroundColor Green
    }
    finally {
        $env:NUGET_PACKAGES = $oldNuGetPackages
        $env:PACKAGE_CONSUMER_RESULT = $oldResult
    }
}
finally {
    Pop-Location
}
