/*
 * Copyright (c) 2008-2009, 3Di, Inc. (http://3di.jp/) and contributors.
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of 3Di, Inc., nor the name of the 3Di Viewer
 *       "Rei" project, nor the names of its contributors may be used to
 *       endorse or promote products derived from this software without
 *       specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY 3Di, Inc. AND CONTRIBUTORS ``AS IS'' AND
 * ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
 * IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
 * PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL 3Di, Inc. OR THE
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
 * EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
 * PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * Additionally, portions of this file bear the following BSD-style license
 * from the IdealistViewer project (URL http://idealistviewer.org/):
 * 
 * Copyright (c) Contributors, http://idealistviewer.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenViewer Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
 * DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE
 * GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER
 * IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
 * OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
 * ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OpenMetaverse;
using OpenMetaverse.Rendering;
using OpenMetaverse.Packets;
using OpenMetaverse.Utilities;
using log4net;


namespace OpenViewer
{
    public class SLProtocol
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public delegate void GridConnected();
        public delegate void Chat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourcetype,
                                  string fromName, UUID id, UUID ownerid, Vector3 position);
        public delegate void LandPatch(Simulator sim, int x, int y, int width, float[] data);
        public delegate void NewAvatar(Simulator sim, Avatar avatar, ulong regionHandle, ushort timeDilation);
        public delegate void NewPrim(Simulator sim, Primitive prim, ulong regionHandle, ushort timeDilation);
        public delegate void Login(LoginStatus status, string message);
        public delegate void ObjectDeleted(Simulator sim, uint objectID);
        public delegate void ObjectUpdated(Simulator simulator, ObjectUpdate update, ulong regionHandle, ushort timeDilation);
        public delegate void SimConnected(Simulator sim);
        public delegate void AssetReceived(AssetDownload transfer, Asset asset);
        public delegate void ImageReceived(AssetTexture tex);
        public delegate void FriendsListchanged();
        public delegate void AvatarSitResponse(UUID objectID, bool autoPilot, Vector3 cameraAtOffset, Vector3 cameraEyeOffset, bool forceMouselook, Vector3 sitPosition, Quaternion sitRotation);
        public delegate void SimDisconnected(NetworkManager.DisconnectType reason, string message);
        public delegate void TeleportFinished(string message, AgentManager.TeleportStatus status, AgentManager.TeleportFlags flags);
        public delegate void CurrentSimChanged(Simulator PreviousSimulator);
        public delegate void LoadURL(string objectName, UUID objectID, UUID ownerID, bool ownerIsGroup, string message, string URL);
        public delegate void RegisterLoginResponseListener(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData);
        public delegate void EventQueueRunningListener(Simulator simulator);
        public delegate void AvatarAnimationListener(UUID objectID);
        public delegate void VolumeLevelListener(UUID sourceID, uint localID, float volumeLevel);

        // Sound delegate.
        public delegate void OnAttachSoundListener(UUID soundID, UUID ownerID, UUID objectID, float gain, byte flags);
        public delegate void OnAttachSoundGainChangeListener(UUID objectID, float gain);
        public delegate void OnPreloadSoundListener(UUID soundID, UUID ownerID, UUID objectID);
        public delegate void OnSoundTriggerListener(UUID soundID, UUID ownerID, UUID objectID, UUID parentID, float gain, ulong regionHandle, Vector3 position);

        public event NewAvatar OnNewAvatar;
        public event Chat OnChat;
        public event GridConnected OnGridConnected;
        public event SimConnected OnSimConnected;
        public event SimDisconnected OnSimDisconnected;
        public event LandPatch OnLandPatch;
        public event Login OnLogin;
        public event NewPrim OnNewPrim;
        public event ObjectDeleted OnObjectDeleted;
        public event ObjectUpdated OnObjectUpdated;
        public event AssetReceived OnAssetReceived;
        public event ImageReceived OnImageReceived;
        public event FriendsListchanged OnFriendsListChanged;
        public event AvatarSitResponse OnAvatarSitResponse;
        public event TeleportFinished OnTeleportFinished;
        public event CurrentSimChanged OnCurrentSimChanged;
        public event LoadURL OnLoadURL;
        public event RegisterLoginResponseListener OnRegisterLoginRespons;
        public event EventQueueRunningListener OnEventQueueRunning;
        public event AvatarAnimationListener OnAnimationUpdate;
        public event VolumeLevelListener OnVolumeLevel;

        // Sound event.
        public event OnAttachSoundListener OnAttachSound;
        public event OnAttachSoundGainChangeListener OnAttachSoundGainChange;
        public event OnPreloadSoundListener OnPreloadSound;
        public event OnSoundTriggerListener OnSoundTrigger;

        // Customize
        public event EventHandler OnTeleport;

        public string loginURI;
        public string firstName;
        public string lastName;
        public string username;
        public string password;
        public string startlocation;

        // received animations are stored here before being processed in the main frame loop
        public Dictionary<UUID, List<UUID>> AvatarAnimations = new Dictionary<UUID,List<UUID>>();

        public GridClient m_user;

        public SLProtocol()
        {
            m_user = new GridClient();

            m_user.Settings.USE_TEXTURE_CACHE = false;
            m_user.Settings.ALWAYS_DECODE_OBJECTS = false;            
            m_user.Settings.SEND_AGENT_THROTTLE = true;
            m_user.Settings.THROTTLE_OUTGOING_PACKETS = true;

            //-- Assets event.--------------------------------------------
            m_user.Assets.OnAssetReceived += assetReceivedCallback;
            m_user.Assets.OnImageReceived += imageReceivedCallback;

            //-- Friends event.--------------------------------------------
            m_user.Friends.OnFriendNamesReceived += Friends_OnFriendNamesReceived;
            m_user.Friends.OnFriendOnline += Friends_OnFriendOnline;
            m_user.Friends.OnFriendOffline += Friends_OnFriendOffline;

            //-- Network event.--------------------------------------------
            m_user.Network.OnConnected += gridConnectedCallback;
            m_user.Network.OnDisconnected += disconnectedCallback;
            m_user.Network.OnSimConnected += simConnectedCallback;
            m_user.Network.OnLogin += loginCallback;
            m_user.Network.OnLogin += loginStatusCallback;
            m_user.Network.OnCurrentSimChanged += Network_OnCurrentSimChanged;
            m_user.Network.OnEventQueueRunning += Network_OnEventQueueRunning;


            //-- Objects event.--------------------------------------------
            m_user.Objects.OnNewAvatar += newAvatarCallback;
            m_user.Objects.OnNewPrim += newPrim;
            m_user.Objects.OnObjectKilled += objectDeletedCallback;
            m_user.Objects.OnObjectUpdated += objectUpdatedCallback;

            //-- Objects event.--------------------------------------------
            m_user.Avatars.OnVolumeLevel += Avatars_OnVolumeLevel;


            //-- Self event.--------------------------------------------
            m_user.Self.OnChat += chatCallback;
            m_user.Self.OnAvatarSitResponse += avatarSitResponseCallback;
            m_user.Self.OnTeleport += Self_OnTeleport;
            m_user.Terrain.OnLandPatch += landPatchCallback;
            m_user.Self.OnLoadURL += OnLoadURLCallback;

            //-- Sound event.--------------------------------------------
            m_user.Sound.OnAttachSound += Sound_OnAttachSound;
            m_user.Sound.OnAttachSoundGainChange += Sound_OnAttachSoundGainChange;
            m_user.Sound.OnPreloadSound += Sound_OnPreloadSound;
            m_user.Sound.OnSoundTrigger += Sound_OnSoundTrigger;

            m_user.Network.RegisterCallback(OpenMetaverse.Packets.PacketType.AvatarAnimation, AvatarAnimationHandler);
            m_user.Network.RegisterLoginResponseCallback(RegisterLoginResponseHandler);

            //m_user.Settings.MULTIPLE_SIMS = true;
        }

        void Avatars_OnVolumeLevel(UUID sourceID, uint localID, float volumeLevel)
        {
            if (OnVolumeLevel != null)
                OnVolumeLevel(sourceID, localID, volumeLevel);
        }

        void Network_OnEventQueueRunning(Simulator simulator)
        {
            if (OnEventQueueRunning != null)
                OnEventQueueRunning(simulator);
        }

        void OnLoadURLCallback(string objectName, UUID objectID, UUID ownerID, bool ownerIsGroup, string message, string URL)
        {
            if (OnLoadURL != null)
                OnLoadURL(objectName, objectID, ownerID, ownerIsGroup, message, URL);
        }

        void Network_OnCurrentSimChanged(Simulator PreviousSimulator)
        {
            if (OnCurrentSimChanged != null)
                OnCurrentSimChanged(PreviousSimulator);
        }

        void Self_OnTeleport(string message, AgentManager.TeleportStatus status, AgentManager.TeleportFlags flags)
        {
            if (status == AgentManager.TeleportStatus.Finished || status == AgentManager.TeleportStatus.Failed)
            {
                if (OnTeleportFinished != null)
                    OnTeleportFinished(message, status, flags);
            }
        }

        private void avatarSitResponseCallback(UUID objectID, bool autoPilot, Vector3 cameraAtOffset, Vector3 cameraEyeOffset, bool forceMouselook, Vector3 sitPosition, Quaternion sitRotation)
        {
            if (OnAvatarSitResponse != null)
                OnAvatarSitResponse(objectID,autoPilot,cameraAtOffset,cameraEyeOffset,forceMouselook,sitPosition,sitRotation);
        }

        private void Friends_OnFriendOffline(FriendInfo friend)
        {
            if( OnFriendsListChanged != null )
                OnFriendsListChanged();
        }

        private void Friends_OnFriendOnline(FriendInfo friend)
        {
            if (OnFriendsListChanged != null)
                OnFriendsListChanged();
        }

        private void Friends_OnFriendNamesReceived(Dictionary<UUID, string> names)
        {
            if (OnFriendsListChanged != null)
                OnFriendsListChanged();
        }

        public Dictionary<UUID, FriendInfo> Friends
        {
            get
            {
                return m_user.Friends.FriendList.Dictionary;
            }
        }

        public void RegisterLoginResponseHandler(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData)
        {
            if (OnRegisterLoginRespons != null)
                OnRegisterLoginRespons(loginSuccess, redirect, message, reason, replyData);
        }

        public void AvatarAnimationHandler(OpenMetaverse.Packets.Packet packet, Simulator sim)
        {
            // When animations for any avatar are received put them in the AvatarAnimations dictionary
            // in this module. They should be processed and deleted inbetween frames in the main frame loop
            // or deleted when an avatar is deleted from the scene.
            AvatarAnimationPacket animation = (AvatarAnimationPacket)packet;

            UUID avatarID = animation.Sender.ID;
            List<UUID> currentAnims = new List<UUID>();

            for (int i = 0; i < animation.AnimationList.Length; i++)
            {
                currentAnims.Add(animation.AnimationList[i].AnimID);

                VUtil.LogConsole(this.ToString(), "Ani:" + animation.AnimationList[i].AnimID.ToString());
            }

            lock (AvatarAnimations)
            {
                if (AvatarAnimations.ContainsKey(avatarID))
                    AvatarAnimations[avatarID] = currentAnims;
                else
                    AvatarAnimations.Add(avatarID, currentAnims);
            }

            if (OnAnimationUpdate != null)
                OnAnimationUpdate(avatarID);
        }

        public void loginStatusCallback(LoginStatus login, string message)
        {
            if (login == LoginStatus.Failed)
            {
                m_log.ErrorFormat("[CONNECTION]: Login Failed:{0}",message);
            }
        }

        private void assetReceivedCallback(AssetDownload transfer, Asset asset)
        {
            if (OnAssetReceived != null)
                OnAssetReceived(transfer, asset);
        }

        private void imageReceivedCallback(ImageDownload image, AssetTexture asset)
        {
            if (OnImageReceived != null)
                OnImageReceived(asset);
        }

        private void objectDeletedCallback(Simulator simulator, uint objectID)
        {
            if (OnObjectDeleted != null)
                OnObjectDeleted(simulator, objectID);
        }

        public void BeginLogin(string loginURI, string username, string password, string startlocation)
        {

            string firstname;
            string lastname;

            this.loginURI = loginURI;
            this.username = username;
            this.password = password;
            this.startlocation = startlocation;

            Util.separateUsername(username, out firstname, out lastname);

            this.firstName = firstname;
            this.lastName = lastname;


            LoginParams loginParams = getLoginParams(loginURI, username, password, startlocation);
            loginParams.Version = "realXtend";

            try
            {
                m_user.Network.Begin3DiLogin(loginParams);
            }
            catch (Exception e)
            {
                m_log.Error("[BEGIN LOGIN] " + e.ToString());
            }
        }

        public void VoiceEffect(float _volumeLevel)
        {
            m_user.Self.VoiceEffect(_volumeLevel);
        }

        public void RequestImage(UUID uuid, AssetType assetType, bool priority)
        {
            m_user.Assets.RequestImage(uuid, ImageType.Normal);
        }

        private void gridConnectedCallback(object sender)
        {
           
            m_user.Appearance.SetPreviousAppearance(false);



            if (OnGridConnected != null)
                OnGridConnected();
        }

        private void simConnectedCallback(Simulator sender)
        {
            m_user.Throttle.Total = 600000;
            m_user.Throttle.Land = 80000;
            m_user.Throttle.Task = 200000;
            m_user.Throttle.Texture = 100000;
            m_user.Throttle.Wind = 10000;
            m_user.Throttle.Resend = 100000;
            m_user.Throttle.Asset = 100000;
            m_user.Throttle.Cloud = 10000;
            m_user.Self.Movement.Camera.Far = 64f;
            m_user.Self.Movement.Camera.Position = m_user.Self.RelativePosition;
            SetHeightWidth(768, 1024);

            if (OnSimConnected != null)
                OnSimConnected(sender);
        }
        private void loginCallback(LoginStatus status, string message)
        {
            VUtil.assetServerUri = this.m_user.Network.AssetServerUri;
            VUtil.authToken = this.m_user.Network.SecureSessionID;
                 
            if (OnLogin != null)
                OnLogin((LoginStatus)status, message);
        }

        public void Logout()
        {
            m_user.Network.Logout();
        }

        public void disconnectedCallback(NetworkManager.DisconnectType reason, string message)
        {
            if (OnSimDisconnected != null)
                OnSimDisconnected(reason, message);
        }

        public bool Connected
        {
            get { return m_user.Network.Connected; }
        }

        public void SendIM(string _target_uuid, string _message)
        {
            m_user.Self.InstantMessage(new UUID(_target_uuid), _message);
        }

        public void SendChat(string _message, int _channel, int _range)
        {
            m_user.Self.Chat(_message, _channel, (ChatType)_range);
        }

        public void Teleport(string region, float x, float y, float z)
        {
            if (OnTeleport != null)
                OnTeleport(this, EventArgs.Empty);

            m_user.Self.Teleport(region, new Vector3(x, y, z));
        }

        private LoginParams getLoginParams(string loginURI, string username, string password, string startlocation)
        {
            string firstname;
            string lastname;

            Util.separateUsername(username, out firstname, out lastname);

            LoginParams loginParams = m_user.Network.DefaultLoginParams(
                firstname, lastname, password, "3Di_OpenViewer", "1.0.0.0");


            loginURI = Util.getSaneLoginURI(loginURI);
            
            if (startlocation.Length == 0)
            {

                if (!loginURI.EndsWith("/"))
                    loginURI += "/";

                string[] locationparse = loginURI.Split('/');
                try
                {
                    startlocation = locationparse[locationparse.Length - 2];
                    if (startlocation == locationparse[2])
                    {
                        startlocation = "last";
                    }
                    else
                    {
                        loginURI = "";
                        for (int i = 0; i < locationparse.Length - 2; i++)
                        {
                            loginURI += locationparse[i] + "/";
                        }
                    }

                }
                catch (Exception)
                {
                    startlocation = "last";
                }

            }
            else
            {
            }

            loginParams.URI = loginURI;


            if (startlocation != "last" && startlocation != "home")
            {
                if (startlocation.StartsWith("uri:") == false)
                    startlocation = "home";
            }

            loginParams.Start = startlocation;

            return loginParams;
        }
        
        private void chatCallback(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourcetype,
                                  string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            // This is weird -- we get start/stop typing chats from
            // other avatars, and we get messages back that we sent.
            // (Tested on OpenSim r3187)
            // So we explicitly check for those cases here.
            if ((int)type < 4 && id != m_user.Self.AgentID)
            {
                if (OnChat != null)
                    OnChat(message, audible, type, sourcetype, fromName, id, ownerid, position);
            }
        }



        private void newPrim(Simulator simulator, Primitive prim, ulong regionHandle, ushort timeDilation)
        {
            if (OnNewPrim != null)
                OnNewPrim(simulator, prim, regionHandle, timeDilation);
        }

        
        private void landPatchCallback(Simulator simulator, int x, int y, int width, float[] data)
        {
            if (OnLandPatch != null)
            {
                OnLandPatch(simulator,x, y, width, data);
            }
        }
        private void newAvatarCallback(Simulator simulator, Avatar avatar, ulong regionHandle,
                                       ushort timeDilation)
        {
            if (OnNewAvatar != null)
            {
               //avatar.Velocity
                OnNewAvatar(simulator,avatar,regionHandle,timeDilation);
            }
        }
        private void objectUpdatedCallback(Simulator simulator, ObjectUpdate update, ulong regionHandle,
                                          ushort timeDilation)
        {
            if (OnObjectUpdated != null)
            {
                OnObjectUpdated(simulator, update, regionHandle, timeDilation);
            }
        }

        public void RequestAnimationStart(UUID _uuid)
        {
            m_user.Self.AnimationStart(_uuid, true);
        }

        public void RequestAnimationStop(UUID _uuid)
        {
            m_user.Self.AnimationStop(_uuid, true);
        }

        public void RequestTexture(UUID assetID)
        {
            m_user.Assets.RequestImage(assetID, ImageType.Normal);
        }

        public void CancelTexture(UUID assetID)
        {
            m_user.Assets.RequestImage(assetID, ImageType.Normal, 0.0f, -1, 0);
        }

        public void SetCameraPosition(Vector3[] camdata)
        {
            for (int i = 0; i < camdata.Length; i++)
            {
                if (Single.IsNaN(camdata[i].X) || Single.IsNaN(camdata[i].Y) || Single.IsNaN(camdata[i].Z))
                    return;
            }

            m_user.Self.Movement.Camera.AtAxis = camdata[1];
            m_user.Self.Movement.Camera.LeftAxis = camdata[0];
            m_user.Self.Movement.Camera.LeftAxis = camdata[2];
        }
        
        public void SetHeightWidth(uint height, uint width)
        {
            m_user.Self.SetHeightWidth((ushort)height, (ushort)width);
        }

        public UUID GetSelfUUID
        {
            get { return m_user.Self.AgentID; }
        }

        public Vector3 Position
        {
            get { return m_user.Self.RelativePosition; }
        }

        public bool StraffLeft
        {
            set {m_user.Self.Movement.LeftPos = value;}
            get { return m_user.Self.Movement.LeftPos; }
        }
        public bool StraffRight
        {
            set { m_user.Self.Movement.LeftNeg = value; }
            get { return m_user.Self.Movement.LeftNeg; }
        }

        public void UpdateFromHeading(double heading)
        {
            m_user.Self.Movement.UpdateFromHeading(heading ,false);
        }

        public void TurnToward(Vector3 target)
        {
            m_user.Self.Movement.TurnToward(target);
        }

        public bool Forward
        {
            set {m_user.Self.Movement.AtPos = value;}
            get { return m_user.Self.Movement.AtPos; }
        }

        public bool Backward
        {
            set { m_user.Self.Movement.AtNeg = value; }
            get { return m_user.Self.Movement.AtNeg; }
        }

        public bool Jump
        {
            set { m_user.Self.Jump(value); }
        }

        public bool Flying
        {
            get { return m_user.Self.Movement.Fly; }
            set { m_user.Self.Movement.Fly = value; }
        }

        public bool Run
        {
            get { return m_user.Self.Movement.AlwaysRun; }
            set { m_user.Self.Movement.AlwaysRun = value; }
        }

        public bool Up
        {
            get { return m_user.Self.Movement.UpPos; }
            set { m_user.Self.Movement.UpPos = value; }
        }

        public bool Down
        {
            get { return m_user.Self.Movement.UpNeg; }
            set { m_user.Self.Movement.UpNeg = value; }
        }

        public void Touch(uint objectLocalID)
        {
            // NOTE: this only handles "Touch" actions. Other possible click actions include Buy, Pay, OpenTask, PlayMedia, OpenMedia. 
            // These would need separate methods to invoke the appropriate server action (through libomv).
            m_user.Self.Touch(objectLocalID);
        }

        public void RequestSit(UUID uuid, Vector3 offset)
        {
            VUtil.LogConsole(this.ToString(), " Sit: target uuid:" + uuid.ToString());

            m_user.Self.RequestSit(uuid, offset); 
            // after this, we should wait for the avatarSitResponse from the server (through libomv)
            // if the response contains "autopilot=true", then the client must
            // (a) try to autopilot to the location
            // (b) if successful, send a sit() request
            //
            // If the response contains "autopilot=false", then the server already completed the
            // sit. No further action from client needed.
        }

        public void Sit()
        {
            m_user.Self.Sit();
        }

        public uint SittingOn()
        {
            return m_user.Self.SittingOn;
        }
        public void StandUp()
        {
            m_user.Self.Stand();
        }

        #region SoundEvent
        private void Sound_OnAttachSound(UUID soundID, UUID ownerID, UUID objectID, float gain, byte flags)
        {
            if (OnAttachSound != null)
                OnAttachSound(soundID, ownerID, objectID, gain, flags);
        }

        private void Sound_OnAttachSoundGainChange(UUID objectID, float gain)
        {
            if (OnAttachSoundGainChange != null)
                OnAttachSoundGainChange(objectID, gain);
        }

        private void Sound_OnPreloadSound(UUID soundID, UUID ownerID, UUID objectID)
        {
            if (OnPreloadSound != null)
                OnPreloadSound(soundID, ownerID, objectID);
        }

        private void Sound_OnSoundTrigger(UUID soundID, UUID ownerID, UUID objectID, UUID parentID, float gain, ulong regionHandle, Vector3 position)
        {
            if (OnSoundTrigger != null)
                OnSoundTrigger(soundID, ownerID, objectID, parentID, gain, regionHandle, position);
        }
        #endregion

        #region VoiceEvent
        #endregion
    }
}
