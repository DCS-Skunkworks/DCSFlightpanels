$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$msBuildPath = 'C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\msbuild'

Write-Host "Building release version " -foregroundcolor "Green"

cmd.exe /c $msBuildPath /target:Build /property:Configuration=Release /property:Platform=x64 'DCSFlightpanels.local.sln'
$buildLastExitCode = $LastExitCode

Write-Host "Build LastExitCode: $buildLastExitCode" -foregroundcolor "Green"

if ( 0 -ne $buildLastExitCode )
{
  Write-Host "Build seems to have failed. No Zip & copy will be done." -foregroundcolor "Red"
  exit
}

Write-Host "Getting file info" -foregroundcolor "Green"
$file_version = (Get-Command $scriptPath\DCSFlightpanels\bin\x64\Release\dcsfp.exe).FileVersionInfo.FileVersion
#Remove revision
$file_version = $file_version.Remove($file_version.LastIndexOf('.'))

Write-Host "Zip release folder & Copy to destination" -foregroundcolor "Green"
Compress-Archive -Force -Path $scriptPath\DCSFlightpanels\bin\x64\Release\* -DestinationPath "F:\My Drive\DCSFlightpanels Dev Test Folder\DCSFlightpanels_x64_$file_version.zip"

Write-Host "Script end" -foregroundcolor "Green"