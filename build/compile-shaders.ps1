<#
.SYNOPSIS
    Compiles the projection pixel shaders into the byte code that ships with the
    Community.Phone assembly.

.DESCRIPTION
    WPF loads effects from compiled pixel shader byte code (.ps), not from HLSL.
    The compiled files are committed to the repository, so a normal build needs
    no shader tools; run this script after editing a .hlsl source.

    The script looks for fxc.exe in the Windows SDK. Install the Windows SDK
    (or the "Desktop development with C++" workload) when it is missing.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File build\compile-shaders.ps1
#>
[CmdletBinding()]
param(
    [string]$ShaderModel = 'ps_2_0'
)

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$shaderDirectory = Join-Path $root 'src\Community.Phone\Projection\Rendering\Shader'

if (-not (Test-Path $shaderDirectory)) {
    throw "Shader directory not found: $shaderDirectory"
}

$fxc = Get-ChildItem "${env:ProgramFiles(x86)}\Windows Kits\10\bin" -Filter fxc.exe -Recurse -ErrorAction SilentlyContinue |
    Where-Object { $_.FullName -match '\\x86\\fxc\.exe$' } |
    Sort-Object FullName -Descending |
    Select-Object -First 1

if (-not $fxc) {
    throw 'fxc.exe was not found. Install the Windows SDK to compile the projection shaders.'
}

Write-Host "Using $($fxc.FullName)"

Get-ChildItem $shaderDirectory -Filter *.hlsl | ForEach-Object {
    $output = [System.IO.Path]::ChangeExtension($_.FullName, '.ps')
    & $fxc.FullName -nologo -T $ShaderModel -E main -Fo $output $_.FullName

    if ($LASTEXITCODE -ne 0) {
        throw "Shader compilation failed: $($_.Name)"
    }

    Write-Host "Compiled $($_.Name) -> $([System.IO.Path]::GetFileName($output))"
}
