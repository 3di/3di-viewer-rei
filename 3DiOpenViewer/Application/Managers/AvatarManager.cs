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
using System.Text;
using IrrlichtNETCP;
using IrrParseLib;
using OpenMetaverse;
using OpenViewer.Utility;
using OpenViewer.Primitives;

namespace OpenViewer.Managers
{
    public class AvatarManager : BaseManager
    {
        private const string ANIMATION_NAME_STANDING_SPEAK = "standing_speak";
        private const string ANIMATION_NAME_SITTING_SPEAK = "sitting_speak";

        //********************************************************
        // Element.
        //********************************************************
        // Const
        private const float CUSTOMIZE_ANIMATION_WAIT_SECOND = 0.5f;
        private const float CAMERA_RESET_LENGTH = 5.0f;
        private const float ROTATION_SPEED = 0.075f;
        private const float moveSpeed = 4.0f;
        private const float div2pi = (float)Math.PI / 2;
        private const float pi = (float)Math.PI;
        private const float pi2 = (float)Math.PI * 2;
        private const double ignoreMoveArea = 0.4f;
        private const double runLength = 3.5f;

        // General avatar.
        private EventHandler OnRequest;
        private Dictionary<string, VObject> entities = new Dictionary<string, VObject>();
        private Dictionary<UUID, string> customizeAnimationKey = new Dictionary<UUID, string>();
        private Queue<Action<VObject>> pipeline = new Queue<Action<VObject>>();
        private Queue<EventQueueParam> eventQueue = new Queue<EventQueueParam>();
        private List<SetTextureParam> requestTextureList = new List<SetTextureParam>();
        private HashedQueue<string, Action<VObject>> pipelineUpdate = new HashedQueue<string, Action<VObject>>();
        private VObject userObject;
        private UUID userUUID;
        private uint updateRate = 1;
        private uint updateCount = 200;

        private uint speakingCount = 0;
        private bool speaking = false;
        private bool avatarNameVisible = true;

        // User avatar.
        private float radHeading = 0;
        private bool radHeadingSmoothReset = false;
        private Vector3D m_userPosition = new Vector3D();
        private int customizeAnimationWait = 0;

        // Picke
        private Line3D projectedray;
        private MeshSceneNode pickSceneNode = null;
        private TrianglePickerMapper trianglePickerMapper = null;
        private VObject objectUnderMouse = null;

#if DEBUG_QUEUE
        public int PipelineQueueLength { get { return(pipelineUpdate.Count); } }
#endif
        //********************************************************
        // Property.
        //********************************************************
        public VObject UserObject { get { return userObject; } }
        public int EntitiesCount { get { lock (entities) { return entities.Count; } } }
        public Dictionary<string, VObject> Entities {get { return(entities); } }

        //********************************************************
        // Function.
        //********************************************************
        #region Base function.
        //--------------------------------------------------------
        // Base function.
        //--------------------------------------------------------
        public AvatarManager(Viewer _viewer)
            : base(_viewer, -1)
        {
            GeneratePickTile();

            if (Reference.Viewer.Config.Source.Configs["Startup"].Contains("visibleAvatarName"))
            {
                avatarNameVisible = Reference.Viewer.Config.Source.Configs["Startup"].GetBoolean("visibleAvatarName");
            }

            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_00, "customize00");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_01, "customize01");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_02, "customize02");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_03, "customize03");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_04, "customize04");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_05, "customize05");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_06, "customize06");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_07, "customize07");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_08, "customize08");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_09, "customize09");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_10, "customize10");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_11, "customize11");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_12, "customize12");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_13, "customize13");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_14, "customize14");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_15, "customize15");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_16, "customize16");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_17, "customize17");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_18, "customize18");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_19, "customize19");
            customizeAnimationKey.Add(VObject.CUSTOMIZE_ANIM_20, "customize20");

        }

        public override void Initialize()
        {
            Reference.Viewer.ProtocolManager.OnTextureFromWebLoaded -= ProtocolManager_OnTextureFromWebLoaded;
            Reference.Viewer.ProtocolManager.OnTextureFromWebLoaded += ProtocolManager_OnTextureFromWebLoaded;
            Reference.Viewer.TextureManager.OnTextureLoaded -= TextureManager_OnTextureLoaded;
            Reference.Viewer.TextureManager.OnTextureLoaded += TextureManager_OnTextureLoaded;

            trianglePickerMapper = new TrianglePickerMapper(Reference.SceneManager.CollisionManager);

            base.Initialize();
        }

        public void Start()
        {
            userObject = new VObject();
            userObject.Node = Reference.SceneManager.AddEmptySceneNode(ParentNode, -1);
        }

        public override void Update(uint frame)
        {
            try
            {
                if (OnRequest != null)
                    OnRequest(this, EventArgs.Empty);

                if (Reference.Device.WindowActive == false)
                {
                    UserPushForward(false);
                    UserPushBackward(false);
                }

                if (frame % 10 == 0)
                    EventControlProcess();

                if (frame % updateRate == 0)
                {
                    lock (userObject)
                    {
                        // sitting icon.
                        bool isSitting = Reference.Viewer.ProtocolManager.AvatarConnection.SittingOn() != 0;
                        Reference.Viewer.GuiManager.DrawChairButton(isSitting);
                        //if (!isSitting && )
                        //    Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStart(Animations.STAND);


                        if (Reference.Viewer.AvatarDisappearDistance < 0.001f)
                        {
                            userObject.Node.Visible = false;
                        }
                        else if (Reference.Viewer.Camera.SNCamera.Target.DistanceFrom(Reference.Viewer.Camera.SNCamera.Position) < Reference.Viewer.AvatarDisappearDistance)
                        {
                            userObject.Node.Visible = false;
                        }
                        else
                        {
                            userObject.Node.Visible = true;
                        }
                    }

                    ChangeSpeakAnimation();

                    ProcessObjectQueue();

                    if (userObject == null)
                        return;

                    // If myself is smoothing off , set camera.
                    if (userObject.SmoothingReset)
                    {
                        radHeadingSmoothReset = true;
                        Reference.Viewer.Camera.SmoothingReset = true;
                    }

                    // If avatar velocity's length over camera scape speed, camera smooth reset.
                    if (Reference.Viewer.AvatarManager.UserObject.Prim != null)
                    {
                        if (Reference.Viewer.AvatarManager.UserObject.Prim.Velocity.Length() > CAMERA_RESET_LENGTH)
                        {
                            Reference.Viewer.Camera.SmoothingReset = true;
                        }
                    }
                    float angleAmount = 0.2f;
                    if (radHeadingSmoothReset)
                    {
                        angleAmount = 1.0f;
                        radHeadingSmoothReset = false;
                    }
                    angleAmount = Utils.Lerp(angleAmount, 1, 1 - Reference.Viewer.FPSRate);
                    if (userObject.Prim != null)
                    {
                        if (userObject.Prim.ParentID == 0)
                        {
                            Vector3D targetRotation = new Vector3D(0, Utils.ToDegrees(pi2 - radHeading), 0);
                            userObject.Node.Rotation = Util.Lerp(userObject.Node.Rotation, targetRotation, angleAmount);
                        }
                    }
                    lock (entities)
                    {
                        foreach (string key in entities.Keys)
                        {
                            VObject obj = entities[key];
                            if (obj.Node != null)
                            {
                                Vector3D nowPosition = obj.Node.Position;

                                float amount = 0.25f;
                                amount = Utils.Lerp(amount, 1, 1 - Reference.Viewer.FPSRate);
                                if (obj.SmoothingReset)
                                {
                                    amount = 1.0f;
                                    obj.SmoothingReset = false;
                                }
                                if (obj.TargetPosition.DistanceFrom(nowPosition) > 0.1f)
                                    obj.Node.Position = Util.Lerp(nowPosition, obj.TargetPosition, amount);
                            }
                        }
                    }
                }
                if (userObject.Node != null)
                {
                    if (Reference.Viewer.StateManager.State == State.CONNECTED)
                    {
                        if (userObject != null)
                        {
                            m_userPosition = userObject.Node.Position;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Reference.Log.Debug("Update: " + e.Message);
            }

            if (customizeAnimationWait > 0)
                customizeAnimationWait--;

            base.Update(frame);
        }

        public override void Draw()
        {
#if YK_DEBUG
            Vector3D vec = new Vector3D(0, 0, 1);
            Vector3D start = new Vector3D();
            Vector3D end = new Vector3D();

            if (projectedray.Vector.LengthSQ > 0)
                vec = projectedray.Vector.Normalize();

            if (Reference.Viewer.Camera.SNCamera != null)
                start = Reference.Viewer.Camera.SNCamera.Position + vec * 1;

            if (Reference.Viewer.Camera.SNCamera != null)
                end = start + vec * 2;

            Reference.VideoDriver.Draw3DLine(new Vector3D(128, 22, 128), new Vector3D(130, 25, 130), Color.Green);
            Reference.VideoDriver.Draw3DTriangle(new Triangle3D(new Vector3D(124, 25, 128), new Vector3D(128, 25, 124), new Vector3D(128, 25, 128)), Color.Red);
#endif

            base.Draw();
        }

        public override void Cleanup()
        {
            userObject = null;

            OnRequest = null;

            lock (entities)
            {
                entities.Clear();
            }
            lock (pipeline)
            {
                pipeline.Clear();
            }
            lock (pipelineUpdate)
            {
                pipelineUpdate.Clear();
            }
            base.Cleanup();
        }
        #endregion

        #region General avatar function.
        //--------------------------------------------------------
        // General avatar function.
        //--------------------------------------------------------
        public void AvatarPickRequest()
        {
            OnRequest += AvatarPickListener;
        }

        private void AvatarPickListener(object _object, EventArgs _args)
        {
            DetectObjectUnderMouse();
            OnRequest -= AvatarPickListener;
        }

        public VObject GetVObjectFromMeshRaw(IntPtr _meshRaw)
        {
            VObject vObj = null;

            lock (entities)
            {
                foreach (VObject obj in entities.Values)
                {
                    if (obj == null)
                        continue;

                    if (obj.MeshNode.Raw == _meshRaw)
                    {
                        vObj = obj;
                        break;
                    }
                }
            }

            return vObj;
        }

        public bool IsContain(string _uuid)
        {
            bool flag = false;

            lock (entities)
            {
                foreach (VObject obj in entities.Values)
                {
                    if (obj.Prim == null)
                        continue;

                    if (obj.Prim.ID == new UUID(_uuid))
                    {
                        flag = true;
                        break;
                    }
                }
            }

            return flag;
        }

        public void AddObject(OpenMetaverse.Avatar _avatar, ulong _regionHandle)
        {
            // Unsafe if the currentsim in libomv is not initialized yet, nullreferenceexception will be raised
            //ulong regionID = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.Handle;
            //if (regionID != _regionHandle)
            //    return;

            _avatar.RegionHandle = _regionHandle;

            string key = _regionHandle.ToString() + _avatar.LocalID.ToString();

            VObject newObj = new VObject();
            newObj.Prim = _avatar;
            newObj.Node = null;

            VObject delObj = null;

            lock (pipeline)
            {
                lock (entities)
                {
                    // WARNING: this decision about delObj using entities must be in the 
                    // **same lock** as the Enqueue() call below! Otherwise, it is possible that
                    // this thread is interrupted after we decide that delObj is NULL, but during
                    // that interruption ANOTHER OnNewAvatar event fires which inserts an avatar
                    // into the Entities, meaning our information and decision about delObj
                    // is now out-of-date.
                    if (entities.ContainsKey(key))
                    {
                        delObj = entities[key];

                        entities.Remove(key);
                    }

                    entities.Add(key, newObj);
                }

                // Atomic operation. Ensure DELETION->ADDITION
                //lock (pipeline) //<-- moved upwards, see WARNING above
                {
                    if (delObj != null)
                    {
                        pipeline.Enqueue(new Action<VObject>(delObj, Operations.DELETE));
                    }
                    pipeline.Enqueue(new Action<VObject>(newObj, Operations.ADD));
                }
            }
        }

        public VObject GetVObjectFromObjectUUID(UUID _primID)
        {
            VObject vObj = null;

            lock (entities)
            {
                foreach (VObject vo in entities.Values)
                {
                    if (vo.Prim == null)
                        continue;

                    if (vo.Prim.ID == _primID)
                    {
                        vObj = vo;
                        break;
                    }
                }
            }

            return vObj;
        }

        public string GetEntitiesKeyFromObjectUUID(UUID _primID)
        {
            VObject vObj = GetVObjectFromObjectUUID(_primID);

            if (vObj == null || vObj.Prim == null)
                return string.Empty;

            return VUtil.GetEntitiesKeyFromPrim(vObj.Prim);
        }

        public void UpdateObject(UUID _avatarUUID)
        {
            VObject obj = null;

            lock (entities)
            {
                foreach (VObject vo in entities.Values)
                {
                    if (vo.Prim != null)
                    {
                        if (vo.Prim.ID == _avatarUUID)
                        {
                            obj = vo;
                            break;
                        }
                    }
                }
            }

            if (obj != null)
                UpdateObjectToPiplineEnqueue(obj);
        }

        public void UpdateObject(OpenMetaverse.ObjectUpdate _update, ulong _regionHandle)
        {
            if (Reference.Viewer.StateManager.State != State.LOGIN &&
                Reference.Viewer.StateManager.State != State.DOWNLOADING &&
                Reference.Viewer.StateManager.State != State.TELEPORTING &&
                Reference.Viewer.StateManager.State != State.CONNECTED)
                return;
            
            string key = _regionHandle.ToString() + _update.LocalID.ToString();
            VObject obj = null;

            lock (entities)
            {
                if (entities.ContainsKey(key))
                {
                    obj = entities[key];

                    obj.Prim.Acceleration = _update.Acceleration;
                    obj.Prim.AngularVelocity = _update.AngularVelocity;
                    obj.Prim.CollisionPlane = _update.CollisionPlane;
                    obj.Prim.Position = _update.Position;
                    obj.Prim.Rotation = _update.Rotation;
                    obj.Prim.PrimData.State = _update.State;
                    obj.Prim.Textures = _update.Textures;
                    obj.Prim.Velocity = _update.Velocity;

                    entities[key] = obj;
                }
            }

            UpdateObjectToPiplineEnqueue(obj);
        }

        public void UpdateAllObjects()
        {
            lock (entities)
            {
                foreach (VObject obj in entities.Values)
                {
                    UpdateObjectToPiplineEnqueue(obj);
                }
            }
        }

        private void UpdateObjectToPiplineEnqueue(VObject _obj)
        {
            if (_obj == null)
            {
                return;
            }

            string key = String.Format("{0}:{1}", _obj.Prim.RegionHandle, _obj.Prim.LocalID);
            Pair<string, Action<VObject>> pair;
            Action<VObject> action = new Action<VObject>(_obj, Operations.UPDATE);

            lock (pipelineUpdate)
            {
                if (pipelineUpdate.TryGetValue(key, out pair))
                {
                    pair.Value = action;
                }
                else
                {
                    pipelineUpdate.Enqueue(key, action);
                }
            }
        }

        public void DeleteObject(ulong _regionHandle, uint _localID)
        {
            string key = _regionHandle.ToString() + _localID.ToString();
            VObject obj = null;

            lock (entities)
            {
                if (entities.ContainsKey(key))
                {
                    obj = entities[key];

                    entities.Remove(key);
                }
            }

            if (obj == null)
            {
                return;
            }
            else
            {
                // Must not access Node outside Initialize->Update->Cleanup
                //if (obj.Node != null)
                //    obj.Node.Visible = false;
            }

            lock (pipeline)
            {
                pipeline.Enqueue(new Action<VObject>(obj, Operations.DELETE));
            }
        }

        public bool ContainObject(ulong _regionHandle, uint _localID)
        {
            bool retVal;
            lock(entities)
            {
                retVal = entities.ContainsKey(_regionHandle.ToString() + _localID.ToString());
            }
            return retVal;
        }

        public void VisibleName(bool _visible)
        {
            lock (entities)
            {
                if (entities.Count > 0)
                {
                    foreach (string key in entities.Keys)
                    {
                        VObject obj = entities[key];
                        SetVisibleNameAllNode(obj.Node, _visible);
                    }
                }
            }
        }

        public string GetUserUUIDList()
        {
            if (entities.Count == 0)
                return "{}";

            StringBuilder sb = new StringBuilder("{\"id\":[");
            lock (entities)
            {
                foreach (VObject vObject in entities.Values)
                {
                    if (vObject == null || vObject.Prim == null)
                        continue;

                    sb.Append("\"" + vObject.Prim.ID.ToString() + "\",");
                }
            }

            sb.Remove(sb.Length - 1, 1);// Remove last ","
            sb.Append("]}");

            return sb.ToString();
        }

        public void RequestCustomizeAnimation(int _index)
        {
            switch (_index)
            {
                case 0: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_00); break;
                case 1: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_01); break;
                case 2: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_02); break;
                case 3: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_03); break;
                case 4: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_04); break;
                case 5: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_05); break;
                case 6: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_06); break;
                case 7: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_07); break;
                case 8: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_08); break;
                case 9: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_09); break;
                case 10: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_10); break;
                case 11: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_11); break;
                case 12: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_12); break;
                case 13: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_13); break;
                case 14: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_14); break;
                case 15: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_15); break;
                case 16: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_16); break;
                case 17: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_17); break;
                case 18: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_18); break;
                case 19: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_19); break;
                case 20: RequestCustomizeAnimation(VObject.CUSTOMIZE_ANIM_20); break;
            }
        }

        public void RequestCustomizeAnimation(UUID _animationUUID)
        {
            if (customizeAnimationKey.ContainsKey(_animationUUID))
            {
                lock (userObject)
                {
                    if (userObject.FrameSetList.ContainsKey(customizeAnimationKey[_animationUUID]) == false)
                        return;
                }
            }

            if (customizeAnimationWait > 0)
                return;

            // If playing animation is customize animation, stop current animation.
            if (userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_00
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_01
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_02
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_03
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_04
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_05
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_06
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_07
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_08
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_09
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_10
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_11
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_12
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_13
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_14
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_15
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_16
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_17
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_18
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_19
                || userObject.CurrentAnimationUUID == VObject.CUSTOMIZE_ANIM_20
                )
            {
                Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(userObject.CurrentAnimationUUID);
            }

            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStart(_animationUUID);

            customizeAnimationWait = (int)(CUSTOMIZE_ANIMATION_WAIT_SECOND * 30);
        }

        private void ChangeSpeakAnimation()
        {
            VObject vobj = null;
            lock (entities)
            {
                foreach (string vobjkey in entities.Keys)
                {
                    vobj = entities[vobjkey];

                    if (vobj.VoiceLevel > 0 || speaking)
                    {
                        string requestAnimationName = string.Empty;

                        switch (vobj.AnimationCurrent.Key)
                        {
                            case "standing":
                                requestAnimationName = ANIMATION_NAME_STANDING_SPEAK;
                                break;

                            case "sitting":
                                requestAnimationName = ANIMATION_NAME_SITTING_SPEAK;
                                break;
                        }

                        if (vobj.VoiceLevel > 0)
                        {
                            speakingCount = 0;
                            speaking = true;
                        }
                        else
                        {
                            const int fps = 30;
                            if ((speakingCount++ > (Reference.Viewer.VoiceWaitTime * fps)) || !UtilityAnimation.IsPossibleVoiceAnimation(vobj.AnimationCurrent.Key))
                            {
                                vobj.SetAnimationLoop(false);
                                speaking = false;
                            }
                        }

                        if (!string.IsNullOrEmpty(requestAnimationName))
                            SetAnimation(vobj, requestAnimationName);
                    }
                }
            }
        }

        private void SetVisibleNameAllNode(SceneNode _node, bool _visible)
        {
            if (_node == null)
                return;

            for (int i = 0; i < _node.Children.Length; i++)
            {
                if (_node.Children[i] is TextSceneNode)
                    _node.Children[i].Visible = _visible;

                SetVisibleNameAllNode(_node.Children[i], _visible);
            }
        }
        #endregion

        #region Queue control function.
        //--------------------------------------------------------
        // Queue control function.
        //--------------------------------------------------------
        private void ProcessObjectQueue()
        {
            lock (pipeline)
            {
                uint processed = 0;
                
                while (pipeline.Count > 0 && processed < updateCount)
                {
                    Action<VObject> action = pipeline.Dequeue();
                    processed++;
                    switch (action.Operation)
                    {
                        case Operations.ADD:
                            ProcessObjectQueueAddToNode(action.Object);
                            break;

                        case Operations.UPDATE:
                            ProcessObjectQueueUpdateToNode(action.Object);
                            break;

                        case Operations.DELETE:
                            ProcessObjectQueueDeleteToNode(action.Object);
                            break;
                    }
                }
            }

            lock (pipelineUpdate)
            {
                uint processed = 0;

                while (pipelineUpdate.Count > 0 && processed < updateCount)
                {
                    Action<VObject> action = pipelineUpdate.Dequeue();
                    processed++;
                    ProcessObjectQueueUpdateToNode(action.Object);
                }
            }

            if (userObject != null)
            {
                pickSceneNode.Position = userObject.Node.Position;
            }
        }

        private void ProcessObjectQueueAddToNode(VObject _obj)
        {
            // Get irr file uuid.
            if (string.IsNullOrEmpty(Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.AssetServerUri))
            {
                _obj.RequestIrrfileUUID = new UUID("70101ee9-888d-5bd6-371f-925c9b668ce3");
            }
            else
            {
                _obj.RequestIrrfileUUID = IrrManager.GetIrrfileUUID(Reference.Viewer.ServerURI, _obj.Prim.ID);
            }
            // Create empty node.
            _obj.Node = Reference.SceneManager.AddEmptySceneNode(ParentNode, (int)_obj.Prim.LocalID);

            _obj.SmoothingReset = true;

            // Set ghost avatar and name plate.
            Reference.Viewer.EffectManager.AddGhostSceneNode(_obj.Node);
            _obj.IsGhost = true;
            if (avatarNameVisible)
            {
                Reference.Viewer.EffectManager.AddNameSceneNode(_obj.Node, ((Avatar)_obj.Prim).Name, false);
                _obj.VoiceNode = Reference.Viewer.EffectManager.AddVoiceEffectSceneNode(_obj.Node);
            }

            AnimatedMesh animMesh = Reference.SceneManager.GetMesh(Util.ApplicationDataDirectory + @"\media\models\avatar_pick_object.x");
            _obj.PickNode = Reference.SceneManager.AddMeshSceneNode(animMesh.GetMesh(0), _obj.Node, _obj.Node.ID);
            _obj.PickNode.SetMaterialFlag(MaterialFlag.Lighting, false);
            _obj.PickNode.SetMaterialType(MaterialType.TransparentAlphaChannel);

            // Create triangle sector of pick object.
            TriangleSelector trisel = Reference.SceneManager.CreateTriangleSelector(animMesh.GetMesh(0), _obj.PickNode);
            _obj.PickNode.TriangleSelector = trisel;
            trianglePickerMapper.AddTriangleSelector(trisel, _obj.PickNode);
            trisel.Drop();

            if (_obj.Prim.ID == Reference.Viewer.ProtocolManager.AvatarConnection.GetSelfUUID)
            {
                userObject = _obj;
                userUUID = Reference.Viewer.ProtocolManager.AvatarConnection.GetSelfUUID;
            }
            ProcessObjectQueueUpdateToNode(_obj);

            Reference.Log.Debug("Added avatar: Name:" + ((Avatar)_obj.Prim).Name + " UUID:" + _obj.RequestIrrfileUUID.ToString());
        }
        
        private void ProcessObjectQueueUpdateToNode(VObject _obj)
        {
            // Little known fact.  Dead avatar in LibOMV have the word 'dead' in their UUID
            // Skip over this one and move on to the next one if it's dead.
            if (((Avatar)_obj.Prim).ID.ToString().Contains("dead"))
            {
                return;
            }

            if (_obj.Node == null)
            {
                return;
            }

            //If we don't have an avatar representation yet for this avatar or it's a full update
            if (_obj.Requesting == false)
            {
                if ((_obj._3DiIrrfileUUID != _obj.RequestIrrfileUUID) && (_obj.RequestIrrfileUUID != UUID.Zero))
                {
                    if (Reference.Viewer.IrrManager.Contains(_obj.RequestIrrfileUUID) == false)
                    {
                        _obj.requestTexturesDirectlyFromAssetServerWithoutJ2KConversion = true; 
                        Reference.Viewer.IrrManager.RequestObject(_obj);
                        Reference.Log.Debug("Request object: UUID:" + _obj.RequestIrrfileUUID.ToString());
                    }
                    else
                    {
                        _obj._3DiIrrfileUUID = _obj.RequestIrrfileUUID;

                        IrrDatas datas = Reference.Viewer.IrrManager.GetObject(_obj._3DiIrrfileUUID, true);
                        if (datas != null)
                        {
                            // Set avatar mesh.
                            lock (entities)
                            {
                                Reference.Viewer.EffectManager.RemoveGhostNode(_obj.Node);
                                _obj.IsGhost = false;

                                AnimatedMeshSceneNode animeNode = Reference.Viewer.IrrManager.IrrFileLoad(datas, Reference.SceneManager, _obj, "tmpmesh_" + _obj.Prim.LocalID.ToString() + "_");
                                if (animeNode != null)
                                {
                                    _obj.Mesh = animeNode.AnimatedMesh.GetMesh(0);
                                    SetAnimation(_obj, "standing");
                                    animeNode.AnimationEnd += animeNode_AnimationEnd;
                                    animeNode.AnimationEnd += _obj.AnimationEndHandler;
                                }
                            }
                        }

                        Reference.Log.Debug("Loaded object: UUID:" + _obj.RequestIrrfileUUID.ToString());
                    }
                }
            }

            AnimationFrame(_obj);

            _obj.TargetPosition = new Vector3D(_obj.Prim.Position.X, _obj.Prim.Position.Z - 0.83f, _obj.Prim.Position.Y);
            _obj.Velocity = new Vector3D(_obj.Prim.Velocity.X, _obj.Prim.Velocity.Z, _obj.Prim.Velocity.Y);

            if (_obj.Prim.ID != userUUID)
            {
                // REVIEW NEEDED: a more general calculation
                //float roll, pitch, yaw;
                //_obj.Prim.Rotation.GetEulerAngles(out roll, out pitch, out yaw);
                //_obj.Node.Rotation = new Vector3D(Utils.ToDegrees(roll), Utils.ToDegrees(pi2 - pitch), Utils.ToDegrees(yaw));
                Vector3 axis;
                float angle;
                _obj.Prim.Rotation.GetAxisAngle(out axis, out angle);
                _obj.Node.Rotation = new Vector3D(0, Utils.ToDegrees((axis.Z > 0 ? 1 : -1) * (pi2 - angle)), 0);
            }

            // If exsit parent prim, set parent.
            uint parentID = _obj.Prim.ParentID;
            if (parentID == 0)
            {
                _obj.ParentPosition = new Vector3();

                if (_obj.IsChildAgent)
                {
                    _obj.IsChildAgent = false;
                    _obj.SmoothingReset = true;
                }
            }
            else
            {
                ulong regionID = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.Handle;

                if (Reference.Viewer.EntityManager.Entities.ContainsKey(regionID.ToString() + parentID.ToString()))
                {
                    VObject parentObj = Reference.Viewer.EntityManager.Entities[regionID.ToString() + parentID.ToString()];

                    if (parentObj != null)
                    {
                        _obj.ParentPosition = parentObj.Prim.Position;

                        IrrlichtNETCP.Quaternion iquParent = new IrrlichtNETCP.Quaternion
                            (parentObj.Prim.Rotation.X,
                             parentObj.Prim.Rotation.Z,
                             parentObj.Prim.Rotation.Y,
                             parentObj.Prim.Rotation.W);
                        iquParent.makeInverse();
                        IrrlichtNETCP.Quaternion iquAvatar = new IrrlichtNETCP.Quaternion
                            (_obj.Prim.Rotation.X,
                             _obj.Prim.Rotation.Z,
                             _obj.Prim.Rotation.Y,
                             _obj.Prim.Rotation.W);
                        iquAvatar.makeInverse();

                        IrrlichtNETCP.Quaternion finalRotation = iquAvatar * iquParent;
                        _obj.Node.Rotation = finalRotation.Matrix.RotationDegrees;
                        _obj.TargetPosition = parentObj.Node.Position + _obj.TargetPosition * iquParent;
                    }

                    _obj.SmoothingReset = true;
                    _obj.IsChildAgent = true;
                }
                else
                {
                    UpdateObjectToPiplineEnqueue(_obj);
                }
            }
            _obj.SyncToChilds();

            // If avatar's height position is minus, teleport to current sim center.
            if (_obj.Prim.ID == Reference.Viewer.ProtocolManager.AvatarConnection.GetSelfUUID && _obj.Prim.Position.Z < 0)
                Reference.Viewer.ProtocolManager.Teleport(Reference.Viewer.ProtocolManager.GetCurrentSimName(), 128, 128, 128); 
        }

        private void animeNode_AnimationEnd(AnimatedMeshSceneNode node)
        {
            VObject targetVObj = GetVObjectFromMeshRaw(node.Raw);

            if (targetVObj == null)
                return;

            if (userObject.MeshNode.Raw == targetVObj.MeshNode.Raw)
            {
                if (StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_00) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_01) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_02) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_03) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_04) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_05) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_06) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_07) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_08) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_09) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_10) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_11) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_12) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_13) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_14) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_15) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_16) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_17) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_18) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_19) ||
                    StopAnimationIfIsKeyCurrentAnimation(UtilityAnimation.CUSTOMIZE_ANIM_20)
                    )
                {
                    Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStart(Animations.STAND);
                    return;
                }
            }

            if (!string.IsNullOrEmpty(targetVObj.AnimationNext.Key))
            {
                StopAnimationIfIsKeyCurrentAnimation(targetVObj.CurrentAnimationUUID);
                SetAnimation(targetVObj, targetVObj.AnimationNext.Key);
            }
        }

        private bool StopAnimationIfIsKeyCurrentAnimation(UUID _animationUUID)
        {
            lock (userObject)
            {
                if (userObject.CurrentAnimationUUID == _animationUUID)
                {
                    Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(_animationUUID);
                    return true;
                }
            }

            return false;
        }

        private void ProcessObjectQueueDeleteToNode(VObject _obj)
        {
            if (_obj.Node == null)
                return;

            Reference.Log.Debug("Delete avatar: Name:" + ((Avatar)_obj.Prim).Name + " Pos:" + _obj.Prim.Position.ToString());
            if (_obj.IsGhost)
            {
                Reference.Viewer.EffectManager.RemoveGhostNode(_obj.Node);
            }

            // Remove this object from our picker.
            if (_obj.PickNode.TriangleSelector != null)
            {
                if (trianglePickerMapper != null)
                {
                    trianglePickerMapper.RemTriangleSelector(_obj.PickNode.TriangleSelector);
                }
                lock (NativeElement.Elements) { if (NativeElement.Elements.ContainsKey(_obj.PickNode.TriangleSelector.Raw)) { NativeElement.Elements.Remove(_obj.PickNode.TriangleSelector.Raw); } }
            }

            _obj.VoiceNode = null;
            _obj.MeshNode = null;
            _obj.PickNode = null;

            SceneNode node = null;
            if (userObject.Node.Raw == _obj.Node.Raw)
            {
                node = Reference.SceneManager.AddEmptySceneNode(ParentNode, -1);
            }
            Reference.SceneManager.AddToDeletionQueue(_obj.Node);

            if (node != null)
                userObject.Node = node;
        }

        /// <summary>
        /// Animations that are received are stored in a dictionary in the protocol module and associated
        /// with an avatar. They are removed from that dictionary here and applied to the proper avatars
        /// in the scene.
        /// </summary>
        private void AnimationFrame(VObject _obj)
        {
            if (_obj.MeshNode is AnimatedMeshSceneNode)
            {
                string key = GetAnimationKey(_obj.Prim.ID);

               if (!string.IsNullOrEmpty(key))
                   SetAnimation(_obj, key);
            }
        }

        private void SetAnimation(VObject _obj, string _key)
        {
            _obj.SetNextAnimation(string.Empty, true);

            if (!_obj.FrameSetList.ContainsKey(_key))
                return;

            if (_obj.PickNode != null)
            {
                _obj.PickNode.Position = new Vector3D(0, 0, 0);
                _obj.PickNode.Scale = new Vector3D(1, 1, 1);
            }

            bool isVoiceAnimation = (_obj.VoiceLevel > 1) ? true : false;
            bool loopFlag = true;
            switch (_key)
            {
                case "sitstart":
                    _obj.SetNextAnimation("sitting", true);
                    if (_obj.PickNode != null)
                    {
                        _obj.PickNode.Position = new Vector3D(-0.35f, 0.1f, 0);
                        _obj.PickNode.Scale = new Vector3D(1, 0.75f, 1);
                    }
                    loopFlag = false;
                    break;

                case UtilityAnimation.ANIMATION_KEY_SPEAK_SITTING:
                    _obj.SetNextAnimation(UtilityAnimation.ANIMATION_KEY_SPEAK_SITTING_END, false);
                    break;
                case UtilityAnimation.ANIMATION_KEY_SPEAK_SITTING_END:
                    _obj.SetNextAnimation(UtilityAnimation.ANIMATION_KEY_SITTING, true);
                    loopFlag = false;
                    break;
                case UtilityAnimation.ANIMATION_KEY_SPEAK_STANDING:
                    _obj.SetNextAnimation(UtilityAnimation.ANIMATION_KEY_SPEAK_STANDING_END, false);
                    break;
                case UtilityAnimation.ANIMATION_KEY_SPEAK_STANDING_END:
                    _obj.SetNextAnimation(UtilityAnimation.ANIMATION_KEY_STANDING, true);
                     loopFlag = false;
                     break;
                case "customize00":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_00;
                    loopFlag = false;
                    break;
                case "customize01":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_01;
                    loopFlag = false;
                    break;
                case "customize02":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_02;
                    loopFlag = false;
                    break;
                case "customize03":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_03;
                    loopFlag = false;
                    break;
                case "customize04":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_04;
                    loopFlag = false;
                    break;
                case "customize05":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_05;
                    loopFlag = false;
                    break;
                case "customize06":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_06;
                    loopFlag = false;
                    break;
                case "customize07":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_07;
                    loopFlag = false;
                    break;
                case "customize08":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_08;
                    loopFlag = false;
                    break;
                case "customize09":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_09;
                    loopFlag = false;
                    break;
                case "customize10":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_10;
                    loopFlag = false;
                    break;
                case "customize11":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_11;
                    loopFlag = false;
                    break;
                case "customize12":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_12;
                    loopFlag = false;
                    break;
                case "customize13":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_13;
                    loopFlag = false;
                    break;
                case "customize14":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_14;
                    loopFlag = false;
                    break;
                case "customize15":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_15;
                    loopFlag = false;
                    break;
                case "customize16":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_16;
                    loopFlag = false;
                    break;
                case "customize17":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_17;
                    loopFlag = false;
                    break;
                case "customize18":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_18;
                    loopFlag = false;
                    break;
                case "customize19":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_19;
                    loopFlag = false;
                    break;
                case "customize20":
                    _obj.CurrentAnimationUUID = VObject.CUSTOMIZE_ANIM_20;
                    loopFlag = false;
                    break;

                default:
                    _obj.CurrentAnimationUUID = new UUID();
                    break;
            }

            int startFrame = _obj.FrameSetList[_key].StartFrame;
            int endFrame = _obj.FrameSetList[_key].EndFrame;
            int animFramesPerSecond = _obj.FrameSetList[_key].AnimationSpeed;

            _obj.AnimationSpeed = animFramesPerSecond;
            _obj.SetAnimation(_key, startFrame, endFrame, loopFlag);
        }

        private string GetAnimationKey(UUID _uuid)
        {
            string key = string.Empty;
            List<UUID> newAnims = null;

            lock (Reference.Viewer.ProtocolManager.AvatarConnection.AvatarAnimations)
            {
                // fetch any pending animations from the dictionary and then
                // delete them from the dictionary
                if (Reference.Viewer.ProtocolManager.AvatarConnection.AvatarAnimations.ContainsKey(_uuid))
                {
                    newAnims = Reference.Viewer.ProtocolManager.AvatarConnection.AvatarAnimations[_uuid];
                    Reference.Viewer.ProtocolManager.AvatarConnection.AvatarAnimations.Remove(_uuid);
                }
            }

            if (newAnims != null)
            {
                foreach (UUID animID in newAnims)
                {
                    key = UtilityAnimation.GetAnimationKeyFromAnimationUUID(animID);

                    // customize animation.
                    if (string.IsNullOrEmpty(key))
                        if (customizeAnimationKey.ContainsKey(animID))
                            key = customizeAnimationKey[animID];

                    Reference.Log.Debug("UUID:" + _uuid + " Key:" + key);
                }
            }

            return key;
        }

        #endregion

        #region User avatar function.
        //--------------------------------------------------------
        // User avatar function.
        //--------------------------------------------------------
        public void Standup()
        {
            lock (userObject)
                userObject.SetNextAnimation(string.Empty, true);

            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(Animations.SIT);
            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(Animations.SIT_FEMALE);
            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(Animations.SIT_GENERIC);
            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(Animations.SIT_GROUND);
            Reference.Viewer.ProtocolManager.AvatarConnection.RequestAnimationStop(Animations.SIT_GROUND_staticRAINED);

            Reference.Viewer.ProtocolManager.StandUp();
        }

        public Vector3 GetUserAvatarPosition()
        {
            Vector3 position = new Vector3();
            lock (userObject)
            {
                position = userObject.Prim.Position + userObject.ParentPosition;
            }
            return position;
        }

        public void UserUpdateMousePosition(Position2D _position)
        {
            UserAvatarMoveMouse(_position);
        }

        public void UserPushForward(bool _press)
        {
            //huanvn - prevent sending SetAlwaysRun packet again and again
            if (Reference.Viewer.ProtocolManager.AvatarConnection.Run)
                Reference.Viewer.ProtocolManager.AvatarConnection.Run = false;
            Reference.Viewer.ProtocolManager.AvatarConnection.Forward = _press;
        }

        public void UserPushBackward(bool _press)
        {
            //huanvn - prevent sending SetAlwaysRun packet again and again
            if (Reference.Viewer.ProtocolManager.AvatarConnection.Run)
                Reference.Viewer.ProtocolManager.AvatarConnection.Run = false;
            Reference.Viewer.ProtocolManager.AvatarConnection.Backward = _press;
        }

        public void UserPushLeft()
        {
            UserAvatarRotation(ROTATION_SPEED);
        }

        public void UserPushRight()
        {
            UserAvatarRotation(-ROTATION_SPEED);
        }

        public void UserSwitchRun()
        {
            if (Reference.Viewer.ProtocolManager.AvatarConnection == null)
                return;

            Reference.Viewer.ProtocolManager.AvatarConnection.Run = !Reference.Viewer.ProtocolManager.AvatarConnection.Run;
        }

        public void UserSwitchFly()
        {
            if (Reference.Viewer.ProtocolManager.AvatarConnection == null)
                return;

            Reference.Viewer.ProtocolManager.AvatarConnection.Flying = !Reference.Viewer.ProtocolManager.AvatarConnection.Flying;
        }

        public void UserJump()
        {
            Reference.Viewer.ProtocolManager.AvatarConnection.Jump = true;
        }

        public void UserPushUp(bool _press)
        {
            Reference.Viewer.ProtocolManager.AvatarConnection.Up = _press;
        }

        public void UserPushDown(bool _press)
        {
            Reference.Viewer.ProtocolManager.AvatarConnection.Down = _press;
        }

        private void UserAvatarRotation(float _addRadAngle)
        {
            radHeading += _addRadAngle;
            Reference.Viewer.ProtocolManager.AvatarConnection.UpdateFromHeading(radHeading);
        }

        private void UserAvatarMoveMouse(Position2D _position)
        {
            if (Reference.Viewer.GuiManager.Focused)
            {
                Reference.Viewer.ProtocolManager.AvatarConnection.Forward = false; 
                return;
            }

            // Get target position from mouse cursor position.
            Vector3D targetPosition = new Vector3D();
            Triangle3D triangle = new Triangle3D();
            bool find = FindPositionFromMousePosition(_position, out targetPosition, out triangle);
            if (find == false)
            {
                return;
            }

            targetPosition.Y = 0;


            // Get user avatar position.
            Vector3D userPosition = m_userPosition; //userObject.Node.Position;
            userPosition.Y = 0;

            bool isRunLenght = false;

            // Create target vector.
            Vector3D targetVec = targetPosition - userPosition;
            if (targetVec.LengthSQ < (ignoreMoveArea * ignoreMoveArea))
            {
                return;
            }
            else
            {
                if (targetVec.LengthSQ > (runLength * runLength))
                    isRunLenght = true;

                targetVec.Normalize();
            }

            Vector3D baseVector = new Vector3D(0, 0, -1);
            Vector3D verticalVector = baseVector.CrossProduct(targetVec);

            bool flipVector = verticalVector.Y > 0;
            if (flipVector == false)
            {
                baseVector = new Vector3D(0, 0, 1);
            }

            radHeading = baseVector.DotProduct(targetVec) * div2pi;
            radHeadingSmoothReset = true;

            if (flipVector)
            {
                radHeading += pi;
            }

            UserAvatarRotation(0);

            Reference.Viewer.ProtocolManager.AvatarConnection.Forward = true;
            Reference.Viewer.ProtocolManager.AvatarConnection.Run = isRunLenght;
        }

        private bool FindPositionFromMousePosition(Position2D _mousePosition, out Vector3D _targetPosition, out Triangle3D _triangle)
        {
            Vector3D intersection = new Vector3D();
            Triangle3D triangle = new Triangle3D();
            bool find = false;

            Line3D line = Reference.SceneManager.CollisionManager.GetRayFromScreenCoordinates(_mousePosition, Reference.SceneManager.ActiveCamera);

            if (pickSceneNode != null)
            {
                find = Reference.SceneManager.CollisionManager.GetCollisionPoint(
                            line,
                            pickSceneNode.TriangleSelector,
                            out intersection,
                            out triangle
                            );
            }

            _targetPosition = intersection;
            _triangle = triangle;

            return find;
        }

        private bool IsInSim(Vector3D _position)
        {
            const float simMin = 0;
            const float simMax = 255;

            bool flag = false;

            if (simMin < _position.X && _position.X < simMax
                && simMin < _position.Z && _position.Z < simMax)
            {
                flag = true;
            }

            return flag;
        }

        public float RadHeading
        {
            get { return radHeading; }
            set
            {
                radHeading = value;
                UserAvatarRotation(0);
            }
        }

        #endregion

        #region Picker function.
        // Picker
        private void DetectObjectUnderMouse()
        {
            if (Reference.SceneManager != null
                && Reference.Device != null
                && Reference.SceneManager.CollisionManager != null)
            {
                SceneNode irrNodeUnderMouse =
                    Reference.SceneManager.CollisionManager.GetSceneNodeFromScreenCoordinates
                        (Reference.Device.CursorControl.Position,
                        0,
                        false);
                if (irrNodeUnderMouse == null)
                {
                    objectUnderMouse = null;
                }
                else
                {
                    Reference.Viewer.Camera.ResetMouseOffsets();
                    projectedray = Reference.SceneManager.CollisionManager.GetRayFromScreenCoordinates
                         (Reference.Device.CursorControl.Position + Reference.Viewer.CursolOffset,
                         Reference.Viewer.Camera.SNCamera);
                    irrNodeUnderMouse =
                        trianglePickerMapper.GetSceneNodeFromRay(projectedray, 0x0128, true, Reference.Viewer.Camera.SNCamera.Position);

                    bool foundRegionPrimCorrespondingToIrrNodeId = false;
                    VObject vobj = null;
                    if (irrNodeUnderMouse != null
                        && irrNodeUnderMouse.ID != -1)
                    {
                        lock (entities)
                        {
                            foreach (string vobjkey in entities.Keys)
                            {
                                vobj = entities[vobjkey];
                                if (vobj != null
                                    && vobj.Node != null
                                    && vobj.Node.ID == irrNodeUnderMouse.ID)
                                {
                                    objectUnderMouse = vobj;

                                    if (vobj.Prim != null)
                                    {
                                        string firstName = string.Empty;;
                                        string lastName = string.Empty;
                                        if (vobj.Prim.NameValues.Length > 1)
                                        {
                                            firstName = (string)vobj.Prim.NameValues[0].Value;
                                            lastName = (string)vobj.Prim.NameValues[1].Value;
                                        }

                                        StringBuilder sb = new StringBuilder();
                                        sb.Append("{");
                                        sb.Append("\"UUID\":" + "\"" + vobj.Prim.ID.ToString() + "\"");
                                        sb.Append(",");
                                        sb.Append("\"NAME\":");
                                        sb.Append("{");
                                        sb.Append("\"FIRST\":" + "\"" + firstName + "\"");
                                        sb.Append(",");
                                        sb.Append("\"LAST\":" + "\"" + lastName + "\"");
                                        sb.Append("}");
                                        sb.Append("}");
                                        Reference.Viewer.Adapter.CallAvatarPicked(sb.ToString());
                                    }

                                    string text = "AVATAR pick target found!: " + vobj.Prim.ID.ToString() + " " + this.ToString() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name;
                                    Reference.Log.Debug(text);

                                    foundRegionPrimCorrespondingToIrrNodeId = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!foundRegionPrimCorrespondingToIrrNodeId)
                    {
                        objectUnderMouse = null;
                    }
                }
            }
        }

        private void GeneratePickTile()
        {
            AnimatedMesh animMesh = Reference.SceneManager.GetMesh(Util.ApplicationDataDirectory + @"\media\models\tile.x");
            if (animMesh != null)
            {
                Mesh mesh = animMesh.GetMesh(0);
                pickSceneNode = Reference.SceneManager.AddMeshSceneNode(mesh, ParentNode, -1);
                pickSceneNode.TriangleSelector = Reference.SceneManager.CreateTriangleSelector(mesh, pickSceneNode);
                pickSceneNode.TriangleSelector.Drop();
                pickSceneNode.SetMaterialType(MaterialType.TransparentAlphaChannel);
            }
        }

        public bool SetTexture(string _primID, int _materialIndex, string _filename, bool _requestEnable)
        {
            VObject vObject;

            string key = GetEntitiesKeyFromObjectUUID(new UUID(_primID));

            if (string.IsNullOrEmpty(key))
                return false;

            if (!entities.ContainsKey(key))
                return false;

            lock (entities)
                vObject = entities[key];

            if (vObject == null)
                return false;

            if (vObject.MeshNode == null)
                return false;

            if (_materialIndex > (vObject.MeshNode.MaterialCount - 1))
                return false;

            string path = Util.TextureFolder + _filename;
            if (!System.IO.File.Exists(path))
            {
                if (_requestEnable)
                {
                    Reference.Viewer.ProtocolManager.RequestImage(_filename, false, vObject);

                    string filename = System.IO.Path.GetFileNameWithoutExtension(_filename);
                    string extension = System.IO.Path.GetExtension(_filename);
                    SetTextureParam param = new SetTextureParam(_primID, _materialIndex, filename, extension);
                    lock (requestTextureList)
                        requestTextureList.Add(param);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            Texture tex = Reference.VideoDriver.GetTexture(path);
            vObject.MeshNode.GetMaterial(_materialIndex).Texture1 = tex;

            return true;
        }


        void TextureManager_OnTextureLoaded(string texname, string extension, VObject node, UUID AssetID)
        {
            EventQueueParam param = new EventQueueParam(EventQueueType.TextureDownloaded, AssetID.ToString());
            lock (eventQueue)
                eventQueue.Enqueue(param);
        }

        void ProtocolManager_OnTextureFromWebLoaded(string filenameWithoutExtension)
        {
            EventQueueParam param = new EventQueueParam(EventQueueType.TextureDownloaded, filenameWithoutExtension);
            lock (eventQueue)
                eventQueue.Enqueue(param);
        }


        private void EventControlProcess()
        {
            if (eventQueue.Count == 0)
                return;

            EventQueueParam param;
            lock (eventQueue)
                param = eventQueue.Dequeue();

            switch (param.Type)
            {
                case EventQueueType.TextureDownloaded:
                    EventControlProcessTextureDownloaded(param.Option);
                    break;
            }
        }

        private void EventControlProcessTextureDownloaded(object _option)
        {
            if (!(_option is string))
                return;

            string textureUUID = _option as string;

            List<SetTextureParam> del = new List<SetTextureParam>();
            foreach (SetTextureParam param in requestTextureList)
            {
                if (param.TextureUUID == textureUUID)
                {
                    string path = param.TextureUUID + param.TextureExtension;
                    SetTexture(param.PrimID, param.MaterialIndex, path, false);
                    del.Add(param);
                }
            }

            foreach (SetTextureParam param in del)
                lock (requestTextureList)
                    requestTextureList.Remove(param);
        }

        #endregion
    }
}
