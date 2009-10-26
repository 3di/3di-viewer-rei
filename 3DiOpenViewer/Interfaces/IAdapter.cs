using System;
using System.Collections.Generic;
using System.Text;

namespace OpenViewer
{
    // Message Plugin->Plugin
    public delegate object MessageHandler(object parameters);
    public delegate string Callback(string message);
    public delegate void DispatchListener(string action, string message);
    public delegate void DebugMessageListener(string _message);
    public delegate void TouchToListener(string _uuid);
    public delegate void OnReceiveMessageListener(string _uuid, string _avatarName, string _message);
    public delegate string[] GetMessageHistoryListener();
    public delegate void TeleportToListener(string _regionName, int _x, int _y, int _z);
    public delegate void TeleportListener(string _uuid, string _avatar, int _x, int _y, int _z);
    public delegate void OpenWindowListener(string _target, string _uri);
    public delegate void AvatarPickListener(string _uuid);
    public delegate void StateChangedListener(int _state);
    public delegate void ImageDownloadedListener(string texname);


    public interface IAdapter
    {
        #region General function.
        void Initialize(IRefController _reference);
        void Cleanup();
        void Update();
        #endregion

        #region Events
        event TouchToListener OnTouched;
        event TouchToListener OnDebugMessage;
        event OnReceiveMessageListener OnReceiveMessage;
        event OnReceiveMessageListener OnReceiveInstantMessage;
        event TeleportToListener OnTeleport;
        event TeleportListener OnTeleported;
        event OpenWindowListener OnOpenWindow;
        event AvatarPickListener OnAvatarPicked;
        event ImageDownloadedListener OnImageLoaded;
        event StateChangedListener OnStateChanged;
        event DispatchListener OnDispatch;

        #endregion

        #region 0. Debug

        void CallDebugMessage(string _message);
        #endregion

        #region 1. Login / Logout
        /// <summary>
        /// Login with specified account data
        /// </summary>
        /// <param name="_firstName">firstName</param>
        /// <param name="_lastName">lastName</param>
        /// <param name="_password">password</param>
        /// <param name="_serverURL">serverURL http://login-server-url</param>
        /// <param name="_location">location  REGION_NAME/X/Y/Z</param>
        void CallLogin(string _firstName, string _lastName, string _password, string _serverURL, string _loginLocation);

        /// <summary>
        /// Logout immediately
        /// </summary>
        void CallLogout();
        #endregion

        #region 2. Touch

        /// <summary>
        /// Touch specified object.
        /// </summary>
        /// <param name="_uuid">Target object UUID</param>
        void CallTouchTo(string _uuid);
        
        /// <summary>
        /// When a user touch specified object in In-world,
        /// this function will notify the object UUID.
        /// </summary>
        /// <param name="_uuid">Touched object UUID</param>
        void CallTouched(string _uuid);

        #endregion

        #region 3. Sit / Stand
        /// <summary>
        /// Sit on specified SIT ball object.
        /// </summary>
        /// <param name="_uuid">Sit target object UUID</param>
        void CallSitOn(string _uuid);

        /// <summary>
        /// Stand up from specified SIT ball object.
        /// </summary>
         void CallStandUp();
        #endregion

        #region 4. Text Chat

        /// <summary>
        /// Send InstantMessage via DHTM
        /// </summary>
        /// <param name="_target_uuid">target user uuid</param>
        /// <param name="_message">message</param>
         void CallSendIM(string _target_uuid, string _message);

        /// <summary>
        /// Send text chat message via DHTM
        /// </summary>
        /// <param name="_message">Chat message</param>
        /// <param name="_range">Range of spread area
        /// 1 : whisper
        /// 2 : say
        /// 3 : shout
        /// </param>
         void CallSendChat(string _message, int _range);

        /// <summary>
        /// When a user receive text chat message in In-world,
        /// this function will notify the reseived message.
        /// </summary>
        /// <param name="_uuid">UUID of avatar</param>
        /// <param name="_avatarName">Name of avatar</param>
        /// <param name="_message">Received message</param>
         void CallReceiveMessaged(string _uuid, string _avatarName, string _message);

         void CallReceiveInstantMessaged(string _uuid, string _avatarName, string _message);

        /// <summary>
        /// Get all stored message count.
        /// </summary>
        /// <returns>Lenght</returns>
         int CallGetMessageHistoryLength();

        /// <summary>
        /// Get all messages from message history.
        /// </summary>
        /// <param name="_index">message's history number</param>
        /// <returns>message</returns>
         string CallGetMessageFromHistory(int _index);

        #endregion

        #region 5. Teleport

        /// <summary>
        /// Teleport to specified location.
        /// </summary>
        /// <param name="_regionName">regionName</param>
        /// <param name="_x">X axsis position</param>
        /// <param name="_y">Y axsis position</param>
        /// <param name="_z">Z axsis position</param>
         void CallTeleportTo(string _regionName, int _x, int _y, int _z);

        /// <summary>
        /// When a user receive someone/himself teleport started same sim in In-world,
        /// this function will notify the message.
        /// </summary>
        /// <param name="_regionName">regionName</param>
        /// <param name="_x">X axsis position</param>
        /// <param name="_y">Y axsis position</param>
        /// <param name="_z">Z axsis position</param>
         void CallTeleport(string _regionName, int _x, int _y, int _z);

        /// <summary>
        /// When a user receive someone/himself teleported same sim in In-world,
        /// this function will notify the message.
        /// </summary>
        /// <param name="_uuid">UUID of avatar</param>
        /// <param name="_avatar">Name of avatar</param>
        /// <param name="_x">X axsis position</param>
        /// <param name="_y">Y axsis position</param>
        /// <param name="_z">Z axsis position</param>
         void CallTeleported(string _uuid, string _avatar, int _x, int _y, int _z);

         void TeleportThread(object _obj);

         void TeleportedThread(object _obj);
        #endregion

        #region 6. LSL triggered html related manupuration

        /// <summary>
        /// Open browser window with specified uri.
        /// </summary>
        /// <param name="_target">Window target</param>
        /// <param name="_uri">Target uri</param>
         void CallOpenWindow(string _target, string _uri);
        #endregion

        #region 7. User avatar

         string CallGetLoggedinAvatarUUIDList();

         void CallAvatarPicked(string _avatarInformation);

         void CallAvatarCustomizeAnimation(int _index);

         string CallGetUserAvatarAnimationName();

         string CallGetUserUUID();

         string CallGetUserAvatarPosition();

         string CallGetUserAvatarUUID();

         string CallGetUserAvatarName();

         void CallUserAvatarUp(bool _flag);

         void CallUserAvatarDown(bool _flag);

         void CallUserAvatarLeft();

         void CallUserAvatarRight();

         void CallReceiveImage(string texname);

         void CallRequestImage(string _assetUUID, string _useCache);

         string CallSetTexture(string _objectUUID, int _materialIndex, string _filename, string _requestEnable);
        #endregion

        #region 8. Common

         void CallStateChanged(int _state);

         int CallGetFPS();

         int CallGetPrimitiveCount();

         int CallGetTextureCount();
        #endregion

        #region 9. Camera
         void CallCameraLookAt(float _px, float _py, float _pz, float _tx, float _ty, float _tz);

         void CallSetCameraDistance(float _distance);

         string CallGetCameraDistance();

        /// <summary>
        /// Get camera position.
        /// </summary>
        /// <returns>Lenght</returns>
         string CallGetCameraPosition();

        /// <summary>
        /// Get camera position.
        /// </summary>
        /// <returns>Lenght</returns>
         string CallGetCameraTarget();

         string CallGetCameraFOV();

         void CallSetCameraFOV(float _fov);

         void CallSetCameraFOVDegree(float _fov);

         string CallGetCameraOffsetY();

         void CallSetCameraOffsetY(float _offsetY);

         string CallGetCameraAngleY();

         void CallSetCameraAngleY(float _min, float _max);
        #endregion

        #region 10. World
         string CallGetAvatarCount();

         string CallGetObjectCount();

         string CallGetRegionName();

         string CallGetWorldTime();

         void CallSetWorldTime(string _dataTime);

         void CallSetTickOn(string _flag);

         void CallSetWorldAmbientColor(string _colors);
        #endregion

        #region 11. Fix directional
         void CallSetFixDirectional(string _flag);

         void CallSetFixDirectionalRotation(string _radRotation);

         void CallSetFixDirectionalDiffuseColor(string _colors);

         void CallSetFixDirectionalAmbientColor(string _colors);
        #endregion

        #region 13. Callback and Dispatch

        void RegisterMessage(string action, MessageHandler message);

        object SendMessage(string action, object parameters);

        // Callback JS->OV
        void RegisterCallback(string action, Callback callback);

        string RunCallback(string action, string message);
         
        // Dispatch OV->JS
        

        void Dispatch(string action, string message);
        #endregion
    }
}