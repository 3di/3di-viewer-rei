var ctrl = null;
function init(){
	
	opvw.include("api/debug_3di_internal_only");
	
	var listener = {
	  OnDebugMessage : function(message) {
	    var debug_info = document.getElementById("js_debug_text_fix");
	    debug_info.value = message;
	  },
	  OnTouched : function(uuid) {
	    TouchEventHandler(uuid);
	  },
	  OnReceivedMessage : function() {
	    GetChatMessage();
	  },
	  OnReceivedInstantMessage : function(uuid, avatarName, message) {
	    GetInstantMessage(message);
	  },	  
	  OnTeleport : function(regionName, x, y, z) {
	    var touch_res = document.getElementById("touch_res");
	    touch_res.value = regionName + " X:" + x.toString() + " Y:" + y.toString() + " Z:" + z.toString();
	  },
	  OnTeleported : function(avatarUUID, avatarName, x, y, z) {
	    var touch_res = document.getElementById("touch_res");
	    touch_res.value = avatarName + " X:" + x.toString() + " Y:" + y.toString() + " Z:" + z.toString();
	  },
	  // No longer needed. it's automaticaly supported on API.
	  /*
	  OnOpenWindow : function(getTarget, getUri) {
	    window.open(getUri, getTarget);
	  },
	  */
	  OnStateChanged :  function(state) {
	    var state_message = document.getElementById("state_message");
	    state_message.value = state + ":" + opvw.API.Common.STATUS[state];
	  },
	  OnImageLoaded :  function(texturename) {
	    var val = document.getElementById("asset_loaded");
	    val.value = texturename;
	  },
	  
	  // OPVW 1.1 IE
	  OnAvatarPicked :  function(json) {
	    var target_avatar_uuid = document.getElementById("target_avatar_uuid");
	    var target_avatar_firstname = document.getElementById("target_avatar_firstname");
	    var target_avatar_lastname = document.getElementById("target_avatar_lastname");
	    var data = JSON.parse(json);
	    target_avatar_uuid.value = data.UUID;
	    target_avatar_firstname.value = data.NAME.FIRST;
	    target_avatar_lastname.value = data.NAME.LAST;
	  },
	  OnVoiceStateChanged :  function(state) {
	    var voice_state_message = document.getElementById("voice_state_message");
	    voice_state_message.value = state + ":" + opvw.API.NonSupported.VOIP_STATUS[state];
	  },
	  OnVoicePrivateChatStateChanged :  function(pstate) {
	    var voice_private_chat_state_message = document.getElementById("voice_private_chat_state_message");
	    voice_private_chat_state_message.value = pstate + ":" + opvw.API.Voice.PRIVATE_STATUS[pstate];
	  },
	  OnDispatch : function(action, message) {
	  	switch(action)
	  	{
			// Add your custom Dispatch handlers here
	  		case "OnMessage":
	  		break;
	  		case "OnVoiceStateChange":
	  			var voice_state_message = document.getElementById("voice_state_message");
	    		voice_state_message.value = message + ":" + opvw.API.NonSupported.VOIP_STATUS[parseInt(message)];
	  		break;
	  		default:
	  		break;
	  	}
	  }
	};
	var plugin = opvw.Plugin;
	plugin.CODE_BASE = "<!--__install_codebase__-->";
	/*
	plugin.addEvent( "OnDebugMessage", listener.OnDebugMessage );
	plugin.addEvent( "OnTouched", listener.OnTouched );
	plugin.addEvent( "OnReceivedMessage", listener.OnReceivedMessage );
	plugin.addEvent( "OnTeleport", listener.OnTeleport );
	plugin.addEvent( "OnTeleported", listener.OnTeleported );
	plugin.addEvent( "OnOpenWindow", listener.OnOpenWindow );
	plugin.addEvent( "OnStateChanged", listener.OnStateChanged );
	*/
	plugin.setAllEvents( listener );
	plugin.loadAllAPI();
	
	var setting = {};
	setting["AvatarNameType"] = 0;
	/*
	// General settings
	setting["BackgroundColor"] = "ffff0000";
	setting["ProgressColor"] = "ffff0000";
	setting["Locale"] = "jp";
	setting["DHTMLRelationEnable"] = "true";
	//setting["RequireVersion"] = "1.1.0.0";
	
	// Init settings
	setting["InitBackgroundURL"] = "http://zaki.asia/3di_openviewer_init.jpg";
	//setting["InitBackgroundMusicURL"] = "http://zaki.asia/3di_openviewer_bgm.ogg";
	
	// Login settings
	setting["LoginBackgroundURL"] = "http://zaki.asia/3di_openviewer_login.jpg";
	setting["LoginMode"] = "manual";
	setting["FirstName"] = "test";
	setting["LastName"] = "test";
	setting["Password"] = "testtest";
	setting["ServerURI"] = "10.0.1.81:10001";
	//setting["LoginLocation"] = "uri:10.0.1.81&128&128&128";
	
	// Draw setting
	setting["AvatarNameType"] = 0;
	setting["DrawSea"] = "false";
	setting["DrawTerrain"] = "false";
	setting["DrawSky"] = "false";
	setting["DrawMenu"] = "false";
	setting["SetStandUpIcon"] = "true";
	//setting["ShaderLevel"] = "low";
	
	// In-world time
	setting["TickOn"] = "false";
	setting["WorldTime"] = "12:00:00";
	setting["WorldAmbientColor"] = "0.0,0.0,0.0";
	
	// Directional light
	setting["IsFixDirectional"] = "true";
	setting["FixDirectionalDirection"] = "1.75,0,0";
	setting["FixDirectionalDiffuseColor"] = "1.0,0.4,0.4";
	setting["FixDirectionalAmbientColor"] = "0.2,0.08,0.08";
	
	// Camera
	setting["CameraStartDistance"] = "2.0";
	setting["CameraKeyWalkingDistance"] = "3.0";
	setting["CameraMinDistance"] = "0.1";
	setting["CameraMaxDistance"] = "5.0";
	setting["CameraFOV"] = "0.785";
	setting["CameraOffsetY"] = "10";
	setting["CameraMinAngleY"] = "0.001";
	setting["CameraMaxAngleY"] = "1.57";
	setting["CameraDefaultAngleY"] = "0.785";
	setting["CameraDefaultAngleX"] = "3.14";
	setting["AvatarDisappearDistance"] = "2";
	*/
	
	ctrl = plugin.start('js_object_fix', 800, 600, setting);
	CalcObject();
}




function CalcObject()
{
	margin_top = 0;
	margin_left = 16;
	input_height = 48;
	
	sy = document.getElementsByTagName("body")[0].scrollTop;
	width = document.body.clientWidth;
	var js_debug_text_fix = document.getElementById("js_debug_text_fix");
	js_debug_text_fix.style.width = width - (margin_left * 2);
	js_debug_text_fix.style.left = margin_left;
	js_debug_text_fix.style.top = sy + margin_top;

	js_object_fix.style.left = margin_left;
	js_object_fix.style.top = sy + margin_top + input_height;
	
	js_cotain_fix.style.top = margin_top + input_height;
	js_cotain_fix.style.left = 800 + (margin_left * 2);
}


//----------------------------------------------------------
// 0. Plugin Info
//----------------------------------------------------------
function GetPluginInfo()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	var info = ctrl.GetPluginInfo();
	debug_info.value = "Version:" + info.Version + " Type:" + info.Type;
}

//----------------------------------------------------------
// 1. Login / Logout
//----------------------------------------------------------
function Login()
{
	var first = document.getElementById("first");
	var last = document.getElementById("last");
	var password = document.getElementById("password");
	var server = document.getElementById("server");
	var loginLocation = document.getElementById("loginLocation");
	ctrl.Login(first.value, last.value, password.value, server.value, loginLocation.value);
}

function Logout()
{
	ctrl.Logout();
}

//----------------------------------------------------------
// 2. Touch
//----------------------------------------------------------
function TouchTo()
{
	var touch_res = document.getElementById("touch_res");
	ctrl.TouchTo(touch_res.value);
}

function TouchEventHandler(message)
{
	var touch_res = document.getElementById("touch_res");
	touch_res.value = message;
}

//----------------------------------------------------------
// 3. Sit
//----------------------------------------------------------
function SitOn()
{
	var touch_res = document.getElementById("touch_res");
	ctrl.SitOn(touch_res.value);
}

function StandUp()
{
	ctrl.StandUp();
}

//----------------------------------------------------------
// 4. Text chat
//----------------------------------------------------------
function SendChat()
{
	var chat_message = document.getElementById("chat_message");
	ctrl.SendChat(chat_message.value, 1);
}

function SendIM()
{
	var im_target = document.getElementById("im_target");
	var im_rcpt = document.getElementById("im_rcpt");
	ctrl.SendIM(im_target.value, im_rcpt.value);
}

function GetChatMessage()
{
    var chat_history_select = document.getElementById("chat_history");
	var listNum = chat_history_select.length;
	var num = ctrl.GetMessageHistoryLength() - listNum;
				
	for (var i=0; i<num; i++)
	{
		var addParam = document.createElement("OPTION");
		addParam.text = ctrl.GetMessageFromHistory(listNum + i);
		addParam.value = listNum + i;
		
		//chat_history.add(addParam,null);
		chat_history_select.appendChild( addParam );
	}
}

function GetInstantMessage(message)
{
    var rcpt = document.getElementById("im_rcpt");
    rcpt.value = message;
}

//----------------------------------------------------------
// 5. Teleport
//----------------------------------------------------------
function Teleport()
{
	var tel_region = document.getElementById("tel_region");
	var tel_x = document.getElementById("tel_x");
	var tel_y = document.getElementById("tel_y");
	var tel_z = document.getElementById("tel_z");
	ctrl.TeleportTo(tel_region.value, tel_x.value, tel_y.value, tel_z.value);
}

//----------------------------------------------------------
// 6.
//----------------------------------------------------------

//----------------------------------------------------------
// 7. Useravatar function
//----------------------------------------------------------
function GetLoggedinAvatarList()
{
	var info = document.getElementById("loggedin_avatar_list");
	info.value = ctrl.GetLoggedinAvatarList();
}

function StartCustomAnimation()
{
	var avatar_anim_index = document.getElementById("avatar_anim_index");
	ctrl.StartCustomAnimation(avatar_anim_index.value);
}

function GetUserAvatarName()
{
	var debug_info = document.getElementById("user_avatar_res");
	debug_info.value = ctrl.GetUserAvatarName();
}

function GetUserAvatarPosition()
{
	var debug_info = document.getElementById("user_avatar_res");
	position = ctrl.GetUserAvatarPosition();
	debug_info.value = "X:" + position.x + " Y:" + position.y + " Z:" + position.z;
}

function GetUserAvatarAnimation()
{
	var val = ctrl.GetUserAvatarAnimationName();
	debug_info.value = val;
}

function GetUserUUID()
{
	var debug_info = document.getElementById("user_avatar_res");
	debug_info.value = ctrl.GetUserUUID();
}
function GetUserAvatarUUID()
{
	var debug_info = document.getElementById("user_avatar_res");
	debug_info.value = ctrl.GetUserAvatarUUID();
}

function UserAvatarUp()
{
	ctrl.UserAvatarUp();
}

function UserAvatarLeft()
{
	ctrl.UserAvatarLeft();
}

function UserAvatarRight()
{
	ctrl.UserAvatarRight();
}

function UserAvatarDown()
{
	ctrl.UserAvatarDown();
}

//----------------------------------------------------------
// 8. Common sample
//----------------------------------------------------------
function GetFPS()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetFPS();
}

function GetPrimitiveCount()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetPrimitiveCount();
}

function GetTextureCount()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetTextureCount();
}

function RequestImage(imageUUID, useCache)
{
	ctrl.RequestImage(imageUUID, useCache);
}

function SetTexture(objectUUID, materialIndex, filename, requestEnable)
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.SetTexture(objectUUID, materialIndex, filename, requestEnable);
}
//----------------------------------------------------------
// 9. Camera lookAt
//----------------------------------------------------------
function CameraLookAt()
{
	ctrl.CameraLookAt(64.0, 48.0, 128.0, 128.0, 25.0, 128.0);
}

function GetCameraPosition()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraPosition();
}

function GetCameraTarget()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraTarget();
}

function GetCameraDistance()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraDistance();
}

function SetCameraDistance()
{
	var camera_distance = document.getElementById("camera_distance");
	ctrl.SetCameraDistance(camera_distance.value);
}

function GetCameraFOV()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraFOV();
}

function SetCameraFOV()
{
	var camera_fov = document.getElementById("camera_fov");
	ctrl.SetCameraFOV(camera_fov.value);
}

function SetCameraFOVDegree()
{
	var camera_fov = document.getElementById("camera_fov");
	ctrl.SetCameraFOVDegree(camera_fov.value);
}

function GetCameraOffsetY()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraOffsetY();
}

function SetCameraOffsetY()
{
	var camera_offset_y = document.getElementById("camera_offset_y");
	ctrl.SetCameraOffsetY(camera_offset_y.value);
}

function GetCameraAngleY()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetCameraAngleY();
}

function SetCameraAngleY()
{
	var camera_angle_min_y = document.getElementById("camera_angle_min_y");
	var camera_angle_max_y = document.getElementById("camera_angle_max_y");
	ctrl.SetCameraAngleY(camera_angle_min_y.value, camera_angle_max_y.value);
}

function SetAvatarDisappearDistance()
{
	var avatar_disappear_distance = document.getElementById("avatar_disappear_distance");
	ctrl.SetAvatarDisappearDistance(avatar_disappear_distance.value);
}

//----------------------------------------------------------
// 10. World
//----------------------------------------------------------
function GetAvatarCount()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetAvatarCount();
}

function GetObjectCount()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetObjectCount();
}

function GetRegionName()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetRegionName();
}

function GetWorldTime()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.GetWorldTime();
}

function SetWorldTime()
{
	var world_time = document.getElementById("world_time");
	ctrl.SetWorldTime(world_time.value);
}

function SetTick(_flag)
{
	ctrl.SetTickOn(_flag);
}

function SetWorldAmbientColor()
{
	var world_ambient_color = document.getElementById("world_ambient_color");
	ctrl.SetWorldAmbientColor(world_ambient_color.value);
}

//----------------------------------------------------------
// 11. Fix directional
//----------------------------------------------------------
function SetFixDirectional(_flag)
{
	ctrl.SetFixDirectional(_flag);
}

function SetFixDirectionalRotation()
{
	var fix_directional_rotation = document.getElementById("fix_directional_rotation");
	ctrl.SetFixDirectionalRotation(fix_directional_rotation.value);
}

function SetFixDirectionalDiffuseColor()
{
	var fix_directional_diffuse = document.getElementById("fix_directional_rotation");
	ctrl.SetFixDirectionalDiffuseColor(fix_directional_diffuse.value);
}

function SetFixDirectionalAmbientColor()
{
	var fix_directional_diffuse = document.getElementById("fix_directional_ambient");
	ctrl.SetFixDirectionalAmbientColor(fix_directional_ambient.value);
}

//----------------------------------------------------------
// 12. Voice
//----------------------------------------------------------
function StartVoiceChat()
{
	ctrl.Callback("voicechat_setenabled", "true");
}

function EndVoiceChat()
{
	ctrl.Callback("voicechat_setenabled", "false");
}

function IsVoiceEnabled()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = "" + ctrl.Callback("voicechat_isenabled", "");
}

function StartPrivateVoiceChat()
{
	var target_avatar_uuid = document.getElementById("target_avatar_uuid");
	ctrl.Callback("voicechat_calltargetuser", target_avatar_uuid.value);
}

function StopPrivateVoiceChat()
{
	ctrl.Callback("voicechat_callstop", "");
}

function IsVoiceMute()
{
	var debug_info = document.getElementById("js_debug_text_fix");
	debug_info.value = ctrl.Callback("voicechat_isvoicemute", "");
}

function VoiceMuteOn()
{
	ctrl.Callback("voicechat_setvoicemute", "true");
}

function VoiceMuteOff()
{
	ctrl.Callback("voicechat_setvoicemute", "false");
}

function VoiceMicVolume()
{
	var mic_volume_level = document.getElementById("mic_volume_level");
	ctrl.Callback("voicechat_setmicvolume", mic_volume_level.value);
}

function VoiceSpeakerVolume()
{
	var speaker_volume_level = document.getElementById("speaker_volume_level");
	ctrl.Callback("voicechat_setspeakervolume", speaker_volume_level.value);
}

function DoDamage()
{
	ctrl.Callback("damage", document.getElementById("damage_level").value);
}

function DoHeal()
{
	ctrl.Callback("heal", document.getElementById("heal_level").value); 
}
