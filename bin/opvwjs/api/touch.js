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
 * Touch class
 * 
 * @class
 */
opvw.API.Touch = opvw.API.Touch || {
	
	/**
	 * Touch to specified object
	 * 
	 * @param {string} uuid
	 */
	TouchTo : function( uuid ){
		opvw.API.plugin.TouchTo( uuid );
	}
	
};
opvw.APIWrapper["TouchTo"] = opvw.API.Touch.TouchTo;

/*
 * EVENT TEMPLATE 
 */
/**
 * When a user touch specified object in In-world, 
 * this function will notify the object UUID.
 *
 * @param (string) uuid Touched object UUID
 */
//function OnTouched( uuid );

