if($args[0] -match "Debug"){		
	cd C:\dev\visualstudio\Projects\FlightPanels\ProUsbPanels\bin\x64\Debug
	del C:\dev\misc\share_laptop\*.*
	copy DCS-BIOS.dll C:\dev\misc\share_laptop\.
	copy CommonClassLibraryJD.dll C:\dev\misc\share_laptop\.
	copy HidLibrary.dll C:\dev\misc\share_laptop\.
	copy NonVisuals.dll C:\dev\misc\share_laptop\.
	#copy Antlr3.Runtime.dll C:\dev\misc\share_laptop\.
	#copy NCalc.dll C:\dev\misc\share_laptop\.
	copy Jace.dll C:\dev\misc\share_laptop\.
	copy Newtonsoft.Json.dll C:\dev\misc\share_laptop\.
	#copy DirectOutput.dll C:\dev\misc\share_laptop\.	
	copy ProUsbPanels.exe C:\dev\misc\share_laptop\.		
}


exit 0