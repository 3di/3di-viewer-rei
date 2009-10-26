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
 * Avatar class
 * 
 * @class
 */
opvw.API.Avatar = opvw.API.Avatar || {
	/**
	 * Get camera Field Of View.
	 * 
	 * @return (number) Field Of View.
	 */
	GetLoggedinAvatarList : function(){
		return opvw.API.plugin.GetLoggedinAvatarUUIDList();
	},
	/**	 * Get avatar name.
	 *
	 * @return (string) Using avatar name.
	 */
	GetUserAvatarName : function() {
		return opvw.API.plugin.GetUserAvatarName();
	},
	/**
	 * Get avatar current position.
	 * 
	 * @return (Object) { x:XXX.XXX, y:XXX.XXX, z:XXX.XXX }
	 */
	GetUserAvatarPosition : function() {
		var position = {};
		//GetUserAvatarPosition (string) current position with XXX.XXX,XXX.XXX,XXX.XXX value  ex) 123.456,1.000,123.456
		var posions = opvw.API.plugin.GetUserAvatarPosition().split(",");
		position["x"] = Number(posions[0]);
		position["y"] = Number(posions[1]);
		position["z"] = Number(posions[2]);
		return position;
	},
	/**	
	 * Get user avatar animation name.
	 * 
	 * @return (string) animation name
	 */
	GetUserAvatarAnimationName : function() {
		return opvw.API.plugin.GetUserAvatarAnimationName();
	},
	/**
	 * Get user acount UUID.
	 * 
	 * @return (string) user UUID
	 */
	GetUserUUID : function() {
		return opvw.API.plugin.GetUserUUID();
	},
	/**
	 * Get user avatar UUID. 
	 * 
	 * @return (string) avatar UUID
	 */
	GetUserAvatarUUID : function() {
		return opvw.API.plugin.GetUserAvatarUUID();
	},
	/**
	 * Make user avatar go forward.
	 */
	UserAvatarUp : function() {
		opvw.API.plugin.UserAvatarUp();
	},
	/**
	 * Make user avatar turn left.
	 */
	UserAvatarLeft : function() {
		opvw.API.plugin.UserAvatarLeft();
	},
	/**
	 * Make user avatar turn right.
	 */
	UserAvatarRight : function() {
		opvw.API.plugin.UserAvatarRight();
	},
	/**
	 * Make user avatar go back.
	 */
	UserAvatarDown : function() {
		opvw.API.plugin.UserAvatarDown();
	},
	/**
 	 * Start specified custom animation with index number
 	 * 
	 * @param {number} index Range 00-20
	 */
	StartCustomAnimation : function(index) {
		return opvw.API.plugin.StartCustomAnimation(index);
	}
};
opvw.APIWrapper["GetLoggedinAvatarList"] = opvw.API.Avatar.GetLoggedinAvatarList;
opvw.APIWrapper["GetUserAvatarName"] = opvw.API.Avatar.GetUserAvatarName;
opvw.APIWrapper["GetUserAvatarPosition"] = opvw.API.Avatar.GetUserAvatarPosition;
opvw.APIWrapper["GetUserUUID"] = opvw.API.Avatar.GetUserUUID;
opvw.APIWrapper["GetUserAvatarUUID"] = opvw.API.Avatar.GetUserAvatarUUID;
opvw.APIWrapper["GetUserAvatarAnimationName"] = opvw.API.Avatar.GetUserAvatarAnimationName;
opvw.APIWrapper["UserAvatarUp"] = opvw.API.Avatar.UserAvatarUp;
opvw.APIWrapper["UserAvatarLeft"] = opvw.API.Avatar.UserAvatarLeft;
opvw.APIWrapper["UserAvatarRight"] = opvw.API.Avatar.UserAvatarRight;
opvw.APIWrapper["UserAvatarDown"] = opvw.API.Avatar.UserAvatarDown;
opvw.APIWrapper["StartCustomAnimation"] = opvw.API.Avatar.StartCustomAnimation;

/*
 * EVENT TEMPLATE 
 */

/**
 * This function is called when an avatar is picked up and provides the avatar user data. 
 *  
 * @param {Object} json { UUID : XXX, NAME : { FIRST : CCC, LAST : CCC } }
 *  UUID  UUID of pciked avatar user
 *  NAME.FIRST  FirstName of pciked avatar user
 *  NAME.LAST  LastName of pciked avatar user
 */
//function OnAvatarPicked(json);