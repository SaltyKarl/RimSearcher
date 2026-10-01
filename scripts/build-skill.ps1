#Requires -Version 7.0
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Invoke-Dotnet {
    & dotnet @args
    if ($LASTEXITCODE -ne 0) { throw "dotnet failed with exit code $LASTEXITCODE." }
}

$root = Split-Path $PSScriptRoot -Parent
$modSource = Join-Path $root 'RimSearcher_DataMod'
$skill = Join-Path $root 'skills/rimsearcher'
$cliProject = Join-Path $root 'Sources/RimSearcher.Cli/RimSearcher.Cli.csproj'
$modProject = Join-Path $root 'Sources/RimSearcher.DataMod/RimSearcher.DataMod.csproj'
$version = ([xml](Get-Content $cliProject -Raw)).Project.PropertyGroup.Version
if ($version -ne ([xml](Get-Content $modProject -Raw)).Project.PropertyGroup.Version -or
    $version -ne ([xml](Get-Content "$modSource/About/About.xml" -Raw)).ModMetaData.modVersion) {
    throw 'CLI, DataMod and About.xml versions must match.'
}

$work = Join-Path $root ".release/build-skill-$([guid]::NewGuid().ToString('N'))"
New-Item $work -ItemType Directory -Force | Out-Null
$keepBackup = $false
Push-Location $root
try {
    Invoke-Dotnet publish $cliProject -c Release -r win-x64 -o "$work/cli"
    Invoke-Dotnet build $modProject -c Release '-t:Rebuild' -o "$work/mod/Assemblies" "-p:NativeOutputPath=$work/mod/Native"
    $cli = "$work/cli/rimsearcher.exe"
    $builtVersion = & $cli --version
    if ($LASTEXITCODE -ne 0 -or $builtVersion.Trim() -ne $version) { throw 'CLI startup or version check failed.' }
    if ([Reflection.AssemblyName]::GetAssemblyName("$work/mod/Assemblies/RimSearcher.DataMod.dll").Version.ToString(3) -ne $version) {
        throw 'Built DataMod version does not match CLI.'
    }

    $managed = @(Get-ChildItem "$work/mod/Assemblies" -File -Filter '*.dll')
    foreach ($file in $managed) {
        if ($file.Name -notmatch '^(RimSearcher\.DataMod|Microsoft\.Data\.Sqlite|SQLitePCLRaw\.[\w.]+|System\.[\w.]+)\.dll$') {
            throw "Unexpected DataMod assembly: $($file.Name)."
        }
    }
    foreach ($name in @('Microsoft.Data.Sqlite.dll', 'SQLitePCLRaw.core.dll', 'SQLitePCLRaw.provider.dynamic_cdecl.dll')) {
        if ($name -notin $managed.Name) { throw "Missing DataMod runtime assembly: $name." }
    }
    $native = @('e_sqlite3.dll', 'libe_sqlite3.so', 'libe_sqlite3.dylib', 'libe_sqlite3.arm64.dylib')
    $nativeFiles = @($native | ForEach-Object { Get-Item "$work/mod/Native/$_" })
    $translations = @(Get-ChildItem "$modSource/Languages" -Recurse -File -Filter '*.xml')
    if ($translations.Count -eq 0) { throw 'DataMod translations are missing.' }

    # Refresh only generated files; root-level exports and metadata stay untouched.
    foreach ($directory in @('Assemblies', 'Native')) {
        if (Test-Path "$modSource/$directory") { Remove-Item "$modSource/$directory" -Recurse -Force }
        New-Item "$modSource/$directory" -ItemType Directory | Out-Null
    }
    $managed | Copy-Item -Destination "$modSource/Assemblies"
    $nativeFiles | Copy-Item -Destination "$modSource/Native"
    $files = @(
        Get-Item "$modSource/About/About.xml"
        Get-ChildItem "$modSource/Assemblies" -File -Filter '*.dll'
        $native | ForEach-Object { Get-Item "$modSource/Native/$_" }
        $translations
    )
    $zip = "$work/RimSearcher_DataMod.zip"
    $archive = [IO.Compression.ZipFile]::Open($zip, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in $files) {
            $relative = [IO.Path]::GetRelativePath($modSource, $file.FullName).Replace('\', '/')
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $file.FullName, "RimSearcher_DataMod/$relative") | Out-Null
        }
    }
    finally { $archive.Dispose() }

    # Package the full Skill with fresh binaries before replacing any distribution artifact.
    $skillZip = "$work/skills.zip"
    $archive = [IO.Compression.ZipFile]::Open($skillZip, [IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($file in Get-ChildItem $skill -Recurse -File) {
            $relative = [IO.Path]::GetRelativePath($skill, $file.FullName).Replace('\', '/')
            $source = switch ($relative) {
                'bin/rimsearcher.exe' { $cli }
                'assets/RimSearcher_DataMod.zip' { $zip }
                default { $file.FullName }
            }
            [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive, $source, "rimsearcher/$relative") | Out-Null
        }
    }
    finally { $archive.Dispose() }

    # Each file replacement preserves its previous version.
    New-Item "$skill/bin", "$skill/assets" -ItemType Directory -Force | Out-Null
    $sources = @($cli, $zip, $skillZip)
    $targets = @("$skill/bin/rimsearcher.exe", "$skill/assets/RimSearcher_DataMod.zip", "$root/.release/skills.zip")
    $updated = @()
    try {
        foreach ($i in 0..($targets.Count - 1)) {
            if ([IO.File]::Exists($targets[$i])) {
                [IO.File]::Replace($sources[$i], $targets[$i], "$work/previous-$i")
            }
            else { [IO.File]::Move($sources[$i], $targets[$i]) }
            $updated += $i
        }
    }
    catch {
        $keepBackup = $true
        foreach ($i in $updated) {
            if (Test-Path "$work/previous-$i") { [IO.File]::Move("$work/previous-$i", $targets[$i], $true) }
            else { Remove-Item $targets[$i] -Force }
        }
        $keepBackup = $false
        throw
    }
    Write-Host "Built RimSearcher ${version}: $skill"
    Write-Host "Release Skill archive: $root/.release/skills.zip"
}
finally {
    Pop-Location
    if ($keepBackup) { Write-Warning "Rollback failed; previous artifacts remain in $work." }
    else { Remove-Item $work -Recurse -Force }
}
