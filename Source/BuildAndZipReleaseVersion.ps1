#declaring & setting some variables
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$msBuildExePath = &"${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe
$destinationFolderPath = 'F:\My Drive\DCSFlightpanels Dev Test Folder\'

Write-Host "Latest MsBuild path found is: $msBuildExePath"  -foregroundcolor "Green"
Write-Host "Building release version " -foregroundcolor "Green"

cmd.exe /c $msBuildExePath /verbosity:minimal /target:Build /property:Configuration=Release /property:Platform=x64 'DCSFlightpanels.local.sln'
$buildLastExitCode = $LastExitCode

Write-Host "Build LastExitCode: $buildLastExitCode" -foregroundcolor "Green"

if ( 0 -ne $buildLastExitCode )
{
  Write-Host "Build seems to have failed. No Zip & copy will be done." -foregroundcolor "Red"
  exit
}

Write-Host "Getting file info" -foregroundcolor "Green"
$file_version = (Get-Command $scriptPath\DCSFlightpanels\bin\x64\Release\dcsfp.exe).FileVersionInfo.FileVersion

#Remove revision from file_version
$file_version = $file_version.Remove($file_version.LastIndexOf('.'))

Write-Host "Zip release folder & Copy to destination" -foregroundcolor "Green"
Write-Host "Destination for zip file" -foregroundcolor "Green" $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip"
#Delete debug and error log before compression
if(Test-Path -Path $scriptPath\DCSFlightpanels\bin\x64\Release\DCSFlightpanels_debug_log.txt -PathType Leaf)
{
	Remove-Item $scriptPath\DCSFlightpanels\bin\x64\Release\DCSFlightpanels_debug_log.txt
}
if(Test-Path -Path $scriptPath\DCSFlightpanels\bin\x64\Release\DCSFlightpanels_error_log.txt -PathType Leaf)
{
	Remove-Item $scriptPath\DCSFlightpanels\bin\x64\Release\DCSFlightpanels_error_log.txt
}
Compress-Archive -Force -Path $scriptPath\DCSFlightpanels\bin\x64\Release\* -DestinationPath $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip"

Write-Host "Script end" -foregroundcolor "Green"