# Builds Thunderstore zips with the correct per-package icon and CHANGELOG.md.
param(
    [string] $CompatVersion = "1.1.10",
    [string] $FixVersion = "1.3.26"
)

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$out = Join-Path $root "dist\upload"

dotnet build (Join-Path $root "Cjayride.SkyheimEitr\Cjayride.SkyheimEitr.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "SkyheimEitr build failed" }

$il = Join-Path $root "tools\ilrepack\pkg\tools\ILRepack.exe"
$bep = "D:\SteamLibrary\steamapps\common\Valheim\BepInEx\core"
$game = "D:\SteamLibrary\steamapps\common\Valheim\valheim_Data\Managed"
$merged = Join-Path $root "dist\skyheim.merged.dll"
& $il /lib:$bep /lib:$game /out:$merged (Join-Path $root "dist\skyheim.dll") (Join-Path $root "dist\eitr\Cjayride.SkyheimEitr.dll")
if ($LASTEXITCODE -ne 0) { throw "ILRepack failed" }

function New-TsZip {
    param($Stage, $Zip, $Files)
    Remove-Item $Stage -Recurse -Force -ErrorAction SilentlyContinue
    New-Item -ItemType Directory -Force -Path $Stage | Out-Null
    foreach ($pair in $Files.GetEnumerator()) {
        if (-not (Test-Path $pair.Value)) {
            throw "Missing $($pair.Value)"
        }
        Copy-Item $pair.Value (Join-Path $Stage $pair.Key)
    }
    Remove-Item $Zip -Force -ErrorAction SilentlyContinue
    Compress-Archive -Path (Join-Path $Stage "*") -DestinationPath $Zip
    Write-Host $Zip
}

New-TsZip -Stage (Join-Path $out "compat-stage") -Zip (Join-Path $out "SkyheimCompat-$CompatVersion.zip") -Files @{
    "Cjayride.SkyheimCompat.dll" = "$root\dist\compat\Cjayride.SkyheimCompat.dll"
    "manifest.json" = "$root\skyheim-patch\manifest.json"
    "README.md" = "$root\skyheim-patch\README.md"
    "CHANGELOG.md" = "$root\skyheim-patch\CHANGELOG.md"
    "icon.png" = "$root\skyheim-patch\icon.png"
}

New-TsZip -Stage (Join-Path $out "fix-stage") -Zip (Join-Path $out "SkyheimFix-$FixVersion.zip") -Files @{
    "skyheim.dll" = $merged
    "skyheim.json" = "$root\dist\skyheim.json"
    "manifest.json" = "$root\skyheim-fix\manifest.json"
    "README.md" = "$root\skyheim-fix\README.md"
    "CHANGELOG.md" = "$root\skyheim-fix\CHANGELOG.md"
    "icon.png" = "$root\skyheim-fix\icon.png"
}
