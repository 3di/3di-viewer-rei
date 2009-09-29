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
 * Callback class
 * 
 * @class
 */
opvw.API.Callback = opvw.API.Callback || {
	
	/**
	 * Generic callback
	 * 
	 * @param {string} action
	 * @param {string} message
	 */
	Callback : function( action, message ){
		return opvw.API.plugin.Callback( action, message );
	}
	
};
opvw.APIWrapper["Callback"] = opvw.API.Callback.Callback;

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

