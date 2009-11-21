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
using IrrlichtNETCP;

namespace OpenViewer
{
    public class TrianglePickerMapper
    {
        // The hashcode for the triangle selector is (int)IntPtr based on it's pointer location from Irrlicht
        private List<TriangleSelector> trilist = new List<TriangleSelector>();

        // Don't make this public or you'll have race conditions.
        private Dictionary<IntPtr, SceneNode> triLookup = new Dictionary<IntPtr, SceneNode>();

        private SceneCollisionManager scm = null;

        public TrianglePickerMapper(SceneCollisionManager pscm)
        {
            scm = pscm;
        }

        public void AddTriangleSelector(TriangleSelector trisel, SceneNode node)
        {
            lock (trisel)
            {
                trilist.Add(trisel);

                if (triLookup.ContainsKey(trisel.Raw))
                    triLookup[trisel.Raw] = node;
                else 
                    triLookup.Add(trisel.Raw, node);
            }
        }

        public SceneNode GetSceneNodeFromRay(Line3D ray, int bitMask, bool noDebug, Vector3D campos)
        {
            SceneNode returnobj = null;
            Vector3D collisionpoint = new Vector3D(0, 0, 0);
            Triangle3D tri = new Triangle3D(0, 0, 0, 0, 0, 0, 0, 0, 0);
            Vector3D closestpoint = new Vector3D(999, 999, 9999);
            List<TriangleSelector> removeList = new List<TriangleSelector>();
            lock (trilist)
            {
                foreach (TriangleSelector trisel in trilist)
                {
                    if (trisel == null)
                    {
                        removeList.Add(trisel);
                        continue;
                    }
                    if (trisel.Raw == IntPtr.Zero)
                    {
                        removeList.Add(trisel);
                        continue;
                    }
                    try
                    {
                        if (scm.GetCollisionPoint(ray, trisel, out collisionpoint, out tri))
                        {
                            if (campos.DistanceFrom(collisionpoint) < campos.DistanceFrom(closestpoint))
                            {
                                closestpoint = collisionpoint;

                                if (triLookup.ContainsKey(trisel.Raw))
                                {
                                    SceneNode sreturnobj = triLookup[trisel.Raw];
                                    if (!(sreturnobj is TerrainSceneNode))
                                        returnobj = sreturnobj;
                                }

                            }

                        }
                    }
                    catch (AccessViolationException)
                    {
                        VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "TrianglePickerMapper::GetSceneNodeFromRay");
                        removeList.Add(trisel);
                        continue;
                    }
                    catch (System.Runtime.InteropServices.SEHException)
                    {
                        removeList.Add(trisel);
                        continue;
                    }
                }
                foreach (TriangleSelector trisel2 in removeList)
                {
                    trilist.Remove(trisel2);
                }

            }
            return returnobj;
        }

        public void RemTriangleSelector(TriangleSelector trisel)
        {
            if (trisel != null)
            {
                lock (trisel)
                {
                    trilist.Remove(trisel);
                    if (triLookup.ContainsKey(trisel.Raw))
                        triLookup.Remove(trisel.Raw);
                }
            }
        }
    }
}
