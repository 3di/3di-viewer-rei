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
 * Sit class
 * 
 * @class
 */
opvw.API.Sit = opvw.API.Sit || {
	
	/**
	 * Let user avatar sit on specified sitable object.
	 * 
	 * @param {string} uuid
	 */
	SitOn : function( uuid ){
		opvw.API.plugin.SitOn( uuid );
	},
	
	/**
	 * Let user avatar stand up from current sitting object.
	 * 
	 * @param {string} uuid
	 */
	StandUp : function( uuid ){
		opvw.API.plugin.StandUp();
	}
	
};
opvw.APIWrapper["SitOn"] = opvw.API.Sit.SitOn;
opvw.APIWrapper["StandUp"] = opvw.API.Sit.StandUp;
