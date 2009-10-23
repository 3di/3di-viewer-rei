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
 * TextChat class
 * 
 * @class
 */
opvw.API.TextChat = opvw.API.TextChat || {
	
	/**
	 * Let user avatar sit on specified sitable object.
	 * 
	 * @param (string) message  Chat message
	 * @param (string) range  Range of spread area
	 *  0 : whisper
	 *  1 : say
	 *  2 : shout
	 */
	SendChat : function( message, range ){
		opvw.API.plugin.SendChat( message, range );
	},
	
	/**
	 * Get specified message from message history with index count.
	 * The index will start from zero(0) and oldest message should be stored with zero index.
	 *
	 * @param (number) index Index count of stored message count.
	 * @return (string) message
	 */
	GetMessageFromHistory : function( index ){
		return opvw.API.plugin.GetMessageFromHistory(index);
	},
	
	/**
	 * Send a user an instant message
	 * 
	 * @param (string) uuid
	 * @param (string) message  Chat message
	 */
	SendIM : function( uuid, message ){
		opvw.API.plugin.SendIM( uuid, message );
	},	
	
	/**
	 * Get all stored message count.
	 * 
	 * @return (number) all message count.
	 */
	GetMessageHistoryLength : function(){
		return opvw.API.plugin.GetMessageHistoryLength();
	}
	
};
opvw.APIWrapper["SendChat"] = opvw.API.TextChat.SendChat;
opvw.APIWrapper["SendIM"] = opvw.API.TextChat.SendIM;
opvw.APIWrapper["GetMessageFromHistory"] = opvw.API.TextChat.GetMessageFromHistory;
opvw.APIWrapper["GetMessageHistoryLength"] = opvw.API.TextChat.GetMessageHistoryLength;

/*
 * EVENT TEMPLATE 
 */

/**
 * When a user receive text chat message in In-world, 
 * this function will notify the reseived message.
 *
 * @param (string) uuid UUID of avatar
 * @param (string) avatarName Name of avatar
 * @param (string) message Received message
 */
//function OnReceivedMessage( uuid, avatarName, message );


