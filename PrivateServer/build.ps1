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
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'Server') -Destination (Join-Path $output 'Server') -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'docs') -Destination (Join-Path $output 'docs') -Recurse
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'README.md') -Destination (Join-Path $output 'README.md')
New-Item -ItemType Directory -Path (Join-Path $output 'Runtime'),(Join-Path $output 'ContentPacks') | Out-Null
if($RuntimeDirectory){
    foreach($name in @('node.exe','LICENSE-Node.txt','cloudflared.exe','LICENSE-cloudflared.txt')){
        $file=Join-Path $RuntimeDirectory $name
        if(!(Test-Path -LiteralPath $file -PathType Leaf)){throw "Missing runtime dependency/notice: $name"}
        Copy-Item -LiteralPath $file -Destination (Join-Path $output ('Runtime/'+$name))
    }
}else{Write-Warning 'Supply Node.js and cloudflared with their licenses in Runtime, or rebuild with -RuntimeDirectory. This folder cannot run the backend yet.'}
foreach($name in @('LICENSE','THIRD_PARTY_NOTICES.md')){if(Test-Path -LiteralPath (Join-Path $sourceRoot $name)){Copy-Item -LiteralPath (Join-Path $sourceRoot $name) -Destination (Join-Path $output $name)}}
$records=Get-ChildItem -LiteralPath $output -Recurse -File | ForEach-Object {@{path=[IO.Path]::GetRelativePath($output,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant();size=$_.Length}}
$records | ConvertTo-Json -Depth 4 | Set-Content -LiteralPath (Join-Path $output 'build-manifest.json') -Encoding utf8
Write-Output "Built $output"
