<#
.SYNOPSIS
  Build and pack AOSharp.Common and SmokeLounge.AOtomation as separate NuGet packages.

.DESCRIPTION
  Produces nupkgs under SDK/nupkgs. SmokeLounge.AOtomation depends on AOSharp.Common
  (ProjectReference becomes a package dependency at pack time).

  If an older netframework AOSharp.Common already exists on a feed you own, keep
  PackageId AOSharp.Common and publish this multi-TFM package as a newer version.
  If you do not own that ID, pass -CommonPackageId.

.PARAMETER Version
  Package version for both packages (default: 1.0.0).

.PARAMETER Configuration
  Build configuration (default: Release).

.PARAMETER CommonPackageId
  Override PackageId for Common only (default: AOSharp.Common).

.PARAMETER SmokeLoungePackageId
  Override PackageId for SmokeLounge (default: SmokeLounge.AOtomation).

.PARAMETER OutputDir
  Directory for nupkgs (default: SDK/nupkgs).

.PARAMETER Push
  If set, push packed nupkgs to -Source after packing.

.PARAMETER Source
  NuGet source for -Push (default: nuget.org).

.PARAMETER ApiKey
  API key for -Push.
#>
[CmdletBinding()]
param(
    [string] $Version = "1.0.0",
    [ValidateSet("Debug", "Release")]
    [string] $Configuration = "Release",
    [string] $CommonPackageId = "AOSharp.Common",
    [string] $SmokeLoungePackageId = "SmokeLounge.AOtomation",
    [string] $OutputDir = "",
    [switch] $Push,
    [string] $Source = "https://api.nuget.org/v3/index.json",
    [string] $ApiKey = ""
)

$ErrorActionPreference = "Stop"

$sdkRoot = Split-Path -Parent $PSScriptRoot
if (-not $OutputDir) {
    $OutputDir = Join-Path $sdkRoot "nupkgs"
}

$commonProj = Join-Path $sdkRoot "AOSharp.Common\AOSharp.Common.csproj"
$smokeProj = Join-Path $sdkRoot "SmokeLounge.AOtomation\SmokeLounge.AOtomation.csproj"

New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null

# Project-scoped IDs avoid -p:PackageId leaking across the whole graph
$sharedProps = @(
    "-p:Version=$Version",
    "-p:PackageVersion=$Version",
    "-p:PackageOutputPath=$OutputDir",
    "-p:AOSharpCommonPackageId=$CommonPackageId",
    "-p:SmokeLoungePackageId=$SmokeLoungePackageId"
)

Write-Host "Building $Configuration (x86)..." -ForegroundColor Cyan
dotnet build $commonProj -c $Configuration -p:Platform=x86 @sharedProps --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed: AOSharp.Common" }

dotnet build $smokeProj -c $Configuration -p:Platform=x86 @sharedProps --nologo
if ($LASTEXITCODE -ne 0) { throw "Build failed: SmokeLounge.AOtomation" }

Write-Host "Packing $CommonPackageId $Version..." -ForegroundColor Cyan
dotnet pack $commonProj -c $Configuration -p:Platform=x86 --no-build @sharedProps --nologo
if ($LASTEXITCODE -ne 0) { throw "Pack failed: AOSharp.Common" }

Write-Host "Packing $SmokeLoungePackageId $Version..." -ForegroundColor Cyan
dotnet pack $smokeProj -c $Configuration -p:Platform=x86 --no-build @sharedProps --nologo
if ($LASTEXITCODE -ne 0) { throw "Pack failed: SmokeLounge.AOtomation" }

$packages = @(Get-ChildItem $OutputDir -Filter "*.nupkg" |
    Where-Object {
        $_.Name -like "*$Version*" -and
        $_.Name -notmatch '\.symbols\.' -and
        ($_.Name -like "$CommonPackageId.*" -or $_.Name -like "$SmokeLoungePackageId.*")
    } | Sort-Object Name)

Write-Host ""
Write-Host "Packages:" -ForegroundColor Green
$packages | ForEach-Object { Write-Host "  $($_.FullName)" }

if ($Push) {
    if (-not $ApiKey) {
        $ApiKey = $env:NUGET_API_KEY
    }
    if (-not $ApiKey) {
        throw "-Push requires -ApiKey or env var NUGET_API_KEY"
    }
    if ($packages.Count -eq 0) {
        throw "No packages to push"
    }
    foreach ($pkg in $packages) {
        Write-Host "Pushing $($pkg.Name) -> $Source" -ForegroundColor Cyan
        & dotnet nuget push $pkg.FullName --source $Source --api-key $ApiKey --skip-duplicate
        if ($LASTEXITCODE -ne 0) { throw "Push failed: $($pkg.Name)" }
    }
    Write-Host "Push complete." -ForegroundColor Green
}

Write-Host "Done." -ForegroundColor Green
