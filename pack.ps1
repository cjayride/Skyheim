# Builds Thunderstore zips with the correct per-package icon and CHANGELOG.md.
param(
    [string] $CompatVersion = "1.2.2",
    [string] $FixVersion = "1.4.2"
)

$ErrorActionPreference = "Stop"
$root = if ($PSScriptRoot) { $PSScriptRoot } else { (Get-Location).ProviderPath }
$out = Join-Path $root "dist\upload"

# localization\skyheim.json is the source of truth; dist\skyheim.json is the packaged copy.
Copy-Item (Join-Path $root "localization\skyheim.json") (Join-Path $root "dist\skyheim.json") -Force

dotnet build (Join-Path $root "Cjayride.SkyheimEitr\Cjayride.SkyheimEitr.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "SkyheimEitr build failed" }
dotnet build (Join-Path $root "Cjayride.SkyheimCompat\Cjayride.SkyheimCompat.csproj") -c Release
if ($LASTEXITCODE -ne 0) { throw "SkyheimCompat build failed" }

$il = Join-Path $root "tools\ilrepack\pkg\tools\ILRepack.exe"
$bep = "D:\SteamLibrary\steamapps\common\Valheim\BepInEx\core"
if (-not (Test-Path (Join-Path $bep "BepInEx.dll"))) {
    $bep = "D:\AI-Agent-Projects\Mods\libs"
}
$game = "D:\SteamLibrary\steamapps\common\Valheim\valheim_Data\Managed"
$repackDir = Join-Path $root "dist\repack"
$tmpDir = Join-Path $root "dist\tmp"
New-Item -ItemType Directory -Force -Path $repackDir | Out-Null
New-Item -ItemType Directory -Force -Path $tmpDir | Out-Null
$env:TEMP = $tmpDir
$env:TMP = $tmpDir
# Output file name becomes the assembly name. The asset bundle looks up scripts in "skyheim".
$merged = Join-Path $repackDir "skyheim.dll"
if (Test-Path $merged) { Remove-Item $merged -Force }
& $il /lib:$bep /lib:$game /out:$merged (Join-Path $root "dist\skyheim.dll") (Join-Path $root "dist\eitr\Cjayride.SkyheimEitr.dll")
if ($LASTEXITCODE -ne 0) { throw "ILRepack failed" }
if ((Get-Item $merged).Length -lt 4500000 -or (Get-Item $merged).Length -gt 5600000) {
    throw "Repacked skyheim.dll has an unexpected size: $((Get-Item $merged).Length) bytes"
}

Add-Type -Path "$env:USERPROFILE\.nuget\packages\mono.cecil\0.11.6\lib\net40\Mono.Cecil.dll"
$resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
$reader = New-Object Mono.Cecil.ReaderParameters
$reader.ReadWrite = $true
$reader.AssemblyResolver = $resolver
$asm = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($merged, $reader)
$plugin = $asm.MainModule.Types | Where-Object { $_.Name -eq "SkyheimPlugin" }
foreach ($attr in $plugin.CustomAttributes) {
    if ($attr.AttributeType.Name -eq "BepInPlugin") {
        $attr.ConstructorArguments[2] = New-Object Mono.Cecil.CustomAttributeArgument($attr.ConstructorArguments[2].Type, $FixVersion)
    }
}
$asm.Write()
$asm.Dispose()

# The merged DLL must still carry the asset bundle, and must carry the Eitr altar patches.
Add-Type -Path "$env:USERPROFILE\.nuget\packages\mono.cecil\0.11.6\lib\net40\Mono.Cecil.dll"
$check = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($merged)
$resNames = @($check.MainModule.Resources | ForEach-Object { $_.Name })
$typeNames = @($check.MainModule.Types | ForEach-Object { $_.Name })
$check.Dispose()
if ($resNames -notcontains "skyheim.resources.skyheim") {
    throw "Repacked skyheim.dll lost its asset bundle resource: $($resNames -join ', ')"
}
foreach ($type in @("SkyheimAssetPatches", "AltarIconPatch", "SkillIconSprites")) {
    if ($typeNames -notcontains $type) { throw "Repacked skyheim.dll is missing $type" }
}
foreach ($res in @("rune-atlas.txt", "skill_fire_magic.png", "skill_frost_magic.png", "skill_holy_magic.png", "skill_nature_magic.png")) {
    if ($resNames -notcontains $res) { throw "Repacked skyheim.dll is missing $res : $($resNames -join ', ')" }
}

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
