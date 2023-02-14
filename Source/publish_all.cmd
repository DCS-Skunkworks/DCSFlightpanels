cd %~dp0
powershell.exe -ExecutionPolicy Bypass -File build_script_ctrlref.ps1
powershell.exe -ExecutionPolicy Bypass -File build_script_dcsfp.ps1
pause