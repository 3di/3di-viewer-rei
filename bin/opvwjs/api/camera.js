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
 * Camera class
 * 
 * @class
 */
opvw.API.Camera = opvw.API.Camera || {

	/**
	 * Set camera position according to the camera position and target position. 
	 * The focus is fixed. 
	 * CAUTION : This Y means SIM's Z axis and Z means SIM's Y axis.
	 *
	 * @param (number) cameraX camera position X on plugin internal axis
	 * @param (number) cameraY camera position Y on plugin internal axis
	 * @param (number) cameraZ camera position Z on plugin internal axis
	 * @param (number) targetX target position X on plugin internal axis
	 * @param (number) targetY target position Y on plugin internal axis
	 * @param (number) targetZ target position Z on plugin internal axis
	 */
	CameraLookAt : function(cameraX,cameraY,cameraZ,targetX,targetY,targetZ){
		opvw.API.plugin.CameraLookAt(cameraX,cameraY,cameraZ,targetX,targetY,targetZ);
	},
	/**
	 * Get camera position.
	 * 
	 * @return (Object) camera x, y, z float positions { x:XXX.XXX, y:XXX.XXX, z:XXX.XXX }
	 */
	GetCameraPosition : function(){
		//x, y, z float positions with comma delimiter: 123.456, 123.456, 123.456
		var position = opvw.API.plugin.GetCameraPosition().split(",");
		position["x"] = Number(position[0]);
		position["y"] = Number(position[1]);
		position["z"] = Number(position[2]);
		return position;
	},
	/**
	 * Get camera position.
	 * 
	 * @return (Object) camera target x, y, z float positions { x:XXX.XXX, y:XXX.XXX, z:XXX.XXX }
	 */
	GetCameraTarget : function(){
		//x, y, z float positions with comma delimiter: 123.456, 123.456, 123.456
		var position = opvw.API.plugin.GetCameraTarget().split(",");
		position["x"] = Number(position[0]);
		position["y"] = Number(position[1]);
		position["z"] = Number(position[2]);
		return position;
	},
	GetCameraDistance : function(){
		return opvw.API.plugin.GetCameraDistance();
	},
	/**
	 * Set camera distance.
	 * If you set out of range number of MaxDistance between MinDistance,
	 * this function do nothing.
	 * Also if you set smaller than zero, the distance will be set as 0.1.
	 * 
	 * @param {number} distance  0 <= d 
	 */
	SetCameraDistance : function(distance){
		return opvw.API.plugin.SetCameraDistance(distance);
	},
	/**
	 * Get camera Field Of View.
	 * 
	 * @return (number) Field Of View.
	 */
	GetCameraFOV : function(){
		return Number(opvw.API.plugin.GetCameraFOV());
	},
	/**
	 * Set camera Field Of View with radian.
	 * 
	 * @param (number) fov Camera Field Of View.
	 */
	SetCameraFOV : function(fov){
		opvw.API.plugin.SetCameraFOV(fov);
	},
	/**
	 * Set camera Field Of View with degree.
	 * 
	 * @param (numbre) fov Field Of View.
	 */
	SetCameraFOVDegree : function(fov){
		opvw.API.plugin.SetCameraFOVDegree(fov);
	},
	/**
	 * Get camera offset Y.
	 * 
	 * @return (number) Camera offset Y.
	 */
	GetCameraOffsetY : function(){
		return opvw.API.plugin.GetCameraOffsetY();
	},
	/**
	 * Set camera offset Y.
	 * 
	 * @param (number) offsetY Camera offset Y.
	 */
	SetCameraOffsetY : function(offset_y){
		opvw.API.plugin.SetCameraOffsetY(offset_y);
	},
	/**
	 * Get camera angle Y.
	 *
	 * @return (number) camera angleY min,max ex) 0.000,3.000
	 */
	GetCameraAngleY : function(){
		return opvw.API.plugin.GetCameraAngleY();
	},
	/**
	 * Set camera angle Y. 
	 * Each parameter can be set in range from 0.0 to PI(3.14..).
	 *
	 * @param (number) min Camera minimum angle (top direction on avatar)
	 * @param (number) min Camera maxmum angle (bottom direction of avatar)
	 */
	SetCameraAngleY : function(min_y,max_y){
		return opvw.API.plugin.SetCameraAngleY(min_y,max_y);
	},
	/**
	 * Set avatar disappear distance.
	 *
	 * @param (number) distance meter
	 */
	SetAvatarDisappearDistance : function(distance){
		return opvw.API.plugin.SetAvatarDisappearDistance(distance);
	}
};
opvw.APIWrapper["CameraLookAt"] = opvw.API.Camera.CameraLookAt;
opvw.APIWrapper["GetCameraPosition"] = opvw.API.Camera.GetCameraPosition;
opvw.APIWrapper["GetCameraTarget"] = opvw.API.Camera.GetCameraTarget;
opvw.APIWrapper["GetCameraDistance"] = opvw.API.Camera.GetCameraDistance;
opvw.APIWrapper["SetCameraDistance"] = opvw.API.Camera.SetCameraDistance;
opvw.APIWrapper["GetCameraFOV"] = opvw.API.Camera.GetCameraFOV;
opvw.APIWrapper["SetCameraFOV"] = opvw.API.Camera.SetCameraFOV;
opvw.APIWrapper["SetCameraFOVDegree"] = opvw.API.Camera.SetCameraFOVDegree;
opvw.APIWrapper["GetCameraOffsetY"] = opvw.API.Camera.GetCameraOffsetY;
opvw.APIWrapper["SetCameraOffsetY"] = opvw.API.Camera.SetCameraOffsetY;
opvw.APIWrapper["GetCameraAngleY"] = opvw.API.Camera.GetCameraAngleY;
opvw.APIWrapper["SetCameraAngleY"] = opvw.API.Camera.SetCameraAngleY;
opvw.APIWrapper["SetAvatarDisappearDistance"] = opvw.API.Camera.SetAvatarDisappearDistance;
