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
using OpenMetaverse;
using IrrlichtNETCP;
using IrrParseLib;
using OpenViewer.Primitives;

namespace OpenViewer
{
    public class VObject : IDisposable
    {
        private enum UpdateType
        {
            AnimationSpeed,
            SetFrameLoop,

            Length,
        }

        private Animation animationCurrent = Animation.Empty;
        private Animation animationNext = Animation.Empty;
        private bool[] updateFlags = new bool[(int)UpdateType.Length];
        private KeyframeSet frameSet = new KeyframeSet(string.Empty, 0, 0, 0);
        private UUID requestIrrrileUUID;
        private bool requesting = false;
        private bool updateFullYN = false;
        private bool isGhost = false;

        public IrrMeshParam BaseParam = new IrrMeshParam(string.Empty);
        public Vector3 ParentPosition = new Vector3();
        public Vector3D TargetPosition = new Vector3D();
        public Vector3D TargetRotation = new Vector3D();
        public Vector3D Velocity = new Vector3D();
        public Dictionary<string, KeyframeSet> FrameSetList = new Dictionary<string, KeyframeSet>();

        public SceneNode Node; // Reference to empty scene node
        public SceneNode VoiceNode; // Reference to voice effect node
        public SceneNode PickNode; // Reference to pick object node
        public SceneNode NodeStaticMesh;
        public AnimatedMeshSceneNode MeshNode; // Reference to graphics node
        public int VoiceLevel = 0;
        public Primitive Prim; // Avatar Extend the primative type
        public Mesh Mesh; // Reference to graphics mesh
        public bool NeedToReload3DiMesh = false;
        public bool SmoothingReset = false;
        public UUID CurrentAnimationUUID;
        public string AnimationCurrentName;
        public UUID _3DiIrrfileUUID;
        public IrrDatas IrrData; // reference to parsed .irr data. We need this when we receive a textureComplete callback, in order to figure out which texture slot references the just-arrived texture.
        public bool requestTexturesDirectlyFromAssetServerWithoutJ2KConversion;

        /// <summary>
        /// In case prim is Avatar, gets or sets whether this VObject represents a child agent.
        /// </summary>
        public bool IsChildAgent = false;

        #region Property
        public Animation AnimationCurrent { get { return animationCurrent; } }
        public Animation AnimationNext { get { return animationNext; } }
        public bool UpdateFullYN { get { return updateFullYN; } set { updateFullYN = value; } }
        public bool IsGhost { get { return isGhost; } set { isGhost = value; } }
        public bool Requesting { get { return requesting; } set { requesting = value; } }
        public UUID RequestIrrfileUUID { get { return requestIrrrileUUID; } set { requestIrrrileUUID = value; } }
        public int AnimationSpeed
        {
            get { return frameSet.AnimationSpeed; }
            set
            {
                frameSet.AnimationSpeed = value;
                updateFlags[(int)UpdateType.AnimationSpeed] = true;

                MeshNode.AnimationSpeed = frameSet.AnimationSpeed;
            }
        }
        #endregion

        public  VObject()
        {
        }
        
        public void SyncToChilds()
        {
            if (MeshNode == null)
                return;

            SyncToChilds(MeshNode);
        }

        private void SyncToChilds(SceneNode _parent)
        {
            if (_parent.Children.Length == 0)
                return;

            foreach (SceneNode child in _parent.Children)
            {
                if (child is AnimatedMeshSceneNode)
                {
                    AnimatedMeshSceneNode animeNode = (AnimatedMeshSceneNode)child;

                    if (updateFlags[(int)UpdateType.AnimationSpeed])
                        animeNode.AnimationSpeed = frameSet.AnimationSpeed;
                    if (updateFlags[(int)UpdateType.SetFrameLoop])
                        animeNode.SetFrameLoop(frameSet.StartFrame, frameSet.EndFrame);

                    if (child.Children.Length > 0)
                        SyncToChilds(child);
                }
            }

            // Reset flags.
            for (int i = 0; i < updateFlags.Length; i++)
                updateFlags[i] = false;
        }

        public void SetNextAnimation(string _key, bool _loop)
        {
            animationNext.Key = _key;
            animationNext.Loop = _loop;
        }

        public void SetAnimationMode(JointUpdateOnRenderMode _mode)
        {
            MeshNode.JointMode = _mode;
        }

        public void AnimationEndHandler(AnimatedMeshSceneNode _node)
        {
            try
            {
                if (FrameSetList.ContainsKey(animationNext.Key))
                {
                    int startFrame = FrameSetList[animationNext.Key].StartFrame;
                    int endFrame = FrameSetList[animationNext.Key].EndFrame;
                    int animFramesPerSecond = FrameSetList[animationNext.Key].AnimationSpeed;

                    AnimationSpeed = animFramesPerSecond;
                    SetAnimation(animationNext.Key, startFrame, endFrame, animationNext.Loop);
                    SyncToChilds(_node);
                }
            }
            finally
            {
                animationNext = Animation.Empty;
            }
        }

        public void SetAnimation(string _animationName, int _startFrame, int _endFrame, bool _loop)
        {
            animationCurrent.Key = _animationName;
            animationCurrent.Loop = _loop;

            frameSet.StartFrame = _startFrame;
            frameSet.EndFrame = _endFrame;
            updateFlags[(int)UpdateType.SetFrameLoop] = _loop;

            MeshNode.SetFrameLoop(frameSet.StartFrame, frameSet.EndFrame);
            MeshNode.LoopMode = _loop;
        }

        public void SetAnimationLoop(bool _loop)
        {
            if (MeshNode != null)
                MeshNode.LoopMode = _loop;
        }

        public void Dispose()
        {
            if (MeshNode != null)
            {
                MeshNode.Dispose();
            }
        }
    }
}
