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
 * BuiltinEvent class
 * 
 * This class is automatically included to enable build in events.
 * 
 * @class
 */
opvw.API.BuiltinEvent = opvw.API.BuiltinEvent || {
	
	/**
	 * When the viewer handle openWindow event called,
	 * This function will notify the data.
	 * 
	 * @param {string} target  Window target
	 * @param {string} uri  Target uri
	 */
	OnOpenWindow : function(target, uri) {
		window.open(uri, target);
	}
	
};
