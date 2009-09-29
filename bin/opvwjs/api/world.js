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
 * World class
 * 
 * @class
 */
opvw.API.World = opvw.API.World || {
	
	/**
	 * Get current region name.
	 *
	 * @return (string) current region name
	 */
	GetRegionName : function(){
		return opvw.API.plugin.GetRegionName();
	},
	/**
	 * Get viewer world time.
	 *
	 * @return (string) viewer world time
	 */
	GetWorldTime : function(){
		return opvw.API.plugin.GetWorldTime();
	},
	/**
	 * Set world time
	 *
	 * @param (string) world_time ex) 10:00:00 OR 2000-01-01 1:00:00
	 */
	SetWorldTime : function(world_time){
		opvw.API.plugin.SetWorldTime(world_time);
	},
	/**
	 * Set world time stopping / streaming flag.
	 *
	 * @param (boolean) flag true : streaming / false : stopping
	 */
	SetTickOn : function(flag){
		opvw.API.plugin.SetWorldTime(flag);
	},
	/**
	 * Set world ambient color
	 *
	 * @param (string) color RGB with 0.0 to 1.0 range ex) black 0.0,0.0,0.0 / white 1.0,1.0,1.0
	 */
	SetWorldAmbientColor : function(ambient_color){
		return opvw.API.plugin.SetWorldAmbientColor(ambient_color);
	}
};
opvw.APIWrapper["GetRegionName"] = opvw.API.World.GetRegionName;
opvw.APIWrapper["GetWorldTime"] = opvw.API.World.GetWorldTime;
opvw.APIWrapper["SetWorldTime"] = opvw.API.World.SetWorldTime;
opvw.APIWrapper["SetTickOn"] = opvw.API.World.SetTickOn;
opvw.APIWrapper["SetWorldAmbientColor"] = opvw.API.World.SetWorldAmbientColor;
