param([string]$RuntimeDirectory,[string]$OutputDirectory)
$ErrorActionPreference='Stop'
$sourceRoot=Split-Path -Parent $PSScriptRoot
if(!$OutputDirectory){$OutputDirectory=Join-Path $sourceRoot ('dist/PlatTogetherOpenSourced-'+(Get-Date -Format 'yyyyMMdd-HHmmss'))}
$output=[IO.Path]::GetFullPath($OutputDirectory)
if(Test-Path -LiteralPath $output){throw 'Choose a new output directory. Builds never replace an existing portable folder or its saves.'}
New-Item -ItemType Directory -Path $output | Out-Null
foreach($project in @(@('Launcher','PlatinumPrivateLauncher'),@('MailSender','PlatinumMailSender'))){
    & dotnet publish (Join-Path $PSScriptRoot ($project[0]+'/'+$project[0]+'.csproj')) -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o (Join-Path $output $project[0])
    if($LASTEXITCODE -ne 0){throw "$($project[0]) build failed"}
}
& dotnet publish (Join-Path $sourceRoot 'MapStudio/MapStudio.csproj') -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o (Join-Path $output 'MapStudio')
if($LASTEXITCODE -ne 0){throw 'Map Studio build failed'}
$serverSource=Join-Path $PSScriptRoot 'Server'
$serverOutput=Join-Path $output 'Server'
New-Item -ItemType Directory -Path $serverOutput | Out-Null
# Source allowlist: a developer may run a server in its checkout. Never package
# ignored databases, account profiles, .git metadata or other local runtime data.
Get-ChildItem -LiteralPath $serverSource -File | Where-Object {$_.Extension -eq '.mjs' -or $_.Name -in @('package.json','server.example.json','README.md','LICENSE','THIRD_PARTY_NOTICES.md')} | Copy-Item -Destination $serverOutput
foreach($tree in @('vendor','test','examples')){
    $base=Join-Path $serverSource $tree
    if(!(Test-Path -LiteralPath $base)){continue}
    Get-ChildItem -LiteralPath $base -Recurse -File | Where-Object {$_.Extension -in @('.mjs','.js','.json') -or $_.Name -in @('LICENSE','LICENSE.txt','README.md')} | ForEach-Object {
        $relative=[IO.Path]::GetRelativePath($serverSource,$_.FullName)
        $target=Join-Path $serverOutput $relative
        New-Item -ItemType Directory -Path ([IO.Path]::GetDirectoryName($target)) -Force | Out-Null
        Copy-Item -LiteralPath $_.FullName -Destination $target
    }
}
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs') -Destination (Join-Path $output 'docs') -Recurse
@'
# Pokemon Platinum Together Open Sourced - Launcher

Open `Launcher/PlatinumPrivateLauncher.exe` and keep this complete folder together.
Read the [setup documentation](docs/README.md) before hosting or importing saves.
This is a development preview; native unattended simulation and custom map/sprite
loading are unfinished. See [verification status](docs/status.md).

- [Launcher, mail app and documentation](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-launcher)
- [Server](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-server)
- [Map Studio and trainer sprites](https://github.com/dr4lera/pokemon-platinum-together-open-sourced-editors)

Supply your own supported game/ROM. Do not share Client, Data, Profiles, OwnerKeys,
claim journals or account backups. No game files or player data are included here.
'@ | Set-Content -LiteralPath (Join-Path $output 'README.md') -Encoding utf8
New-Item -ItemType Directory -Path (Join-Path $output 'Runtime'),(Join-Path $output 'ContentPacks') | Out-Null
if($RuntimeDirectory){
    foreach($name in @('node.exe','LICENSE-Node.txt','cloudflared.exe','LICENSE-cloudflared.txt')){
        $file=Join-Path $RuntimeDirectory $name
        if(!(Test-Path -LiteralPath $file -PathType Leaf)){throw "Missing runtime dependency/notice: $name"}
        Copy-Item -LiteralPath $file -Destination (Join-Path $output ('Runtime/'+$name))
    }
}else{Write-Warning 'Supply Node.js and cloudflared with their licenses in Runtime, or rebuild with -RuntimeDirectory. This folder cannot run the backend yet.'}
foreach($name in @('LICENSE','THIRD_PARTY_NOTICES.md')){if(Test-Path -LiteralPath (Join-Path $sourceRoot $name)){Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination (Join-Path $output $name)}}
# Fail closed if a game binary or the quarantined lab payload enters a package.
$blockedHash='5520b7d176a3598b67f3c126359deada0f4099a6f961143453d7d790d69ab6c2'
foreach($payload in (Get-ChildItem -LiteralPath $output -Recurse -File)){
    if($payload.Name -ieq 'Platinum.exe'){throw 'Release blocked: game executables must not be distributed.'}
    if((Get-FileHash -LiteralPath $payload.FullName -Algorithm SHA256).Hash -ieq $blockedHash){throw 'Release blocked: quarantined game payload detected.'}
}
$records=Get-ChildItem -LiteralPath $output -Recurse -File | ForEach-Object {@{path=[IO.Path]::GetRelativePath($output,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant();size=$_.Length}}
$records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output 'build-manifest.json') -Encoding utf8
Write-Output "Built $output"
