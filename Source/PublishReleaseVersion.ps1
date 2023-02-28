#Declaring & setting some variables
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$publishPath = $scriptPath+"\_PublishTemp_\"

#---------------------------------
# Pre-checks
#---------------------------------
#Checking destination folder first
if (($env:dcsfpReleaseDestinationFolderPath -eq $null) -or (-not (Test-Path $env:dcsfpReleaseDestinationFolderPath))){
	Write-Host "Fatal error. Destination folder does not exists. Please set environment variable 'dcsfpReleaseDestinationFolderPath' to a valid value" -foregroundcolor "Red"
	exit
}

#---------------------------------
# Tests execution
#---------------------------------
Write-Host "Starting test execution" -foregroundcolor "Green"
$testPath = $scriptPath+"\Tests"
Set-Location -Path $testPath
dotnet test
$testsLastExitCode = $LastExitCode
Write-Host "Tests LastExitCode: $testsLastExitCode" -foregroundcolor "Green"
if ( 0 -ne $testsLastExitCode )
{
  Write-Host "Fatal error. Some unit tests failed." -foregroundcolor "Red"
  exit
}
Write-Host "Finished test execution" -foregroundcolor "Green"

#---------------------------------
# Release version management
#---------------------------------
Write-Host "Starting release version management" -foregroundcolor "Green"
#Get Path to csproj
$projectFilePath = $scriptPath+"\DCSFlightpanels\DCSFlightpanels.csproj"
If(-not(Test-Path $projectFilePath)){
	Write-Host "Fatal error. Project path not found: $projectPath" -foregroundcolor "Red"
	exit
}

#Readind project file
$xml = [xml](Get-Content $projectFilePath)
[string]$assemblyVersion = $xml.Project.PropertyGroup.AssemblyVersion

#Split the Version Numbers
$avMajor, $avMinor, $avBuild, $avRevision   = $assemblyVersion.Split('.')

Write-Host "Current assembly version is: $assemblyVersion" -foregroundcolor "Green"
Write-Host "Current Build is: $avBuild" -foregroundcolor "Green"

#Determining new build version based on the number of days since 1/1/2000
$startDate=[datetime] '01/01/2000 00:00'
$endDate= Get-Date
$ts = New-TimeSpan -Start $startDate -End $endDate
$calculatedBuild = $ts.Days
Write-Host "Calculated build is: $calculatedBuild" -foregroundcolor "Green"

#Eventual increase of revision number
if ($calculatedBuild -eq $avBuild) {
	Write-Host "Increasing revision" -foregroundcolor "Green"
	Write-Host "   From: $avRevision" -foregroundcolor "Green"
	$avRevision = [Convert]::ToInt32($avRevision.Trim(),10)+1
	Write-Host "   To:   $avRevision" -foregroundcolor "Green"
} else {    
	$avRevision = "1"
	Write-Host "Revision reset to $avRevision" -foregroundcolor "Green"
}

#Sets new version into Project 
#Warning: for this to work, since the PropertyGroup is indexed, AssemblyVersion must be in the FIRST Propertygroup (or else, change the index).
$xml.Project.PropertyGroup[0].AssemblyVersion = "$avMajor.$avMinor.$calculatedBuild.$avRevision".Trim()
[string]$assemblyVersion = $xml.Project.PropertyGroup.AssemblyVersion
Write-Host "New assembly version is $assemblyVersion" -foregroundcolor "Green"

#Saving project file
$xml.Save($projectFilePath)
Write-Host "Project file updated" -foregroundcolor "Green"
Write-Host "Finished release version management" -foregroundcolor "Green"

#---------------------------------
# Publish-Build & Zip
#---------------------------------
#Cleaning previous publish
Write-Host "Starting cleaning previous build" -foregroundcolor "Green"
Set-Location -Path $scriptPath
dotnet clean DCSFlightpanels\DCSFlightpanels.csproj -o $publishPath
dotnet clean ControlReference\ControlReference.csproj -o $publishPath

#Removing eventual previous non-splitted sample extensions
Write-Host "Starting Removing eventual previous non-splitted sample extensions" -foregroundcolor "Green"
remove-Item -Path $publishPath\Extensions\SamplePanelEventPlugin.dll -ErrorAction Ignore 


Write-Host "Starting Publish" -foregroundcolor "Green"
Set-Location -Path $scriptPath


Write-Host "Starting Publish ControlReference" -foregroundcolor "Green"
dotnet publish ControlReference\ControlReference.csproj --self-contained false -f net6.0-windows -r win-x64 -c Release -o $publishPath /p:DebugType=None /p:DebugSymbols=false
$buildLastExitCode = $LastExitCode

Write-Host "Build ControlRef LastExitCode: $buildLastExitCode" -foregroundcolor "Green"

if ( 0 -ne $buildLastExitCode )
{
  Write-Host "Fatal error. Build seems to have failed on ControlReference. No Zip & copy will be done." -foregroundcolor "Red"
  exit
}

Write-Host "Starting Publish DCSFP" -foregroundcolor "Green"
dotnet publish DCSFlightpanels\DCSFlightpanels.csproj --self-contained false -f net6.0-windows -r win-x64 -c Release -o $publishPath /p:DebugType=None /p:DebugSymbols=false
$buildLastExitCode = $LastExitCode

Write-Host "Build DCSFP LastExitCode: $buildLastExitCode" -foregroundcolor "Green"

if ( 0 -ne $buildLastExitCode )
{
  Write-Host "Fatal error. Build seems to have failed on DCSFP. No Zip & copy will be done." -foregroundcolor "Red"
  exit
}

# Copy SamplePanelEventPluginxxx.dll(s) to Extensions folder
Write-Host "Including SamplePanelEventPlugin.dll(s)" -foregroundcolor "Green"
Copy-Item -Path $publishPath\SamplePanelEventPlugin1.dll -Destination $publishPath\Extensions\SamplePanelEventPlugin1.dll
Copy-Item -Path $publishPath\SamplePanelEventPlugin2.dll -Destination $publishPath\Extensions\SamplePanelEventPlugin2.dll

#Getting file info & remove revision from file_version
Write-Host "Getting file info" -foregroundcolor "Green"
$file_version = (Get-Command $publishPath\dcsfp.exe).FileVersionInfo.FileVersion
Write-Host "File version is $file_version" -foregroundcolor "Green"

#Compressing release folder to destination
Write-Host "Destination for zip file:" $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip" -foregroundcolor "Green"
Compress-Archive -Force -Path $publishPath\* -DestinationPath $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip"

Write-Host "Finished publishing release version" -foregroundcolor "Green"

Write-Host "Script end" -foregroundcolor "Green"
