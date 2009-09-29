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
using OpenMetaverse;

namespace OpenViewer
{
    public class MeshFactory
    {
        private Dictionary<string, IrrlichtNETCP.Mesh> StoredMesh = new Dictionary<string, Mesh>();
        private Dictionary<string, IrrlichtNETCP.Mesh> IdenticalMesh = new Dictionary<string, Mesh>();
        IrrlichtDevice device;
        private MeshManipulator mm = null;
        private List<IntPtr> killed = new List<IntPtr>();
        
        public MeshFactory(MeshManipulator pmm, IrrlichtDevice pdevice)
        {
            mm = pmm;
            device = pdevice;
        }

        ~MeshFactory()
        {
            foreach (Mesh mesh in StoredMesh.Values)
                mesh.Dispose();
        }

        public bool GetMeshInstance(Primitive prim, out Mesh objMesh)
        {
            Primitive.ConstructionData primData = prim.PrimData;
            int sides = 4;
            int hollowsides = 4;

            float profileBegin = primData.ProfileBegin;
            float profileEnd = primData.ProfileEnd;
            

            if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.Circle)
                sides = 24;
            else if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.EqualTriangle)
                sides = 3;
            else if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.HalfCircle)
            { // half circle, prim is a sphere
                sides = 24;
                profileBegin = 0.5f * profileBegin + 0.5f;
                profileEnd = 0.5f * profileEnd + 0.5f;
            }

            if ((HoleType)primData.ProfileHole == HoleType.Same)
                hollowsides = sides;
            else if ((HoleType)primData.ProfileHole == HoleType.Circle)
                hollowsides = 24;
            else if ((HoleType)primData.ProfileHole == HoleType.Triangle)
                hollowsides = 3;
            objMesh = null;

            string storedmeshcode = (sides.ToString() + profileBegin.ToString() + profileEnd.ToString() + ((float)primData.ProfileHollow).ToString() + hollowsides.ToString() + primData.PathScaleX.ToString() + primData.PathScaleY.ToString() + primData.PathBegin.ToString() +
                primData.PathEnd.ToString() + primData.PathShearX.ToString() + primData.PathShearY.ToString() +
                primData.PathRadiusOffset.ToString() + primData.PathRevolutions.ToString() + primData.PathSkew.ToString() +
                ((int)primData.PathCurve).ToString() + primData.PathScaleX.ToString() + primData.PathScaleY.ToString() +
                primData.PathTwistBegin.ToString() + primData.PathTwist.ToString());

            
            
            bool identicalcandidate = true;
            if (prim.Textures != null)
            {
                foreach (Primitive.TextureEntryFace face in prim.Textures.FaceTextures)
                {
                    if (face != null)
                        identicalcandidate = false;
                }
            }

            StringBuilder sbIdenticalMesh = new StringBuilder();
            sbIdenticalMesh.Append(storedmeshcode);

            // this test is short circuit dependent - don't change the order 
            if (prim.Textures != null && prim.Textures.DefaultTexture != null)
                {
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.TextureID);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.Bump);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.Fullbright);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.Glow);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.MediaFlags);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.OffsetU);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.OffsetV);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.RepeatU);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.RepeatV);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.RGBA.ToRGBString());
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.Rotation);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.Shiny);
                    sbIdenticalMesh.Append(prim.Textures.DefaultTexture.TexMapType.ToString());
                }

            string identicalmeshcode = sbIdenticalMesh.ToString();


            if (identicalcandidate)
            {
                lock (IdenticalMesh)
                {
                    if (IdenticalMesh.ContainsKey(identicalmeshcode))
                        objMesh = IdenticalMesh[identicalmeshcode];
                    
                }
                if (objMesh == null)
                {
                    objMesh = PrimMesherG.PrimitiveToIrrMesh(prim, LevelOfDetail.High);
                }
                lock (IdenticalMesh)
                {
                    if (!IdenticalMesh.ContainsKey(identicalmeshcode))
                        IdenticalMesh.Add(identicalmeshcode, objMesh);
                }

                lock (StoredMesh)
                {
                    if (!StoredMesh.ContainsKey(storedmeshcode))
                        StoredMesh.Add(storedmeshcode, objMesh);
                }
                return false;
            }

            
            lock (StoredMesh)
            {
                if (StoredMesh.ContainsKey(storedmeshcode))
                {
                    objMesh = StoredMesh[storedmeshcode];
                }
            }

            if (objMesh == null)
            {
                objMesh = PrimMesherG.PrimitiveToIrrMesh(prim, LevelOfDetail.High);
                lock (StoredMesh)
                {
                    if (!StoredMesh.ContainsKey(storedmeshcode))
                    {
                        StoredMesh.Add(storedmeshcode, objMesh);
                    }
                }
            }

            // outside lock.
            if (objMesh != null)
            {
                objMesh = mm.CreateMeshCopy(objMesh);
                return true;
            }

            return false;
        }

        public Mesh GetSculptMesh(UUID assetid, TextureExtended sculpttex, SculptType stype, Primitive prim)
        {
            Mesh result = null;


            lock (StoredMesh)
            {
                if (StoredMesh.ContainsKey(assetid.ToString()))
                {
                    result = StoredMesh[assetid.ToString()];
                    return result;
                }
            }
            if (result == null)
            {
                System.Drawing.Bitmap bm = sculpttex.DOTNETImage;
                result = PrimMesherG.SculptIrrMesh(bm, stype);
                if (!killed.Contains(sculpttex.Raw))
                {
                    try
                    {
                        killed.Add(sculpttex.Raw);
                        device.VideoDriver.RemoveTexture(sculpttex);
                    }
                    catch (AccessViolationException)
                    {
                        VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "MeshFactory::GetSculptMesh");
                        System.Console.WriteLine("Unable to remove a sculpt texture from the video driver!");
                    }
                }
                bm.Dispose();
                if (result != null)
                {
                    lock (StoredMesh)
                    {
                        if (!StoredMesh.ContainsKey(assetid.ToString()))
                        {
                            StoredMesh.Add(assetid.ToString(), result);
                        }
                    }
                }
            }

            if (result != null)
            {
                return result;
            }

            return null;
            
        }

        public int UniqueObjects
        {
            get { return StoredMesh.Count; }
        }
    }
}
