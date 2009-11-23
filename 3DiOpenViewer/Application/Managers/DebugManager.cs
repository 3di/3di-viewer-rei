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
using IrrlichtNETCP;
using log4net;
using OpenMetaverse;

namespace OpenViewer.Managers
{
    public class DebugManager : BaseComponent
    {
        private EventHandler Action;

        public DebugManager(Viewer _viewer, int _id)
            : base(_viewer, _id)
        {
            ChangeMode(0);
        }

        public override void Cleanup()
        {
        }

        public override void Update(uint frame)
        {
            if (!Reference.Viewer.GuiManager.IsVisibleDebugWindow())
                return;
            
            if (frame % 1000 == 0)
            {
                CalcNodeRoot();
                CalcNodeAvatarManager();
                CalcNodePrimManager();
                CalcNodeTerrainManager();
                CalcNodeShaderManager();
            }

            if (frame % 10 == 0)
            {
                Reference.Viewer.GuiManager.DebugClear();

                if (Action != null)
                    Action(this, EventArgs.Empty);
            }
        }


        public void ChangeMode(int _index)
        {
            switch (_index)
            {
                case 0:
                    Action = SystemInformation;
                    break;

                case 1:
                    Action = NodeInformation;
                    break;

                case 2:
                    Action = UserInformation;
                    break;

                case 3:
                    Action = ViewerInformation;
                    break;

                case 4:
                    Action = ClientNetworkInformation;
                    break;

                case 5:
                    Action = ClientSelfInformation;
                    break;

                case 6:
                    Action = ElementsInformation;
                    break;
#if DEBUG_QUEUE
                case 8:
                    Action = QueueInformation;
                    break;
#endif

                default:
                    ChangeMode(0);
                    break;
            }
        }

        #region SystemInformation
        private void SystemInformation(object sender, EventArgs e)
        {
            int key = 0;

            long memkb = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64 / 1024;
            string processName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;

            Reference.Viewer.GuiManager.DebugAdd(key, "Application");
            Reference.Viewer.GuiManager.DebugAdd(key, " - ProcessName:" + processName);
            Reference.Viewer.GuiManager.DebugAdd(key, " - Version:" + Reference.Viewer.Version.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - Mem(KB):" + memkb.ToString("#,###"));

            for (int i = 0; i < System.GC.MaxGeneration; i++)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, " - GC:" + i.ToString() + System.GC.GetGeneration(i).ToString());
            }
            Reference.Viewer.GuiManager.DebugAdd(key, "");
            Reference.Viewer.GuiManager.DebugAdd(key, "Irrlicht");
            Reference.Viewer.GuiManager.DebugAdd(key, " - FPS:" + Reference.VideoDriver.FPS);
            Reference.Viewer.GuiManager.DebugAdd(key, " - TexCount:" + Reference.VideoDriver.TextureCount);
            Reference.Viewer.GuiManager.DebugAdd(key, " - PrimCount:" + Reference.VideoDriver.PrimitiveCountDrawn);
        }
        #endregion

        #region NodeInformation
        private void NodeInformation(object sender, EventArgs e)
        {
            int key = 1;

            Reference.Viewer.GuiManager.DebugAdd(key, "RootNode");
            Reference.Viewer.GuiManager.DebugAdd(key, " - AnimeNode:" + rootNodeCounter.Anim.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - BasicNode:" + rootNodeCounter.Basic.ToString());

            Reference.Viewer.GuiManager.DebugAdd(key, "ManagerNode:");
            Reference.Viewer.GuiManager.DebugAdd(key, " - EmptyNode:" + BaseManager.ManagerNum);

            Reference.Viewer.GuiManager.DebugAdd(key, "AvatarNode");
            Reference.Viewer.GuiManager.DebugAdd(key, " - AnimeNode:" + avatarNodeCounter.Anim.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - BasicNode:" + avatarNodeCounter.Basic.ToString());

            Reference.Viewer.GuiManager.DebugAdd(key, "PrimNode");
            Reference.Viewer.GuiManager.DebugAdd(key, " - AnimeNode:" + primNodeCounter.Anim.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - BasicNode:" + primNodeCounter.Basic.ToString());

            Reference.Viewer.GuiManager.DebugAdd(key, "TerrainNode");
            Reference.Viewer.GuiManager.DebugAdd(key, " - AnimeNode:" + terrainNodeCounter.Anim.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - BasicNode:" + terrainNodeCounter.Basic.ToString());

            Reference.Viewer.GuiManager.DebugAdd(key, "ShaderNode");
            Reference.Viewer.GuiManager.DebugAdd(key, " - AnimeNode:" + shaderNodeCounter.Anim.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, " - BasicNode:" + shaderNodeCounter.Basic.ToString());
        }
        #endregion

        #region UserInformation
        private void UserInformation(object sender, EventArgs e)
        {
            int key = 2;

            if (Reference.Viewer.ProtocolManager.AvatarConnection.Connected == false)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, "Disconnect:");
                return;
            }

            if (Reference.Viewer.AvatarManager.UserObject.Prim == null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, "Not Info:");
                return;
            }

            Reference.Viewer.GuiManager.DebugAdd(key, "AnimationName:" + Reference.Viewer.AvatarManager.UserObject.AnimationCurrent.Key);
            Reference.Viewer.GuiManager.DebugAdd(key, "SittingOn:" + Reference.Viewer.ProtocolManager.AvatarConnection.SittingOn().ToString());

            float[] p = new float[3];

            //-------------------------------------------------------------
            // Avatar parameter.
            //-------------------------------------------------------------
            if (Reference.Viewer.AvatarManager.UserObject != null &&
                Reference.Viewer.AvatarManager.UserObject.Node.Children.Length > 1)
            {
                p[0] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Position.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Position.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Position.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "NodePos:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Rotation.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Rotation.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Rotation.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "NodeRot:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Scale.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Scale.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.MeshNode.Scale.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "NodeScale:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));
            }

            if (Reference.Viewer.AvatarManager.UserObject != null)
            {
                p[0] = Reference.Viewer.AvatarManager.UserObject.Prim.Position.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.Prim.Position.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.Prim.Position.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "PrimPos:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.Prim.Rotation.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.Prim.Rotation.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.Prim.Rotation.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "PrimRot:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.Prim.Scale.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.Prim.Scale.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.Prim.Scale.Z;
                Reference.Viewer.GuiManager.DebugAdd(key, "PrimScale:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.Prim.Velocity.X;
                p[1] = Reference.Viewer.AvatarManager.UserObject.Prim.Velocity.Y;
                p[2] = Reference.Viewer.AvatarManager.UserObject.Prim.Velocity.Z;
                float length = Reference.Viewer.AvatarManager.UserObject.Prim.Velocity.Length();
                Reference.Viewer.GuiManager.DebugAdd(key, "Velocity:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00") + " L:" + length.ToString("0000.000"));

                //-------------------------------------------------------------
                // Base paramater.
                //-------------------------------------------------------------
                Reference.Viewer.GuiManager.DebugAdd(key, "BaseParam:");

                p[0] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Position[0];
                p[1] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Position[1];
                p[2] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Position[2];
                Reference.Viewer.GuiManager.DebugAdd(key, " - Position:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Rotation[0];
                p[1] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Rotation[1];
                p[2] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Rotation[2];
                Reference.Viewer.GuiManager.DebugAdd(key, " - Rotation:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                p[0] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Scale[0];
                p[1] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Scale[1];
                p[2] = Reference.Viewer.AvatarManager.UserObject.BaseParam.Scale[2];
                Reference.Viewer.GuiManager.DebugAdd(key, " - Scale:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));
            }
            //-------------------------------------------------------------
            // Etc.
            //-------------------------------------------------------------
            Reference.Viewer.GuiManager.DebugAdd(key, "ETC:");

            if (Reference.Viewer.ProtocolManager.AvatarConnection != null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, " - Run:" + Reference.Viewer.ProtocolManager.AvatarConnection.Run.ToString());
                Reference.Viewer.GuiManager.DebugAdd(key, " - Fly:" + Reference.Viewer.ProtocolManager.AvatarConnection.Flying.ToString());
                Reference.Viewer.GuiManager.DebugAdd(key, " - RadHead:" + Reference.Viewer.AvatarManager.RadHeading.ToString("000.00"));
            }

            ulong regionID = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.Handle;
            uint localID = Reference.Viewer.AvatarManager.UserObject == null ? 0 : Reference.Viewer.AvatarManager.UserObject.Prim.ParentID;

            if (Reference.Viewer.EntityManager.Entities.ContainsKey(regionID.ToString() + localID.ToString()))
            {
                VObject parentObj = Reference.Viewer.EntityManager.Entities[regionID.ToString() + localID.ToString()];

                if (parentObj != null)
                {
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Parent Information:");

                    p[0] = parentObj.Prim.Position.X;
                    p[1] = parentObj.Prim.Position.Y;
                    p[2] = parentObj.Prim.Position.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " -  - PrimPos:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    p[0] = parentObj.Prim.Rotation.X;
                    p[1] = parentObj.Prim.Rotation.Y;
                    p[2] = parentObj.Prim.Rotation.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " -  - PrimRot:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    p[0] = parentObj.Prim.Scale.X;
                    p[1] = parentObj.Prim.Scale.Y;
                    p[2] = parentObj.Prim.Scale.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " -  - PrimScale:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));
                }
            }
        }
        #endregion

        #region ViewerInformation
        private void ViewerInformation(object sender, EventArgs e)
        {
            int key = 3;
            float[] p = new float[3];

            DateTime worldTime = Reference.Viewer.WorldTime;
            Reference.Viewer.GuiManager.DebugAdd(key, "WorldTime: H:" + worldTime.Hour.ToString() + " M:" + worldTime.Minute.ToString() + " S:" + worldTime.Second.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, "Cursor:" + Reference.Device.CursorControl.Position.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, "Focused:" + Reference.Viewer.GuiManager.Focused.ToString());

            if (Reference.Viewer.CacheManager != null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, "Cache:");
                Reference.Viewer.GuiManager.DebugAdd(key, " - Max:" + Reference.Viewer.CacheManager.CacheMaxSize.ToString("000,000,000,000"));
                Reference.Viewer.GuiManager.DebugAdd(key, " - Now:" + Reference.Viewer.CacheManager.CacheSize.ToString("000,000,000,000"));
            }

            if (Reference.Viewer.Camera != null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, "Camera:");
                if (Reference.Viewer.Camera.SNCamera != null)
                {
                    p[0] = Reference.Viewer.Camera.SNCamera.Position.X;
                    p[1] = Reference.Viewer.Camera.SNCamera.Position.Y;
                    p[2] = Reference.Viewer.Camera.SNCamera.Position.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Position:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    p[0] = Reference.Viewer.Camera.SNCamera.Target.X;
                    p[1] = Reference.Viewer.Camera.SNCamera.Target.Y;
                    p[2] = Reference.Viewer.Camera.SNCamera.Target.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Target:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    p[0] = Reference.Viewer.Camera.SNCamera.UpVector.X;
                    p[1] = Reference.Viewer.Camera.SNCamera.UpVector.Y;
                    p[2] = Reference.Viewer.Camera.SNCamera.UpVector.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Up:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    p[0] = Reference.Viewer.Camera.SNCamera.Rotation.X;
                    p[1] = Reference.Viewer.Camera.SNCamera.Rotation.Y;
                    p[2] = Reference.Viewer.Camera.SNCamera.Rotation.Z;
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Rotation:" + " X:" + p[0].ToString("0000.00") + " Y:" + p[1].ToString("0000.00") + " Z:" + p[2].ToString("0000.00"));

                    Reference.Viewer.GuiManager.DebugAdd(key, " - Near:" + Reference.Viewer.Camera.SNCamera.NearValue);
                    Reference.Viewer.GuiManager.DebugAdd(key, " - Far:" + Reference.Viewer.Camera.SNCamera.FarValue);
                }
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraMode:" + Reference.Viewer.Camera.CameraMode);
                Reference.Viewer.GuiManager.DebugAdd(key, " - MoveSpeed:" + Reference.Viewer.Camera.MoveSpeed);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraDistance:" + Reference.Viewer.CameraDistance);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraKeyWalkingDistance:" + Reference.Viewer.CameraKeyWalkingDistance);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraMaxDistance:" + Reference.Viewer.CameraMaxDistance);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraMinDistance:" + Reference.Viewer.CameraMinDistance);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraFOV:" + Reference.Viewer.CameraFOV);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraOffsetY:" + Reference.Viewer.CameraOffsetY);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraMinAngleY:" + Reference.Viewer.CameraMinAngleY);
                Reference.Viewer.GuiManager.DebugAdd(key, " - CameraMaxAngleY:" + Reference.Viewer.CameraMaxAngleY);
            }
            
        }
        #endregion

        #region ClientNetworkInformation
        private void ClientNetworkInformation(object sender, EventArgs e)
        {
            int key = 4;

            GridClient client = Reference.Viewer.ProtocolManager.AvatarConnection.m_user;

            if (client == null)
                return;

            NetworkManager network = client.Network;

            Reference.Viewer.GuiManager.DebugAdd(key, "AssetServerUri:" + network.AssetServerUri);
            Reference.Viewer.GuiManager.DebugAdd(key, "Connected:" + network.Connected);
            Reference.Viewer.GuiManager.DebugAdd(key, "LoginErrorKey:" + network.LoginErrorKey);
            Reference.Viewer.GuiManager.DebugAdd(key, "LoginMessage:" + network.LoginMessage);
            Reference.Viewer.GuiManager.DebugAdd(key, "SimCount:" + network.Simulators.Count);

            Reference.Viewer.GuiManager.DebugAdd(key, "CurrentSim:");
            if (network.CurrentSim != null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, " - Connected:" + network.CurrentSim.Connected);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Handle:" + network.CurrentSim.Handle);
                Reference.Viewer.GuiManager.DebugAdd(key, " - ID:" + network.CurrentSim.ID);
                Reference.Viewer.GuiManager.DebugAdd(key, " - IsRunning:" + network.CurrentSim.IsRunning);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Name:" + network.CurrentSim.Name);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Sequence:" + network.CurrentSim.Sequence);
                Reference.Viewer.GuiManager.DebugAdd(key, " - SimOwner:" + network.CurrentSim.SimOwner);
                Reference.Viewer.GuiManager.DebugAdd(key, " - SimVersion:" + network.CurrentSim.SimVersion);
                Reference.Viewer.GuiManager.DebugAdd(key, " - WaterHeigth:" + network.CurrentSim.WaterHeight);
            }
        }
        #endregion

        #region ClientSelfInformation
        private void ClientSelfInformation(object sender, EventArgs e)
        {
            int key = 5;

            GridClient client = Reference.Viewer.ProtocolManager.AvatarConnection.m_user;

            if (client == null)
                return;

            AgentManager agent = client.Self;

            Reference.Viewer.GuiManager.DebugAdd(key, "AgentID:" + agent.AgentID);
            Reference.Viewer.GuiManager.DebugAdd(key, "AgentAccess:" + agent.AgentAccess);
            Reference.Viewer.GuiManager.DebugAdd(key, "Health:" + agent.Health.ToString());
            Reference.Viewer.GuiManager.DebugAdd(key, "HomeLookAt:" + agent.HomeLookAt);
            Reference.Viewer.GuiManager.DebugAdd(key, "LocalID:" + agent.LocalID);
            Reference.Viewer.GuiManager.DebugAdd(key, "Name:" + agent.Name);
            Reference.Viewer.GuiManager.DebugAdd(key, "SittingOn:" + agent.SittingOn);
            Reference.Viewer.GuiManager.DebugAdd(key, "StartLocation:" + agent.StartLocation);
            Reference.Viewer.GuiManager.DebugAdd(key, "TeleportMessage:" + agent.TeleportMessage);
            Reference.Viewer.GuiManager.DebugAdd(key, "Velocity:" + agent.Velocity);

            AgentManager.AgentMovement movement = agent.Movement;
            if (movement != null)
            {
                Reference.Viewer.GuiManager.DebugAdd(key, "Movement:");
                Reference.Viewer.GuiManager.DebugAdd(key, " - AlwayRun:" + movement.AlwaysRun);
                Reference.Viewer.GuiManager.DebugAdd(key, " - AtNeg:" + movement.AtNeg);
                Reference.Viewer.GuiManager.DebugAdd(key, " - AtPos:" + movement.AtPos);
                Reference.Viewer.GuiManager.DebugAdd(key, " - AutoResetControls:" + movement.AutoResetControls);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Away:" + movement.Away);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Flay:" + movement.Fly);
                Reference.Viewer.GuiManager.DebugAdd(key, " - SitOnGround:" + movement.SitOnGround);
                Reference.Viewer.GuiManager.DebugAdd(key, " - StandUp:" + movement.StandUp);
                Reference.Viewer.GuiManager.DebugAdd(key, " - Stop:" + movement.Stop);
                Reference.Viewer.GuiManager.DebugAdd(key, " - UpdateEnabled:" + movement.UpdateEnabled);
            }
        }
        #endregion

        #region ElementsInformation
        private void ElementsInformation(object sender, EventArgs e)
        {
#if DEBUG
            // WARNING: Slow implementation for debug purposes only!!!
            int key = 6;

            Dictionary<string, int> elementGroups = new Dictionary<string, int>();
            lock (NativeElement.Elements)
            {
                foreach (IntPtr intptr in NativeElement.Elements.Keys)
                {
                    if (elementGroups.ContainsKey(NativeElement.Elements[intptr].GetType().ToString()))
                    {
                        elementGroups[NativeElement.Elements[intptr].GetType().ToString()]++;
                    }
                    else
                    {
                        elementGroups.Add(NativeElement.Elements[intptr].GetType().ToString(), 1);
                    }
                }

                List<string> elementGroupsList = new List<string>();
                foreach (string lmntGroup in elementGroups.Keys)
                {
                    elementGroupsList.Add("" + elementGroups[lmntGroup].ToString("D3") + ":" + lmntGroup);
                }

                elementGroupsList.Sort();
                elementGroupsList.Reverse();

                Reference.Viewer.GuiManager.DebugAdd(key, "Elements");
                foreach (string s in elementGroupsList)
                {
                    Reference.Viewer.GuiManager.DebugAdd(key, s);
                }
            }
#endif
        }
        #endregion

        #region QueueInformation
        private void QueueInformation(object sender, EventArgs e)
        {
#if DEBUG_QUEUE
            int key = 8;

            Reference.Viewer.GuiManager.DebugAdd(key, "EntityManager: objectQueue " + Reference.Viewer.EntityManager.ObjectQueueLength);
            Reference.Viewer.GuiManager.DebugAdd(key, "EntityManager: pipeline " + Reference.Viewer.EntityManager.PipelineQueueLength);
            Reference.Viewer.GuiManager.DebugAdd(key, "EntityManager: textureQueue " + Reference.Viewer.EntityManager.TextureQueueLength);

            Reference.Viewer.GuiManager.DebugAdd(key, "AvatarManager: pipeline " + Reference.Viewer.AvatarManager.PipelineQueueLength);
#endif
        }
        #endregion

        #region Util function
        private class NodeCounter
        {
            public int Anim = 0;
            public int Basic = 0;
        }

        private NodeCounter rootNodeCounter = new NodeCounter();
        private NodeCounter avatarNodeCounter = new NodeCounter();
        private NodeCounter primNodeCounter = new NodeCounter();
        private NodeCounter terrainNodeCounter = new NodeCounter();
        private NodeCounter shaderNodeCounter = new NodeCounter();

        private void CalcNodeRoot()
        {
            rootNodeCounter.Anim = 0;
            rootNodeCounter.Basic = 0;

            CalcNode(Reference.SceneManager.RootSceneNode, ref rootNodeCounter, true);
        }

        private void CalcNodeAvatarManager()
        {
            avatarNodeCounter.Anim = 0;
            avatarNodeCounter.Basic = 0;

            CalcNode(Reference.Viewer.AvatarManager.ParentNode, ref avatarNodeCounter, true);
        }

        private void CalcNodePrimManager()
        {
            primNodeCounter.Anim = 0;
            primNodeCounter.Basic = 0;

            CalcNode(Reference.Viewer.EntityManager.ParentNode, ref primNodeCounter, true);
        }

        private void CalcNodeTerrainManager()
        {
            terrainNodeCounter.Anim = 0;
            terrainNodeCounter.Basic = 0;

            CalcNode(Reference.Viewer.TerrainManager.ParentNode, ref terrainNodeCounter, true);
        }

        private void CalcNodeShaderManager()
        {
            shaderNodeCounter.Anim = 0;
            shaderNodeCounter.Basic = 0;

            CalcNode(Reference.Viewer.ShaderManager.ParentNode, ref shaderNodeCounter, true);
        }

        private void CalcNode(SceneNode _node, ref NodeCounter _nodeCounter, bool _deep)
        {
            for (int i = 0; i < _node.Children.Length; i++)
            {
                if (_node is IrrlichtNETCP.AnimatedMeshSceneNode)
                {
                    _nodeCounter.Anim++;
                }
                else
                {
                    _nodeCounter.Basic++;
                }

                if (_deep)
                    CalcNode(_node.Children[i], ref _nodeCounter, _deep);
            }
        }

        #endregion
    }
}
