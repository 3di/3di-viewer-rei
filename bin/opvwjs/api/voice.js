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
 * Voice class
 * 
 * @class
 */
opvw.API.Voice = opvw.API.Voice || {
	
	PRIVATE_STATUS : {
		0 : "Waiting",
		1 : "CallPreparing",
		2 : "Calling",
		3 : "Ringing",
		4 : "Talking",
		5 : "TalkEnd"
	},
	
	/**
	 * Enable voice chat
	 * After called this function, voice process starts to run.
	 */
	StartVoiceChat : function(){
		opvw.API.plugin.StartVoiceChat();
	},
	/**
	 * Disable voice chat
	 * After called this function, voice process is terminated.
	 */
	EndVoiceChat : function(){
		opvw.API.plugin.EndVoiceChat();
	},
	/**
	 * Test voice is enabled.
	 */
	IsVoiceEnabled : function(){
		return opvw.API.plugin.IsVoiceEnabled();
	},
	/**
	 * Start private voice chat.
	 * If still conecting/calling to someone, the session is terminated and start to call.
	 * 
	 * @param {string} uuid UUID of target user (NOT avatar uuid)
	 */
	StartPrivateVoiceChat : function(uuid){
		return opvw.API.plugin.StartPrivateVoiceChat(uuid);
	},
	/**
	 * Caller can cancel the calling session.
	 * Once sessioin is created, both side can stop private session with this function.
	 */
	StopPrivateVoiceChat : function(){
		return opvw.API.plugin.StopPrivateVoiceChat();
	},
	/**
	 * Test voice is muted
	 */
	IsVoiceMute : function(){
		return opvw.API.plugin.IsVoiceMute();
	},
	/**
	 * Set voice volume mute on
	 */
	SetVoiceMuteOn : function(){
		return opvw.API.plugin.SetVoiceMuteOn();
	},
	/**
	 * Set voice volume mute off
	 */
	SetVoiceMuteOff : function(){
		return opvw.API.plugin.SetVoiceMuteOff();
	},
	/**
	 * Set mic volume
	 * @param (numbre) level (0.0:Min - 1.0:Max)
	 */
	SetMicVolume : function(level){
		return opvw.API.plugin.SetMicVolume(level);
	},
	/**
	 * Set speaker volume
	 * @param (number) level (0.0:Min - 1.0:Max)
	 */
	SetSpeakerVolume : function(level){
		return opvw.API.plugin.SetSpeakerVolume(level);
	}
};
opvw.APIWrapper["StartVoiceChat"] = opvw.API.Voice.StartVoiceChat;
opvw.APIWrapper["EndVoiceChat"] = opvw.API.Voice.EndVoiceChat;
opvw.APIWrapper["IsVoiceEnabled"] = opvw.API.Voice.IsVoiceEnabled;
opvw.APIWrapper["StartPrivateVoiceChat"] = opvw.API.Voice.StartPrivateVoiceChat;
opvw.APIWrapper["StopPrivateVoiceChat"] = opvw.API.Voice.StopPrivateVoiceChat;
opvw.APIWrapper["IsVoiceMute"] = opvw.API.Voice.IsVoiceMute;
opvw.APIWrapper["SetVoiceMuteOn"] = opvw.API.Voice.SetVoiceMuteOn;
opvw.APIWrapper["SetVoiceMuteOff"] = opvw.API.Voice.SetVoiceMuteOff;
opvw.APIWrapper["SetMicVolume"] = opvw.API.Voice.SetMicVolume;
opvw.APIWrapper["SetSpeakerVolume"] = opvw.API.Voice.SetSpeakerVolume;

/*
 * EVENT TEMPLATE 
 */

/**
 * This function is called when private voice chat status will be changed into followings:
 * 
 * @param (number) pstate  
 *   0: Waiting
 *   1: CallPreparing
 *   2: Calling
 *   3: Ringing
 *   4: Talking
 *   5: TalkEnd
 */
//function OnVoicePrivateChatStateChanged(pstate);