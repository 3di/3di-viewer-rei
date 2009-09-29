/*
 * 3Di OpenViewer JavaScript Library 
 *  
 * Copyright 2009,  3Di,Inc.
 * All rights reserved.
 * 
 * This library provides communication support between 3Di OpenViewer Plug-in and DHTML.
 * The library will work on following environments:
 *  IE 6.0+
 *  Mozilla 3.0+
 *  
 */
/**
 * Common class
 * 
 * @class
 */
opvw.API.Common = opvw.API.Common || {
	
	STATUS : {
		0: "ENTRY",
		1: "INITIALIZING",
		2: "INITIALIZED",
		3: "LOGIN",
		4: "DOWNLOADING",
		5: "CONNECTED",
		6: "TELEPORT_REQUESTED",
		7: "TELEPORTING",
		8: "CLOSING",
		9: "ERROR",
		10: "EXITING"
	},
	/**
	 * Get plug-in information.
	 * 
	 * @return (Object) {"Version":"1.1.0.0","Type":"Firefox"} 
	 */
	GetPluginInfo : function(){
		// (JSON) plug-in info {"Version":"1.1.0.0","Type":"Firefox"}
		return JSON.parse(opvw.API.plugin.GetPluginInfo());
	}
	
};
opvw.APIWrapper["GetPluginInfo"] = opvw.API.Common.GetPluginInfo;


/*
 * EVENT TEMPLATE 
 */

/**
 * When internal status changes, this function will be shown current status.
 *
 * @param (number) status
 * 00: ENTRY
 * 01: INITIALIZING
 * 02: INITIALIZED
 * 03: LOGIN
 * 04: DOWNLOADING
 * 05: CONNECTED
 * 06: TELEPORT_REQUESTED
 * 07: TELEPORTING
 * 08: CLOSING
 * 09: ERROR
 * 10: EXITING
 */
//function OnStateChanged( status );
