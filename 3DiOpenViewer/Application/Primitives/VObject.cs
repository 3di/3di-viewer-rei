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
using OpenMetaverse;
using IrrlichtNETCP;
using IrrParseLib;
using OpenViewer.Primitives;

namespace OpenViewer
{
    public class VObject : IDisposable
    {
        // Customize animation key
        public static readonly UUID CUSTOMIZE_ANIM_00 = new UUID("{C5829C0B-B82C-4f3d-9475-0826D48E5DB8}");
        public static readonly UUID CUSTOMIZE_ANIM_01 = new UUID("{C006EBC3-A40D-4a7d-B24D-A8323A198DF2}");
        public static readonly UUID CUSTOMIZE_ANIM_02 = new UUID("{046903EF-9358-45e1-BBDF-433CE99D3366}");
        public static readonly UUID CUSTOMIZE_ANIM_03 = new UUID("{290FE528-5128-4451-B9A7-39E761D8F60F}");
        public static readonly UUID CUSTOMIZE_ANIM_04 = new UUID("{43DEFB09-3996-41d2-ACBC-E1E217111396}");
        public static readonly UUID CUSTOMIZE_ANIM_05 = new UUID("{3BF9354B-8D8F-4d74-AB1B-98B867101285}");
        public static readonly UUID CUSTOMIZE_ANIM_06 = new UUID("{7B5960F3-5634-4e97-8225-E5DFCFC78654}");
        public static readonly UUID CUSTOMIZE_ANIM_07 = new UUID("{CF5A0D1D-8CCC-48ba-9D3F-81A4A50A7D11}");
        public static readonly UUID CUSTOMIZE_ANIM_08 = new UUID("{0F56F522-AB3F-44ae-B3A1-9425CF47DF7E}");
        public static readonly UUID CUSTOMIZE_ANIM_09 = new UUID("{2A356685-63C5-4454-9AC9-BAB87E37DA5A}");
        public static readonly UUID CUSTOMIZE_ANIM_10 = new UUID("{84B2F1B0-534C-4c51-82C0-0917BEA3C673}");
        public static readonly UUID CUSTOMIZE_ANIM_11 = new UUID("{20C1A61A-BC42-4202-9A53-9976BE26545E}");
        public static readonly UUID CUSTOMIZE_ANIM_12 = new UUID("{F7B995EA-9F96-4b7c-9C32-15BB50A17F73}");
        public static readonly UUID CUSTOMIZE_ANIM_13 = new UUID("{83A1870B-BAB5-4c2c-BAC9-558C7E22D0A9}");
        public static readonly UUID CUSTOMIZE_ANIM_14 = new UUID("{228B2569-8AD1-42f6-9C86-78DF237F0A86}");
        public static readonly UUID CUSTOMIZE_ANIM_15 = new UUID("{A377FEBB-2732-4e77-952D-A2F7326D6539}");
        public static readonly UUID CUSTOMIZE_ANIM_16 = new UUID("{81563482-4C13-4063-8986-1473D8AD2235}");
        public static readonly UUID CUSTOMIZE_ANIM_17 = new UUID("{2BA7296F-9F84-43fe-B078-C79047CF3085}");
        public static readonly UUID CUSTOMIZE_ANIM_18 = new UUID("{11D022B0-3851-4d77-BB85-08DFBBFC3BD4}");
        public static readonly UUID CUSTOMIZE_ANIM_19 = new UUID("{3DDFE90E-A50A-454d-8B87-5B48AB20E29D}");
        public static readonly UUID CUSTOMIZE_ANIM_20 = new UUID("{E980C815-0CAC-4ec4-9C19-3071085C4804}");


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
