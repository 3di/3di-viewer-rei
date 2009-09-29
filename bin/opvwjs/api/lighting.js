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
 * Lighting class
 * 
 * @class
 */
opvw.API.Lighting = opvw.API.Lighting || {
	
	/**
	 * Set flag to use fixed directional light
	 *
	 * @param (boolean) flag
	 */
	SetFixDirectional : function(flag){
		opvw.API.plugin.SetFixDirectional(flag);
	},
	/**
	 * Set fixed directinal rotation
	 *
	 * @param radius rotation for x,y,z axis  ex) 1.75,0,0
	 * 
	 * @param {number} x  rotation the axis  ex) 1.75
	 * @param {number} y  rotation the axis  ex) 1.75
	 * @param {number} z  rotation the axis  ex) 1.75
	 */
	SetFixDirectionalRotation : function(x, y, z){
		var rotation = x + "," + y + "," + z;
		opvw.API.plugin.SetFixDirectionalRotation(rotation);
	},
	/** 
	 * Set fixed directional diffuse color
	 *
	 * @param {number} r  color RGB with 0.0 to 1.0 range
	 * @param {number} g  color RGB with 0.0 to 1.0 range
	 * @param {number} b  color RGB with 0.0 to 1.0 range
	 */
	SetFixDirectionalDiffuseColor : function(r, g, b){
		var diffuse = r + "," + g + "," + b;
		opvw.API.plugin.SetFixDirectionalDiffuseColor(diffuse);
	},
	/**
	 * Set fixed directional ambient color
	 *
	 * @param {number} r  color RGB with 0.0 to 1.0 range
	 * @param {number} g  color RGB with 0.0 to 1.0 range
	 * @param {number} b  color RGB with 0.0 to 1.0 range
	 */
	SetFixDirectionalAmbientColor : function(r, g, b){
		var color = r + "," + g + "," + b;
		opvw.API.plugin.SetFixDirectionalAmbientColor(color);
	}
};
opvw.APIWrapper["SetFixDirectional"] = opvw.API.Lighting.SetFixDirectional;
opvw.APIWrapper["SetFixDirectionalRotation"] = opvw.API.Lighting.SetFixDirectionalRotation;
opvw.APIWrapper["SetFixDirectionalDiffuseColor"] = opvw.API.Lighting.SetFixDirectionalDiffuseColor;
opvw.APIWrapper["SetFixDirectionalAmbientColor"] = opvw.API.Lighting.SetFixDirectionalAmbientColor;
