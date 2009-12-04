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
 */

using System;
using System.Collections.Generic;
using System.Threading;
using IrrlichtNETCP;
using OpenMetaverse;

namespace OpenViewer.Managers
{
    public delegate void TextureFromWebCallback(string filenameWithoutExtension);

    public class ProtocolManager : BaseComponent
    {
        private const int TELEPORT_MAX_X = 255;
        private const int TELEPORT_MAX_Y = 255;
        private const int TELEPORT_MAX_Z = 10000;

        // Chat function of libomv can't send 1100byte over packet, (wide char is utf-8, so a charcter is 3 byte. and eof.)
        private const int maxMessageLength = (1100 / 3) - 1;

        private SLProtocol avatarConnection = null;
        public SLProtocol AvatarConnection { get { return (avatarConnection); } }
        private Dictionary<ulong, List<int>> receivedPatchesDetail = new Dictionary<ulong, List<int>>();
        private string requestedTeleportRegionName;
        private int requestedTeleportX, requestedTeleportY, requestedTeleportZ;
        private bool teleportDestinationIsNeighboring = false;
        public bool fullCleanup = false;
        // huanvn - previous connected sim
        private Simulator previousSim;
        // Usui - detached sims: "a detached simulator" is a simulator you have visited before 
        //   and the connection to it is disconnected now
        private Dictionary<ulong, Simulator> DetachedSims = new Dictionary<ulong, Simulator>();

        public event EventHandler OnParcelChanged;
        public event TextureFromWebCallback OnTextureFromWebLoaded;

        public ProtocolManager(Viewer viewer)
            : base(viewer, -1)
        {
            avatarConnection = new SLProtocol();
            avatarConnection.m_user.Settings.MULTIPLE_SIMS = true;
            avatarConnection.m_user.Settings.STORE_LAND_PATCHES = true;
            avatarConnection.m_user.Settings.PARCEL_TRACKING = false;

            // Event handlers
            // - grid
            avatarConnection.OnGridConnected += OnGridConnected;

            // - region
            avatarConnection.OnSimConnected += OnSimConnected;
            avatarConnection.OnLandPatch += OnLandPatch;
            avatarConnection.OnSimDisconnected += OnSimDisconnected;
            avatarConnection.OnTeleportFinished += OnTeleportFinished;
            avatarConnection.OnCurrentSimChanged += OnCurrentSimChanged;

            // - avatars
            avatarConnection.OnLogin += OnLogin;
            avatarConnection.OnNewAvatar += OnNewAvatar;
            avatarConnection.OnAvatarSitResponse += OnAvatarSitResponse;
            avatarConnection.OnAnimationUpdate += avatarConnection_OnAnimationUpdate;

            // - prims and assets
            avatarConnection.OnNewPrim += OnNewPrim_warning; // warning: libomv invokes OnNewPrim event both for new prims and updates to prims
            avatarConnection.OnObjectUpdated += OnObjectUpdated;
            avatarConnection.OnObjectDeleted += OnObjectDeleted;

            // - miscellanious
            avatarConnection.OnChat += OnChat;
            avatarConnection.OnIM += OnIM;
            avatarConnection.OnLoadURL += OnLoadURL;
            avatarConnection.OnRegisterLoginRespons += RegisterLoginResponseHandler;

            // - sound
            avatarConnection.OnAttachSound += OnAttachSound;

            Reference.Viewer.StateManager.OnChanged += new StateManager.ChangedListener(StateManager_OnChanged);
        }

        void StateManager_OnChanged(State _state)
        {
            if (_state == State.CONNECTED)
            {
                Simulator sim = avatarConnection.m_user.Network.CurrentSim;
                if (sim != null)
                {
                    //avatarConnection.m_user.Parcels.RequestAllSimParcels(sim);
                }
            }
        }

        // Run when the scene is (re)initializing
        public override void Initialize()
        {
            receivedPatchesDetail.Clear();
        }

        // Run every frame
        public override void Update(uint frame)
        {
            if (Reference.Viewer.StateManager.State == State.CONNECTED)
            {
                try
                {
                    Vector3 userPosition = new Vector3();
                    lock (Reference.Viewer.AvatarManager.UserObject)
                    {
                        if (Reference.Viewer.AvatarManager.UserObject.Prim != null)
                        {
                            userPosition = Reference.Viewer.AvatarManager.UserObject.Prim.Position;
                        }
                    }
                    if (userPosition != Vector3.Zero)
                    {
                        /*
                        int nowParcel = 0;
                        nowParcel = avatarConnection.m_user.Parcels.GetParcelLocalID(avatarConnection.m_user.Network.CurrentSim, userPosition);

                        if (currentParcel != nowParcel)
                        {
                            currentParcel = nowParcel;

                            if (OnParcelChanged != null)
                                OnParcelChanged(this, EventArgs.Empty);
                        }
                        */
                    }
                }
                catch (Exception e)
                {
                    Reference.Log.Warn(@"[PROTOCOLMANAGER]: Exception occured in Update - " + e.Message);
                    Reference.Log.Debug(@"[PROTOCOLMANAGER]: Exception occured in Update - " + e.StackTrace);
                }
            }
        }

        // Run when the scene is closing
        public override void Cleanup()
        {
            receivedPatchesDetail.Clear();
            if (fullCleanup)
            {
                if (avatarConnection != null && avatarConnection.Connected)
                {
                    avatarConnection.Logout();
                }
            }
            else
            {
                fullCleanup = true;
            }
            if (Reference.Viewer.StateManager.State == State.CLOSING)
            {
                DetachedSims.Clear();
            }
        }

        #region Public interface
        public void Login(string server, string first, string last, string password, string location)
        {
            avatarConnection.BeginLogin(server, first.ToLower() + " " + last.ToLower(), password, location);
        }


        private string teleportReqRegionName;
        private float teleportReqX;
        private float teleportReqY;
        private float teleportReqZ;

        public void VoiceEffect(float _volumeLevel)
        {
            avatarConnection.VoiceEffect(_volumeLevel);
        }

        public void Teleport(string _regionName, float _x, float _y, float _z)
        {
            teleportReqRegionName = _regionName;
            teleportReqX = _x;
            teleportReqY = _y;
            teleportReqZ = _z;

            Reference.Viewer.GuiManager.IsShowCursor = false;
            Reference.Viewer.EffectManager.FadeOut(TeleportEventHandler);
        }

        private void TeleportEventHandler(object _sender, EventArgs _arg)
        {
            TeleportStart(teleportReqRegionName, teleportReqX, teleportReqY, teleportReqZ);
        }

        public void TeleportStart(string regionName, float x, float y, float z)
        {
            GridRegion destRegion;
            avatarConnection.m_user.Grid.GetGridRegion(regionName, GridLayerType.Objects, out destRegion);

            if (destRegion.RegionHandle == 0)
            {
                // No such region, don't teleport
                Reference.Viewer.GuiManager.ShowMessageWindow(DialogText.WarningTeleportMessage);
                return;
            }

            // clamp position.
            x = Utils.Clamp(x, 0, TELEPORT_MAX_X);
            y = Utils.Clamp(y, 0, TELEPORT_MAX_Y);
            z = Utils.Clamp(z, 0, TELEPORT_MAX_Z);

            if (destRegion.RegionHandle == avatarConnection.m_user.Network.CurrentSim.Handle)
            {
                // Intra-region teleport - do NOT ask OpenViewer to clean up the scene
                lock (Reference.Viewer.StateManager.SyncStat)
                {
                    if (Reference.Viewer.StateManager.State == State.CONNECTED)
                    {
                        TeleportRequest(regionName, x, y, z);
                        teleportDestinationIsNeighboring = false;
                    }
                }
            }
            else
            {
                // Inter-region teleport: must ask OpenViewer to clean up the scene, then teleport

                // There are 2 cases for Inter-region teleport: 
                // (1) non-neighboring region: prims, avatars, terrain will be newly downloaded, and libomv
                // will automatically invoke OnNewPrim,OnNewAvatar,OnLandPatch callbacks, which will
                // construct the scene normally. We must not invoke "fake" callbacks in this case.
                // (2) neighboring region: prims, avatars, terrain already downloaded into libomv data structures.
                // libomv will not invoke callbacks again, so we must "fake" the event and invoke the callbacks
                // manually (in OnTeleportFinished) with the already-downloaded data, to construct the scene
                // properly.

                // Here we differentiate between the 2 cases and set a flag for appropriate later
                // processing in OnTeleportFinished.

                // WARNING: avatarConnection.m_user.Network.Simulators caches *ALL* simulators 
                // that libomv has ever connected to, even if they are no longer neighboring to
                // the current region, so we can't use it to judge if the destRegion is neighboring
                // or not. We must look at the grid coordinates.
                // We define a neighboring sim as one of the 8 surrounding sims from the current sim.
                // This definition works for OpenSim, not necessarily for SL.

                teleportDestinationIsNeighboring = false;
                GridRegion curRegion;
                avatarConnection.m_user.Grid.GetGridRegion(avatarConnection.m_user.Network.CurrentSim.Name, GridLayerType.Objects, out curRegion);
                int deltaX = destRegion.X - curRegion.X;
                int deltaY = destRegion.Y - curRegion.Y;
                if (Math.Abs(deltaX) <= 1 && Math.Abs(deltaY) <= 1) // NOTE: we already checked above that destRegion is not curRegion
                {
                    teleportDestinationIsNeighboring = true;
                    // Checking if the SIM is a neighbor is not enough, because what we really need to check is if
                    // LibOMV is already connected to. In case for example the destination is supposed to be a neighbor,
                    // but it was disconnected, we need to reconnect.
                    //teleportDestinationIsNeighboring = false;
                    //foreach (Simulator s in avatarConnection.m_user.Network.Simulators)
                    //{
                    //    if (s != null)
                    //    {
                    //        if (s.Handle == curRegion.RegionHandle)
                    //        {
                    //            if (s.Connected && s.Caps != null)
                    //            {
                    //                teleportDestinationIsNeighboring = true;
                    //            }
                    //        }
                    //    }
                    //}
                }

                previousSim = avatarConnection.m_user.Network.CurrentSim; // huanvn - keep track of previous sim after teleporting
                lock (Reference.Viewer.StateManager.SyncStat)
                {
                    if (Reference.Viewer.StateManager.State == State.CONNECTED)
                    {
                        Reference.Viewer.StateManager.State = State.TELEPORT_REQUESTED; // see OpenViewer.cs
                        this.requestedTeleportRegionName = regionName;
                        this.requestedTeleportX = (int)x;
                        this.requestedTeleportY = (int)y;
                        this.requestedTeleportZ = (int)z;
                        new Thread(new ThreadStart(this.Thread_to_WaitForTeleportState)).Start();
                    }
                }
            }
        }

        public string GetCurrentSimName()
        {
            string simName = string.Empty;

            if (avatarConnection.Connected)
            {
                if (avatarConnection.m_user.Network.CurrentSim.Connected)
                {
                    if (avatarConnection.m_user.Network.CurrentSim.Name.Length > 15)
                    {
                        simName = avatarConnection.m_user.Network.CurrentSim.Name.Substring(0, 12) + "...";
                    }
                    else
                    {
                        simName = avatarConnection.m_user.Network.CurrentSim.Name;
                    }
                }
            }

            return simName;
        }

        /// <summary>
        /// This function request image to asset server or url.
        /// </summary>
        /// <param name="_filename">asset uuid or url</param>
        /// <param name="_useCache"></param>
        /// 
        public void RequestImage(string _filename, bool _useCache)
        {
            RequestImage(_filename, _useCache, null);
        }

        public void RequestImage(string _uuidOrUrl, bool _useCache, VObject _vObject)
        {
            if (_useCache)
            {
                string path = Util.TextureFolder + _uuidOrUrl;
                if (System.IO.File.Exists(path))
                    return;
            }

            if (_uuidOrUrl.StartsWith("http://") || _uuidOrUrl.StartsWith("https://"))
                RequestImageToWebServer(_uuidOrUrl);
            else
            {
                string uuid = System.IO.Path.GetFileNameWithoutExtension(_uuidOrUrl);
                RequestImage(new UUID(uuid), _vObject);
            }
        }

        private void RequestImage(UUID _assetUUID, VObject _vObject)
        {
            if (_vObject == null)
                avatarConnection.RequestImage(_assetUUID, AssetType.Object, true);
            else
                Reference.Viewer.TextureManager.RequestImage(new UUID(_assetUUID), _vObject);
        }

        public void RequestImageToWebServer(string _url)
        {
            using (System.Net.WebClient wc = new System.Net.WebClient())
            {
                string filename = System.IO.Path.GetFileNameWithoutExtension(_url);
                string extension = System.IO.Path.GetExtension(_url);
                string path = Util.TextureFolder + filename + extension;
                wc.DownloadFile(_url, path);

                if (OnTextureFromWebLoaded != null)
                    OnTextureFromWebLoaded(filename);
            }
        }

        public void RerequestTerrain()
        {
            // Check if we are connected and if land patches are being received already
            if (avatarConnection.m_user.Network.CurrentSim != null)
            {
                //if (receivedPatches.ContainsKey(avatarConnection.m_user.Network.CurrentSim.Handle) && receivedPatches[avatarConnection.m_user.Network.CurrentSim.Handle] != 0)
                if (receivedPatchesDetail.ContainsKey(avatarConnection.m_user.Network.CurrentSim.Handle) && receivedPatchesDetail[avatarConnection.m_user.Network.CurrentSim.Handle].Count != 0)
                {
                    Reference.Log.Debug("[NOTIFICATION]: Logging in takes longer than 5 seconds.");
                }
                else
                {
                    Reference.Log.Debug("[NOTIFICATION]: Login progress was halted. Rerequesting terrain data.");

                    OpenMetaverse.Simulator simulator = avatarConnection.m_user.Network.CurrentSim;
                    OpenMetaverse.Packets.RegionHandshakeReplyPacket reply = new OpenMetaverse.Packets.RegionHandshakeReplyPacket();
                    reply.AgentData.AgentID = avatarConnection.m_user.Self.Client.Self.AgentID;
                    reply.AgentData.SessionID = avatarConnection.m_user.Self.Client.Self.SessionID;
                    reply.RegionInfo.Flags = 0;
                    avatarConnection.m_user.Self.Client.Network.SendPacket(reply, simulator);
                }
            }
        }

        #endregion

        #region Event Handlers

        #region Grid Events
        void OnGridConnected()
        {
        }


        #endregion

        #region Region Events
        void OnSimConnected(OpenMetaverse.Simulator sim)
        {
            // warning: this callback is called for ALL sims you connect to including neighboring regions
            OpenMetaverse.Packets.GenericMessagePacket packet = new OpenMetaverse.Packets.GenericMessagePacket();
            packet.AgentData.AgentID = avatarConnection.m_user.Self.AgentID;
            packet.AgentData.SessionID = avatarConnection.m_user.Self.SessionID;
            packet.AgentData.TransactionID = UUID.Zero;
            packet.MethodData.Invoice = UUID.Zero;
            packet.MethodData.Method = Utils.StringToBytes("ClientPreference");
            packet.ParamList = new OpenMetaverse.Packets.GenericMessagePacket.ParamListBlock[2];
            packet.ParamList[0] = new OpenMetaverse.Packets.GenericMessagePacket.ParamListBlock();
            packet.ParamList[0].Parameter = Utils.StringToBytes("ParcelPropertiesPacketOnSignificantMovement:false");
            packet.ParamList[1] = new OpenMetaverse.Packets.GenericMessagePacket.ParamListBlock();
            packet.ParamList[1].Parameter = Utils.StringToBytes("WindLayerPacketOnFrame:false");
            avatarConnection.m_user.Network.SendPacket(packet, sim);
        }

        void OnSimDisconnected(OpenMetaverse.NetworkManager.DisconnectType reason, string message)
        {
            System.Diagnostics.Debug.WriteLine("SIM Disconnected " + reason + "\n" + message);
        }

        void OnLandPatch(OpenMetaverse.Simulator sim, int x, int y, int width, float[] data)
        {
            if (Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim != null
                && sim.Handle != Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim.Handle)
                return;

            lock (Reference.Viewer.StateManager.SyncStat)
            {
                if (Reference.Viewer.IsDrawTerrain == false)
                {
                    if (Reference.Viewer.StateManager.State == State.LOGIN)
                    {
                        Reference.Viewer.GuiManager.ProgressLogin((int)(256f * 100f));
                        Reference.Viewer.StateManager.State = State.CONNECTED;
                        Reference.Viewer.AvatarManager.UpdateAllObjects();
                    }
                    return;
                }

                if (Reference.Viewer.StateManager.State != State.CONNECTED
                    && Reference.Viewer.StateManager.State != State.DOWNLOADING)
                {
                    if (Reference.Viewer.StateManager.State == State.TELEPORTING
                        || Reference.Viewer.StateManager.State == State.LOGIN)
                    {
                        // expected state
                    }
                    else
                    {
                        //Console.WriteLine("Unexpected state in OnLandPatch callback: " + Reference.Viewer.StateManager.State);
                    }
                    Reference.Viewer.StateManager.State = State.DOWNLOADING;
                }
            }
            ulong handle = sim.Handle;

            for (int tx = 0; tx < width; tx++)
            {
                for (int ty = 0; ty < width; ty++)
                {
                    float d = data[ty * width + tx];
                    if (d > 1000f || d < 0f) d = 0f;
                    Reference.Viewer.TerrainManager.SetPatch(handle, x * 16 + tx, y * 16 + ty, d);
                }
            }

            if (Reference.Viewer.StateManager.State == State.DOWNLOADING)
            {
                //lock (receivedPatches)
                lock(receivedPatchesDetail)
                {
                    if (!receivedPatchesDetail.ContainsKey(handle))
                    {
                        receivedPatchesDetail.Add(handle, new List<int>());
                    }

                    if (!receivedPatchesDetail[handle].Contains(x * 512 + y))
                        receivedPatchesDetail[handle].Add(x * 512 + y);
                }
                Reference.Viewer.GuiManager.ProgressLogin((int)Math.Floor((float)receivedPatchesDetail[handle].Count / 256f * 100f));
            }
            else if (Reference.Viewer.StateManager.State == State.CONNECTED)
            {
                lock (receivedPatchesDetail)
                {
                    if (!receivedPatchesDetail.ContainsKey(handle))
                    {
                        receivedPatchesDetail.Add(handle, new List<int>());
                    }
                    if (!receivedPatchesDetail[handle].Contains(x * 512 + y))
                        receivedPatchesDetail[handle].Add(x*512+y);
                    else
                        Reference.Log.Info("[LANDMAPS]: Duplicate landpatch received.");                
                }
            }

            lock (Reference.Viewer.StateManager.SyncStat)
            {
                if (Reference.Viewer.StateManager.State == State.DOWNLOADING
                    || Reference.Viewer.StateManager.State == State.TELEPORTING)
                {
                    if (receivedPatchesDetail[handle].Count >= 250)
                    {
                        Reference.Viewer.TerrainManager.GenerateTerrain(handle);
                        Reference.Viewer.StateManager.State = State.CONNECTED;
                        Reference.Viewer.AvatarManager.UpdateAllObjects();
                    }
                } // LandPatches will be generated on teleport *and* they will come in through the region server
                else if (Reference.Viewer.StateManager.State == State.CONNECTED)
                {
                    Reference.Viewer.TerrainManager.GenerateTerrain(handle);
                }
            } 
            // huanvn - close connection to previous foreign sim after teleporting
            if (Reference.Viewer.StateManager.State == State.CONNECTED && !teleportDestinationIsNeighboring)
            {
                lock (DetachedSims)
                {
                    if (previousSim != null)
                    {
                        if (DetachedSims.ContainsKey(previousSim.Handle))
                        {
                            DetachedSims.Remove(previousSim.Handle);
                        }
                        previousSim.Disconnect(false);
                        avatarConnection.m_user.Network.Simulators.Remove(previousSim);
                        DetachedSims.Add(previousSim.Handle, previousSim);
                        previousSim.ObjectsAvatars.Dictionary.Clear();
                        previousSim.ObjectsPrimitives.Dictionary.Clear();
                        previousSim = null;
                    }
                }
            }
        }

        void OnCurrentSimChanged(OpenMetaverse.Simulator PreviousSimulator)
        {
        }

        void OnTeleportFinished(string message, OpenMetaverse.AgentManager.TeleportStatus status, OpenMetaverse.AgentManager.TeleportFlags flags)
        {
            if (status == AgentManager.TeleportStatus.Finished)
            {
                // Happily ever after
                OpenMetaverse.Packets.UseCircuitCodePacket uccp = new OpenMetaverse.Packets.UseCircuitCodePacket();
                uccp.CircuitCode.Code = avatarConnection.m_user.Network.CircuitCode;
                uccp.CircuitCode.ID = avatarConnection.m_user.Self.AgentID;
                uccp.CircuitCode.SessionID = avatarConnection.m_user.Self.SessionID;

                avatarConnection.m_user.Network.SendPacket(uccp, avatarConnection.m_user.Network.CurrentSim);

                if (teleportDestinationIsNeighboring)
                {
                    // WARNING: unfortunately, sometimes libomv does generate a OnNewPrim callback
                    // after teleport to a neighboring region (though usually it does not). The effect
                    // of this is a redundant deletion/insertion of a prim; libomv adds the prim itself
                    // through the OnNewPrim callback, then we add it again here with a fake callback,
                    // which causes the prim to be removed from the scene and re-added.
                    GenerateFakeCallbacksForNeighboringTeleportFinished();
                }
                else
                {
                    Simulator currentSim = avatarConnection.m_user.Network.CurrentSim;
                    if (DetachedSims.ContainsKey(currentSim.Handle))
                    {
                        // Fabricate RegionHandshakeReply to make OpenSim to emit LayerData packets again
                        OpenMetaverse.Packets.RegionHandshakeReplyPacket reply = new OpenMetaverse.Packets.RegionHandshakeReplyPacket();
                        reply.AgentData.AgentID = avatarConnection.m_user.Self.AgentID;
                        reply.AgentData.SessionID = avatarConnection.m_user.Self.SessionID;
                        reply.RegionInfo.Flags = 0;
                        avatarConnection.m_user.Network.SendPacket(reply, avatarConnection.m_user.Network.CurrentSim);
                        avatarConnection.m_user.Self.CompleteAgentMovement(currentSim);
                        avatarConnection.m_user.Self.Movement.SendUpdate();
                        avatarConnection.m_user.Self.Movement.SendUpdate();
                        avatarConnection.m_user.Self.Movement.SendUpdate();

                        // Recover missing avatars and prims which have arrived after teleport request and before the teleport finish
                        Simulator detached = DetachedSims[currentSim.Handle];
                        foreach (Avatar av in detached.ObjectsAvatars.FindAll(delegate(Avatar a) { return true; }))
                        {
                            OnNewAvatar(currentSim, av, currentSim.Handle, (ushort)(currentSim.Stats.Dilation * 65535.0f));
                        }
                        foreach (Primitive prim in detached.ObjectsPrimitives.FindAll(delegate(Primitive p) { return true; }))
                        {
                            OnNewPrim_warning(currentSim, prim, prim.RegionHandle, (ushort)(currentSim.Stats.Dilation * 65535.0f));
                        }

                        // Recover the region name, but why currentSim.Name is empty for the first place?
                        currentSim.Name = DetachedSims[currentSim.Handle].Name;
                    }
                }

                Reference.Viewer.Adapter.CallTeleported(
                    avatarConnection.m_user.Self.AgentID.ToString(),
                    avatarConnection.username,
                    (int)avatarConnection.m_user.Self.SimPosition.X,
                    (int)avatarConnection.m_user.Self.SimPosition.Y,
                    (int)avatarConnection.m_user.Self.SimPosition.Z
                    );
                //Reference.Viewer.StateManager.State = State.CONNECTED; // don't do this here -> wait until terrain has finished downloading (see OnLandPatch)
            }
            else
            {
                // Teleport failed, so we need to add back the scene that we just deleted
                teleportDestinationIsNeighboring = true; // turn on this flag to return back current sim
                GenerateFakeCallbacksForNeighboringTeleportFinished();
                Reference.Viewer.GuiManager.ShowTeleportFailedWindow(message);
            }

            Reference.Viewer.EffectManager.FadeIn(TeleportedEventHandler);
        }

        private void TeleportedEventHandler(object _sender, EventArgs _arg)
        {
            Reference.Viewer.GuiManager.IsShowCursor = true;
        }


        #endregion

        #region Avatar Events
        void OnLogin(OpenMetaverse.LoginStatus status, string message)
        {
            if (status == OpenMetaverse.LoginStatus.Failed)
            {
                int messageID = 0;
                string messageText = message;

                if (messageText.Contains("You appear to be already logged in."))
                    messageID = 1;

                switch (messageID)
                {
                    case 0:
                        messageText = DialogText.ErrorLoginMessageme00;
                        break;

                    case 1:
                        messageText = DialogText.ErrorLoginMessageme01;
                        break;
                }

                Reference.Viewer.GuiManager.LoginFailed = true;
                Reference.Viewer.GuiManager.LoginMessage = messageText;
                Reference.Log.Error(message);
            }
            else
            {
                // TODO: start waiting timeout
            }
        }

        void OnNewAvatar(OpenMetaverse.Simulator sim, OpenMetaverse.Avatar avatar, ulong regionHandle, ushort timeDilation)
        {
            if (Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim != null
                && sim.Handle != Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim.Handle)
            {
                if (DetachedSims.ContainsKey(regionHandle))
                {
                    DetachedSims[regionHandle].ObjectsAvatars.Dictionary.Add(avatar.LocalID, avatar);
                }
                return;
            }
            if (regionHandle != 0)
            {
                Reference.Viewer.AvatarManager.AddObject(avatar, regionHandle);
            }
        }
         void OnAvatarSitResponse(OpenMetaverse.UUID objectID, bool autoPilot, OpenMetaverse.Vector3 cameraAtOffset, OpenMetaverse.Vector3 cameraEyeOffset, bool forceMouselook, OpenMetaverse.Vector3 sitPosition, OpenMetaverse.Quaternion sitRotation)
        {
            if (autoPilot)
            {
                // Client-side autopilot is requested from the server when the avatar tries
                // to sit on a prim that is (a) occupied and (b) far away from the avatar.
                // In SL the client-side autopilot moves the avatar towards the target, and may succeed or fail.
                // If success, then the avatar sits at the final location near the sit ball.
                // Here, we have no autopilot implemented, so we simply do nothing, effectively
                // ignoring the sit request for a far-away prim that is occupied.

                bool clientSideAutoPilotSucceeded = false;

                // TODO: later, we should implement autopilot code and check if the client-side autopilot
                // succeeded.

                if (clientSideAutoPilotSucceeded)
                {
                    // After successfully moving to the target, tell server that we successfully autopiloted and can now sit.
                    avatarConnection.Sit();
                }
            }
            else
            {
                // no action needed: server already successfully processed the sit.
            }
        }
        #endregion

        #region Prim and Asset Events
        void OnNewPrim_warning(OpenMetaverse.Simulator sim, OpenMetaverse.Primitive prim, ulong regionHandle, ushort timeDilation)
        // warning: libomv invokes OnNewPrim event both for new prims and updates to prims
        {
            if (Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim != null
                && sim.Handle != Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim.Handle)
            {
                if (DetachedSims.ContainsKey(regionHandle))
                {
                    DetachedSims[regionHandle].ObjectsPrimitives.Dictionary.Add(prim.LocalID, prim);
                }
                return;
            }

            Reference.Viewer.EntityManager.HandleNewPrimEvent_warning(prim, regionHandle, timeDilation); // warning: libomv invokes OnNewPrim event both for new prims and updates to prims
        }

        void avatarConnection_OnAnimationUpdate(UUID objectID)
        {
            Reference.Viewer.AvatarManager.UpdateObject(objectID);
        }

        void OnObjectUpdated(OpenMetaverse.Simulator sim, OpenMetaverse.ObjectUpdate update, ulong regionHandle, ushort timeDilation)
        {
            if (Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim != null
                && sim.Handle != Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim.Handle)
                return;

            VObject obj = null;

            if (update.Avatar)
            {
                Reference.Viewer.AvatarManager.UpdateObject(update, regionHandle);
            }
            else
            {
                lock (Reference.Viewer.EntityManager.Entities)
                {
                    if (Reference.Viewer.EntityManager.Entities.ContainsKey(regionHandle.ToString() + update.LocalID.ToString()))
                    {
                        obj = Reference.Viewer.EntityManager.Entities[regionHandle.ToString() + update.LocalID.ToString()];


                        // Update the primitive properties for this object.
                        obj.Prim.Acceleration = update.Acceleration;
                        obj.Prim.AngularVelocity = update.AngularVelocity;
                        obj.Prim.CollisionPlane = update.CollisionPlane;
                        obj.Prim.Position = update.Position;
                        obj.Prim.Rotation = update.Rotation;
                        obj.Prim.PrimData.State = update.State;
                        if (update.Textures != null)
                            obj.Prim.Textures = update.Textures;
                        obj.Prim.Velocity = update.Velocity;

                        obj.UpdateFullYN = false;
                        // Save back to the Entities.  vObject used to be a value type, so this was neccessary.
                        // it may not be anymore.

                        Reference.Viewer.EntityManager.Entities[regionHandle.ToString() + update.LocalID.ToString()] = obj;
                    }
                }
            }


            // Enqueue this object into the modification queue.
            if (obj != null)
            {
                Reference.Viewer.EntityManager.UpdateObject(obj);
            }
        }

        void OnObjectDeleted(OpenMetaverse.Simulator sim, uint objectID)
        {
            if (Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim != null
                && sim.Handle != Reference.Viewer.ProtocolManager.avatarConnection.m_user.Network.CurrentSim.Handle)
                return;

            ulong regionHandle = sim.Handle;

            if (Reference.Viewer.AvatarManager.ContainObject(regionHandle, objectID))
            {
                Reference.Viewer.AvatarManager.DeleteObject(regionHandle, objectID);
            }
            else
            {
                Reference.Viewer.EntityManager.DeleteObject(regionHandle, objectID);
            }
        }

        #endregion

        #region Miscellanious
        private void RegisterLoginResponseHandler(bool loginSuccess, bool redirect, string message, string reason, LoginResponseData replyData)
        {
            Vector3D forward = new Vector3D(1, 0, 0);
            Vector3D target = new Vector3D(replyData.LookAt.X, replyData.LookAt.Z, replyData.LookAt.Y);

            float radHeading = forward.DotProduct(target);
            Reference.Viewer.AvatarManager.RadHeading = radHeading * (float)Math.PI;
        }

        public void TouchTo(uint _localID)
        {
            avatarConnection.Touch(_localID);
        }

        public void OnChat(string message, ChatAudibleLevel audible, ChatType type, ChatSourceType sourcetype, string fromName, UUID id, UUID ownerid, Vector3 position)
        {
            Reference.Viewer.ChatManager.Add(message, audible, type, sourcetype, fromName, id, ownerid, position);
        }

        public void OnIM(UUID senderID, string fromName, string message)
        {
            Reference.Viewer.Adapter.CallReceiveInstantMessaged(senderID.ToString(), fromName, message);
        }

        public void SendIM(string _target_uuid, string _message)
        {
            avatarConnection.SendIM(_target_uuid, _message);
        }

        public void SendChat(string _message, int _range)
        {
            if (_message.Length > maxMessageLength)
                _message = _message.Substring(0, maxMessageLength);

            int channel = 0;

            if (_message.Length > 0 && _message[0] == '/')
            {
                string[] msgs = _message.Split(new char[] { ' ' });
                if (msgs.Length > 1)
                {
                    string s_channel = msgs[0].Remove(0, 1);
                    int.TryParse(s_channel, out channel);
                }
            }

            avatarConnection.SendChat(_message, channel, _range);

            if (channel == 0)
                Reference.Viewer.ChatManager.Add(_message, 0, (ChatType)_range, ChatSourceType.Agent, avatarConnection.username, UUID.Random(), avatarConnection.GetSelfUUID, avatarConnection.Position);
        }

        public void SitOn(string _targetUUID)
        {
            SitOn(new UUID(_targetUUID));
        }

        public void SitOn(UUID _targetUUID)
        {
            if (avatarConnection.SittingOn() == 0)
                avatarConnection.RequestSit(_targetUUID, new Vector3(0, 0, 0)); // FIXME: get sit target offset from primitive
        }

        public void StandUp()
        {
            if (avatarConnection.SittingOn() != 0)
            {
                avatarConnection.StandUp();
            }
        }

        void OnAttachSound(UUID soundID, UUID ownerID, UUID objectID, float gain, byte flags)
        {
            // If sound uuid exist.
            if (soundID != UUID.Zero && Reference.Viewer.SoundManager != null)
            {
                Reference.Viewer.SoundManager.PlaySE(soundID);
            }
        }

        void OnLoadURL(string objectName, UUID objectID, UUID ownerID, bool ownerIsGroup, string message, string URL)
        {
            Reference.Viewer.Adapter.CallOpenWindow("_blank", URL);
        }
        #endregion
        #endregion

        void Thread_to_WaitForTeleportState()
        {
            int wait_timeout = 0;
            while (Reference.Viewer.StateManager.State == State.TELEPORT_REQUESTED && wait_timeout < 300)
            {
                wait_timeout++;
                Thread.Sleep(200);
            }
            if (wait_timeout > 300)
            {
                // Timeout, reconnect to the original region
                GenerateFakeCallbacksForNeighboringTeleportFinished(); // We never teleported, only the scene was cleaned up, so currentSim did not change yet
                lock (Reference.Viewer.StateManager.SyncStat)
                {
                    Reference.Viewer.StateManager.State = State.DOWNLOADING;
                }
            }
            else
            {
                if (Reference.Viewer.StateManager.State == State.TELEPORTING)
                {
                    // OpenViewer.cs cleaned up the scene and changed the state to TELEPORTING.
                    // Now we can do the actual teleport.
                    TeleportRequest(this.requestedTeleportRegionName, this.requestedTeleportX, this.requestedTeleportY, this.requestedTeleportZ);
                }
                else
                {
                    // Teleport failed. Unfortunately, we already deleted the scene in preparation for the teleport.
                    // Do something graceful here.
                    Reference.Log.Error("[TELEPORT] Teleport failed");
                }
            }
        }

        void TeleportRequest(string _region, float _x, float _y, float _z)
        {
            if (string.IsNullOrEmpty(_region))
                return;

            bool detached = false;
            foreach (Simulator sim in DetachedSims.Values)
            {
                if (_region == sim.Name)
                {
                    detached = true;
                    break;
                }
            }
            if (detached)
            {
                // Since the destination is detached, RegionHandshake won't be sent out.
                int timeout = avatarConnection.m_user.Self.Client.Settings.SIMULATOR_TIMEOUT;
                avatarConnection.m_user.Self.Client.Settings.SIMULATOR_TIMEOUT = 1;
                avatarConnection.Teleport(_region, _x, _y, _z);
                avatarConnection.m_user.Self.Client.Settings.SIMULATOR_TIMEOUT = timeout;
            }
            else
            {
                avatarConnection.Teleport(_region, _x, _y, _z);
            }
        }

        void GenerateFakeCallbacksForNeighboringTeleportFinished()
        {
            System.Diagnostics.Debug.WriteLine("******************GenerateFakeCallbacks******************");
            foreach (Avatar av in avatarConnection.m_user.Network.CurrentSim.ObjectsAvatars.FindAll(delegate(Avatar a) { return true; }))
            {
                this.OnNewAvatar(
                    avatarConnection.m_user.Network.CurrentSim,
                    av,
                     avatarConnection.m_user.Network.CurrentSim.Handle,
                    (ushort)(avatarConnection.m_user.Network.CurrentSim.Stats.Dilation * 65535.0f)
                    );
            }
            foreach (Primitive prim in avatarConnection.m_user.Network.CurrentSim.ObjectsPrimitives.FindAll(delegate(Primitive p) { return true; }))
            {
                this.OnNewPrim_warning(
                    avatarConnection.m_user.Network.CurrentSim,
                    prim,
                    prim.RegionHandle,
                    (ushort)(avatarConnection.m_user.Network.CurrentSim.Stats.Dilation * 65535.0f)
                    );
            }

            TerrainPatch[] patchArray;
            if (avatarConnection.m_user.Terrain.SimPatches.TryGetValue(avatarConnection.m_user.Network.CurrentSim.Handle, out patchArray))
            {
                // reset the terrain pixel counter for this region
                lock (receivedPatchesDetail)
                {
                    if (!receivedPatchesDetail.ContainsKey(avatarConnection.m_user.Network.CurrentSim.Handle))
                    {
                        receivedPatchesDetail.Add(avatarConnection.m_user.Network.CurrentSim.Handle, new List<int>());
                    }
                    else
                    {
                        receivedPatchesDetail[avatarConnection.m_user.Network.CurrentSim.Handle].Clear();
                    }
                }
                // load all the terrain patches for this region
                foreach (TerrainPatch patch in patchArray)
                {
                    if (patch != null)
                    {
                        this.OnLandPatch( // <- will generate terrain when enough patches have arrived
                            avatarConnection.m_user.Network.CurrentSim,
                            patch.X,
                            patch.Y,
                            16, // see OpenMetaverse.TerrainPatch, Data is always 16x16
                            patch.Data);
                    }
                }
            }
        }
    }
}