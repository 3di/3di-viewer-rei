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
 * Non supported class
 * 
 * This class is for 3Di internal debug use.
 * 
 * @class
 */
opvw.API.NonSupported = opvw.API.NonSupported || {
	
	VOIP_STATUS : {
		0 : "Initialize", 
		1 : "Disenable", 
		2 : "DaemonStart", 
		3 : "DaemonStarted", 
		4 : "DaemonJoin", 
		5 : "Idling", 
		6 : "RequestAccount", 
		7 : "ProvisionAccount", 
		8 : "ProvisioningAccount", 
		9 : "ProvisionedAccount", 
		10 : "ConnectorCreate", 
		11 : "ConnectorCreating", 
		12 : "ConnectorCreated", 
		13 : "AccountLogin", 
		14 : "AccountLoggingin", 
		15 : "AccountLoggedin", 
		16 : "ProvisionParcelInfo", 
		17 : "ProvisioningParcelInfo", 
		18 : "ProvisionedParcelInfo", 
		19 : "SessionCreate", 
		20 : "SessionCreating", 
		21 : "SessionCreated", 
		22 : "SessionConnect", 
		23 : "SessionConnecting", 
		24 : "SessionConnected", 
		25 : "Running", 
		26 : "Teleporting", 
		27 : "Teleported", 
		28 : "SessionTerminate", 
		29 : "SessionTerminating", 
		30 : "SessionTerminated", 
		31 : "AccountLogout", 
		32 : "AccountLoggingout"
	},
	
	/**
	 * Get current FPS
	 * 
	 * @debug non supported
	 */
	GetFPS : function(){
		return opvw.API.plugin.GetFPS();
	},
	/**
	 * Get triangle polygons count
	 * 
	 * @debug non supported
	 */
	GetPrimitiveCount : function(){
		return opvw.API.plugin.GetPrimitiveCount();
	},
	/**
	 * Get texture count
	 * 
	 * @debug non supported
	 */
    GetTextureCount : function(){
		return opvw.API.plugin.GetTextureCount();
	},
	/**
	 * Get avatar count in same region.
	 *
	 * @return (numbre) avatar count
	 */
	GetAvatarCount : function(){
		return opvw.API.plugin.GetAvatarCount();
	},
	/**
	 * Get object current in same region.
	 *
	 * @return (numbre) object count
	 */
	GetObjectCount : function(){
		return opvw.API.plugin.GetObjectCount();
	}
	
}
opvw.APIWrapper["GetFPS"] = opvw.API.NonSupported.GetFPS;
opvw.APIWrapper["GetPrimitiveCount"] = opvw.API.NonSupported.GetPrimitiveCount;
opvw.APIWrapper["GetTextureCount"] = opvw.API.NonSupported.GetTextureCount;
opvw.APIWrapper["GetAvatarCount"] = opvw.API.NonSupported.GetAvatarCount;
opvw.APIWrapper["GetObjectCount"] = opvw.API.NonSupported.GetObjectCount;

/*
 * EVENT TEMPLATE 
 */

/**
 * This function will be called when voice state will be changed into followings:
 * 
 * @param {number} state
 *    0 : "Initialize", 
 *    1 : "Disenable", 
 *    2 : "DaemonStart", 
 *    3 : "DaemonStarted", 
 *    4 : "DaemonJoin", 
 *    5 : "Idling", 
 *    6 : "RequestAccount", 
 *    7 : "ProvisionAccount", 
 *    8 : "ProvisioningAccount", 
 *    9 : "ProvisionedAccount", 
 *   10 : "ConnectorCreate", 
 *   11 : "ConnectorCreating", 
 *   12 : "ConnectorCreated", 
 *   13 : "AccountLogin", 
 *   14 : "AccountLoggingin", 
 *   15 : "AccountLoggedin", 
 *   16 : "ProvisionParcelInfo", 
 *   17 : "ProvisioningParcelInfo", 
 *   18 : "ProvisionedParcelInfo", 
 *   19 : "SessionCreate", 
 *   20 : "SessionCreating", 
 *   21 : "SessionCreated", 
 *   22 : "SessionConnect", 
 *   23 : "SessionConnecting", 
 *   24 : "SessionConnected", 
 *   25 : "Running", 
 *   26 : "Teleporting", 
 *   27 : "Teleported", 
 *   28 : "SessionTerminate", 
 *   29 : "SessionTerminating", 
 *   30 : "SessionTerminated", 
 *   31 : "AccountLogout", 
 *   32 : "AccountLoggingout"
 */
//function OnVoiceStateChanged(state);