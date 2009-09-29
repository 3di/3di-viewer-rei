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
 * opvw namespase
 * 
 * This namespase support to load 'js' module.
 * 
 * @namespace
 */
var opvw = opvw || {
	
	// Public Static member
	BASE_MODULE_NAME : "opvwjs/opvw.js",
	
	// Public member
	context : null,
	
	// Private member
	_loadedModule : {},
	_basePath : null,
	
	// Public function
	/**
	 * Include a module.
	 * The path name of 'opvwjs' is not needed to be included. 
	 * If you want to use some 'js' module file in sub folders, you can include with following way:
	 *  ex) opvw.include("thirdparty/json2");
	 * NOTE : This function must be called on global scope only.
	 * 
	 * @param {string} module  Name of module. Never include ".jp" sufix  ex) opvw.voip
	 */
	include : function( module ){
		var doc = opvw.context.document;
		if( typeof doc != 'undefined' &&
			module != undefined && 
			!opvw._loadedModule[module] ){
			if( !opvw._basePath ){
				var scripts = doc.getElementsByTagName('script');
				for (var script, i = 0; script = scripts[i]; i++) {
					var src = script.src;
					var l = src.length;
					if (src.substr(l - opvw.BASE_MODULE_NAME.length) == opvw.BASE_MODULE_NAME) {
						opvw._basePath = src.substr(0, l - 7); // To reserve 'opvwjs'
					}
				}
			}
			var headTag = doc.getElementsByTagName("head")[0];
			var scriptTag = doc.createElement("script");
			scriptTag.setAttribute( "type", "text/javascript" );
			scriptTag.setAttribute( "src", opvw._basePath + module + ".js" );
			headTag.appendChild(scriptTag);
			opvw._loadedModule[module] = true;
		}
	},
	
	/**
	 * Returns true if the user's browser is Microsoft IE.
	 * 
	 * @private
	 * @return {boolean} true if the user's browser is Microsoft IE.
	 */
	isMSIE : function() {
	  var ua = navigator.userAgent.toLowerCase();
	  var msie = /msie/.test(ua) && !/opera/.test(ua);
	  return msie;
	}
	
};
opvw.context = this; // MUST SET FIRST!
opvw.include("thirdparty/json2");

/**
 * opvw.API
 * 
 * The namespace for API
 * 
 * @namespace
 */
opvw.API = opvw.API || {}; 

/**
 * API Wrapper class
 * 
 * This class provides delegated plug-in API.
 * 
 * @class
 */
opvw.APIWrapper = opvw.APIWrapper || {

	plugin : null,
	
	// Public function 
	toString : function(){
		return "opvw.APIWrapper";
	},
	
	/**
	 * Set Plugin element
	 * 
	 * @param {Element} plugin
	 */
	setPlugin : function(plugin){
		opvw.API.plugin = plugin;
	}
	
};

/**
 * Plugin class
 * 
 * This class should be used only one instance not to open 2 more plug-in at the same time.
 * 
 * @class
 */
opvw.include("api/builtin_event");
opvw.Plugin = opvw.Plugin || {
	
	// Public STATIC member
	PLUGIN_ID : "opvw_PLUGIN_ID",
	AX_SSETTING_ID : "opvw_AX_SSETTING_ID",
	CLASS_ID : "clsid:AB809708-8AA8-4aa8-9E31-7A16213F46CD",
	CODE_BASE : "http://3di-opensim.com/openviewer/product/3Di_OpenViewer.cab",
	
	// Public member
	plugin : null, // plugin
	container : null, // targeted container element
	eventList : {}, // all event listener object
	
	// Public function 
	toString : function(){
		return "opvw.Plugin";
	},
	
	/**
	 * Start OpenViewer plug-in
	 * This function inserts new "OBJECT" element into target element to set the plug-in.
	 * 
	 * @param {string} targetID  ID of setting target element object.
	 * @param {number} width  This value is prioritized over "WindowWidth"
	 * @param {number} height  This value is prioritized over "WindowHeight"
	 * @param {Object} values (optional)  Set proper value with proper key. Each value should be "String" type.
	 * 
	 * @return {Object} API Wrapper of plug-in
	 */
	start : function( targetID, width, height, values ){
		var doc = opvw.context.document;
		if( typeof doc != 'undefined' && 
			targetID != undefined ){
				
			var key = null;
			
			// Set up BuiltinEvent
			var baseEvents = opvw.API.BuiltinEvent;
			for ( key in baseEvents ){
				if ( baseEvents.hasOwnProperty(key) ) {
					opvw.Plugin.addEvent(key, opvw.API.BuiltinEvent[key]);
				}
			}
			
			var container = doc.getElementById( targetID );
			opvw.Plugin.container = container;
			var dummyParent = doc.createElement( "div" );
			if (opvw.isMSIE()) {	// for IE
				var dummyChild = doc.createElement( "div" );
				var objTag = doc.createElement( "object" );
				objTag.setAttribute( "id", opvw.Plugin.PLUGIN_ID );
				objTag.setAttribute( "type", "application/x-oleobject" );
				objTag.setAttribute( "classid", opvw.Plugin.CLASS_ID );
				objTag.setAttribute( "codebase", opvw.Plugin.CODE_BASE );
				objTag.setAttribute( "width", width );
				objTag.setAttribute( "height", height );
				// Param
				values = values != undefined ? values : new Object();
				values.WindowWidth = width;
				values.WindowHeight = height;
				for( key in values ){
					if( values.hasOwnProperty( key ) ){
						var paramTag = doc.createElement( "param" );
						paramTag.setAttribute( "name", key );
						paramTag.setAttribute( "value", values[key] );
						objTag.appendChild( paramTag );
					}
				}
				dummyChild.appendChild( objTag );
				
				// Register Event
				var data = ''
				var eventList = opvw.Plugin.eventList;
				for ( key in eventList ){
					if ( eventList.hasOwnProperty(key) ) {
						var argCnt = eventList[key].arguments != undefined ? eventList[key].arguments.length: 15;
						var paramTxt = "";
						for( var cnt = 0; cnt < argCnt; cnt++ ){
							paramTxt += "p" + cnt +",";
						}
						paramTxt = paramTxt.substr(0, paramTxt.length - 1);
						data += '<script language="javascript" type="text/jscript" for="' + opvw.Plugin.PLUGIN_ID + '" event="' + key + '(' + paramTxt + ')">';
						data += 'opvw.Plugin.eventList["' + key + '"](' + paramTxt + ');';
						data += '</script>';
					}
				}
				
				dummyParent.innerHTML = dummyChild.innerHTML + data; // HACK : IE can not show the plug-in with "appendChild" method.
				container.appendChild( dummyParent );
			}else{	// for Firefox
				var embedTag = doc.createElement( "embed" );
				embedTag.setAttribute( "id", opvw.Plugin.PLUGIN_ID );
				embedTag.setAttribute( "type", "application/x-3di-openviewer" );
				embedTag.setAttribute( "width", width );
				embedTag.setAttribute( "height", height );
				// Embed attribute
				values = values != undefined ? values : new Object();
				values.WindowWidth = width;
				values.WindowHeight = height;
				for( key in values ){
					if( values.hasOwnProperty( key ) ){
						embedTag.setAttribute( key, values[key] );
					}
				}
				dummyParent.appendChild( embedTag );
				container.appendChild( dummyParent );
				
				// Register Event
				embedTag.SetEventListener(opvw.Plugin.eventList);
			}
			opvw.Plugin.plugin = doc.getElementById( opvw.Plugin.PLUGIN_ID );
			
			// Make API wrapper
			var wrapper = opvw.APIWrapper;
			wrapper.setPlugin(opvw.Plugin.plugin);
			return wrapper;
		}
	},

	/**
	 * Add event listener with EventTypeKey and EventFunction
	 * If you want to use this function, it MUST be called before calling "opvw.Plugin.start" function. 
	 * 
	 * @param {string} eventType
	 * @param {Function} eventFunc
	 */
	addEvent : function( eventType, eventFunc ){
		if( eventType != undefined && 
			eventFunc != undefined ){
			opvw.Plugin.eventList[eventType] = eventFunc;
		}
	},
	
	/**
	 * Set all event listener. Whole event listener is replaced.
	 * If you want to use this function, it MUST be called before calling "opvw.Plugin.start" function. 
	 * 
	 * @param {Object} lists
	 */
	setAllEvents : function( lists ){
		if( lists != undefined ){
			opvw.Plugin.eventList = lists;
		}
	},
	
	/**
	 * load all API.
	 * If you want to use this function, it MUST be called between after calling "opvw.Plugin.setAllEvents/addEvent" and calling "opvw.Plugin.start". 
	 * Please use "opvw.include" function like in this code.
	 */
	loadAllAPI : function(){
		// NOTE : Don't need to include 'api/builtin_event' here!
		opvw.include("api/common");
		opvw.include("api/loginout");
		opvw.include("api/sit");
		opvw.include("api/teleport");
		opvw.include("api/textchat");
		opvw.include("api/touch");
		opvw.include("api/avatar");
		opvw.include("api/camera");
		opvw.include("api/world");
		opvw.include("api/lighting");
		opvw.include("api/voice");
		opvw.include("api/callback");
	}
	
};




