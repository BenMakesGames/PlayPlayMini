$buildStart = Get-Date

dotnet clean -c Release
dotnet build -c Release

# Find only package files produced during this build
$packageFiles = Get-ChildItem -Path . -Recurse -Include *.nupkg, *.snupkg | Where-Object { $_.LastWriteTime -ge $buildStart }

if ($packageFiles.Count -eq 0) {
    Write-Host "No package files found."
    exit
}

Write-Host "`nFound $($packageFiles.Count) package file(s):"
$packageFiles | ForEach-Object { Write-Host "  $($_.FullName)" }

# Ask for destination path
while ($true) {
    $destPath = Read-Host "`nEnter path to copy package files to (or press Enter to skip)"

    if ([string]::IsNullOrWhiteSpace($destPath)) {
        Write-Host "Skipping file copy."
        break
    }

    if (Test-Path -Path $destPath -PathType Container) {
        foreach ($file in $packageFiles) {
            Copy-Item -Path $file.FullName -Destination $destPath
            Write-Host "Copied: $($file.Name)"
        }
        Write-Host "Done! Copied $($packageFiles.Count) file(s) to $destPath"
        break
    }
    else {
        Write-Host "Directory does not exist. Please enter a valid path."
    }
}

# Ask about publishing to NuGet
$nupkgFiles = $packageFiles | Where-Object { $_.Extension -eq ".nupkg" }

if ($nupkgFiles.Count -eq 0) {
    Write-Host "`nNo .nupkg files to publish."
    exit
}

$publish = Read-Host "`nPublish to NuGet? (y/N)"

if ($publish -eq "y" -or $publish -eq "Y") {
    # Check for API key in environment variable first
    $apiKey = $env:NUGET_API_KEY

    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        $apiKey = Read-Host "Enter NuGet API key (or set NUGET_API_KEY environment variable)"
    }
    else {
        Write-Host "Using API key from NUGET_API_KEY environment variable."
    }

    if ([string]::IsNullOrWhiteSpace($apiKey)) {
        Write-Host "No API key provided. Skipping publish."
    }
    else {
        foreach ($file in $nupkgFiles) {
            Write-Host "Publishing $($file.Name)..."
            dotnet nuget push $file.FullName --api-key $apiKey --source https://api.nuget.org/v3/index.json --skip-duplicate
            if ($LASTEXITCODE -eq 0) {
                Write-Host "  Published successfully."
            }
            else {
                Write-Host "  Failed to publish."
            }
        }
        Write-Host "`nDone publishing to NuGet."
    }
}
