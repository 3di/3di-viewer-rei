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
 * Teleport class
 * 
 * @class
 */
opvw.API.Teleport = opvw.API.Teleport || {
	
	/**
	 * Teleport to specified location.
	 *
	 * @param (string) regionName
	 * @param (number) x  X axsis position of SIM  0 <= X <= 255
	 * @param (number) y  Y axsis position of SIM  0 <= Y <= 255
	 * @param (number) z  Z axsis position of SIM  0 <= Z <= 10000
	 */
	TeleportTo : function( regionName, x, y, z ){
		opvw.API.plugin.TeleportTo( regionName, x, y, z );
	}
	
};
opvw.APIWrapper["TeleportTo"] = opvw.API.Teleport.TeleportTo;

/*
 * EVENT TEMPLATE 
 */

/**
 * When a user start to teleport somewhere,
 * this function will notify.
 *
 * @param (string) regionName
 * @param (number) x  X axsis position of SIM  0 <= X <= 255
 * @param (number) y  Y axsis position of SIM  0 <= Y <= 255
 * @param (number) z  Z axsis position of SIM  0 <= Z <= 10000
 */
//function OnTeleport( regionName, x, y, z ); 

/**
 * When a user suceeded to teleport somewhere, 
 * this function will notify.
 * 
 * @param (string) uuid  UUID of avatar
 * @param (string) avatarName  Name of avatar
 * @param (number) x  X axsis position of SIM  0 <= X <= 255
 * @param (number) y  Y axsis position of SIM  0 <= Y <= 255
 * @param (number) z  Z axsis position of SIM  0 <= Z <= 10000
 */
//function OnTeleported( uuid, avatar, x, y, z );
