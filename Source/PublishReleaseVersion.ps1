#Declaring & setting some variables
$scriptPath = split-path -parent $MyInvocation.MyCommand.Definition
$publishPath = $scriptPath+"\_PublishTemp_\"

#Checking destination folder first
if (($env:dcsfpReleaseDestinationFolderPath -eq $null) -or (-not (Test-Path $env:dcsfpReleaseDestinationFolderPath))){
	Write-Host "Fatal error. Destination folder does not exists. Please set environment variable 'dcsfpReleaseDestinationFolderPath' to a valid value" -foregroundcolor "Red"
	exit
}

#---------------------------------
#Start of release version management
#---------------------------------
#Get Path to csproj
$projectFilePath = $scriptPath+"\DCSFlightpanels\DCSFlightpanels.csproj"
If(-not(Test-Path $projectPath)){
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

#---------------------------------
#End of release version management
#---------------------------------

#Calling Publish on project
Write-Host "Publishing release version" -foregroundcolor "Green"

dotnet publish DCSFlightpanels\DCSFlightpanels.csproj --self-contained false -f net6.0-windows -r win-x64 -c Release -o $publishPath /p:DebugType=None /p:DebugSymbols=false
$buildLastExitCode = $LastExitCode

Write-Host "Build LastExitCode: $buildLastExitCode" -foregroundcolor "Green"

if ( 0 -ne $buildLastExitCode )
{
  Write-Host "Fatal error. Build seems to have failed. No Zip & copy will be done." -foregroundcolor "Red"
  exit
}

#Getting file info & remove revision from file_version
Write-Host "Getting file info" -foregroundcolor "Green"
$file_version = (Get-Command $publishPath\dcsfp.exe).FileVersionInfo.FileVersion
Write-Host "File version is $file_version" -foregroundcolor "Green"

#Compressing release folder to destination
Write-Host "Destination for zip file:" $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip" -foregroundcolor "Green"
Compress-Archive -Force -Path $publishPath\* -DestinationPath $env:dcsfpReleaseDestinationFolderPath"\DCSFlightpanels_x64_$file_version.zip"

Write-Host "Script end" -foregroundcolor "Green"