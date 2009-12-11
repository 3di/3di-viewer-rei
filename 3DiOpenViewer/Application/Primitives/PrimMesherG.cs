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
using OpenMetaverse;
using PrimMesher;
using IrrlichtNETCP;
using log4net;

namespace OpenViewer
{
    public enum LevelOfDetail
    {
        Low,
        Medium,
        High
    }
    public static class PrimMesherG
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static Vector2D convVect2d(UVCoord uv)
        {
            return new Vector2D(uv.U, uv.V);
        }

        public static Vector3D convVect3d(Coord c)
        {// translate coordinates XYZ to XZY
            return new Vector3D(c.X, c.Z, c.Y);
        }

        public static Vector3D convNormal(Coord c)
        {// translate coordinates XYZ to XZY
            return new Vector3D(c.X, c.Z, c.Y);
        }

        private static Mesh FacesToIrrMesh(List<ViewerFace> viewerFaces, int numPrimFaces)
        {
            Color color = new Color(255, 255, 255, 255);

            Mesh mesh;
            try
            {
                mesh = new Mesh();
            }
            catch (IndexOutOfRangeException)
            {
                return null;
            }
            int numViewerFaces = viewerFaces.Count;

            MeshBuffer[] mb = new MeshBuffer[numPrimFaces];

            for (int i = 0; i < mb.Length; i++)
                mb[i] = new MeshBuffer(VertexType.Standard);

            try
            {
                uint[] index = new uint[mb.Length];

                for (int i = 0; i < index.Length; i++)
                    index[i] = 0;

                for (uint i = 0; i < numViewerFaces; i++)
                {
                    ViewerFace vf = viewerFaces[(int)i];

                    try
                    {
                        mb[vf.primFaceNumber].SetVertex(index[vf.primFaceNumber], new Vertex3D(convVect3d(vf.v1), convNormal(vf.n1), color, convVect2d(vf.uv1)));
                        mb[vf.primFaceNumber].SetVertex(index[vf.primFaceNumber] + 1, new Vertex3D(convVect3d(vf.v2), convNormal(vf.n2), color, convVect2d(vf.uv2)));
                        mb[vf.primFaceNumber].SetVertex(index[vf.primFaceNumber] + 2, new Vertex3D(convVect3d(vf.v3), convNormal(vf.n3), color, convVect2d(vf.uv3)));

                    }
                    catch (OutOfMemoryException)
                    {
                        return null;
                    }
                    catch (IndexOutOfRangeException)
                    {
                        return null;
                    }

                    mb[vf.primFaceNumber].SetIndex(index[vf.primFaceNumber], (ushort)index[vf.primFaceNumber]);
                    mb[vf.primFaceNumber].SetIndex(index[vf.primFaceNumber] + 1, (ushort)(index[vf.primFaceNumber] + 2));
                    mb[vf.primFaceNumber].SetIndex(index[vf.primFaceNumber] + 2, (ushort)(index[vf.primFaceNumber] + 1));

                    index[vf.primFaceNumber] += 3;
                }

                for (int i = 0; i < mb.Length; i++)
                {
                    mesh.AddMeshBuffer(mb[i]);
                }

                Box3D box = new Box3D(0, 0, 0, 0, 0, 0);
                for (int i = 0; i < mesh.MeshBufferCount; i++)
                {
                    mesh.GetMeshBuffer(i).RecalculateBoundingBox();
                    box.AddInternalBox(mesh.GetMeshBuffer(i).BoundingBox);
                }
                mesh.BoundingBox = box;
                // don't dispose here
                //mb.Dispose();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole("[ACCESSVIOLATION]", "PrimMesherG::FacesToIrrMesh");
                mesh = null;
            }

            return mesh;
        }

        // experimental - build sculpt mesh using indexed access to vertex, normal, and UV lists
        private static Mesh SculptMeshToIrrMesh(SculptMesh sculptMesh)
        {
            Color color = new Color(255, 255, 255, 255);

            Mesh mesh = new Mesh();

            int numFaces = sculptMesh.faces.Count;

            MeshBuffer mb = new MeshBuffer(VertexType.Standard);

            int numVerts = sculptMesh.coords.Count;

            try
            {
                for (int i = 0; i < numVerts; i++)
                    mb.SetVertex((uint)i, new Vertex3D(convVect3d(sculptMesh.coords[i]), convNormal(sculptMesh.normals[i]), color, convVect2d(sculptMesh.uvs[i])));

                ushort index = 0;
                foreach (Face face in sculptMesh.faces)
                {
                    mb.SetIndex(index++, (ushort)face.v1);
                    mb.SetIndex(index++, (ushort)face.v3);
                    mb.SetIndex(index++, (ushort)face.v2);
                }

                mesh.AddMeshBuffer(mb);

                // don't dispose here
                //mb.Dispose();
            }

            catch (AccessViolationException)
            {
                VUtil.LogConsole("[ACCESSVIOLATION]", "PrimMesherG::SCultMeshToIrrMesh");
                m_log.Error("ACCESSVIOLATION");
                mesh = null;
            }

            return mesh;
        }
      
        public static Mesh PrimitiveToIrrMesh(Primitive prim, LevelOfDetail detail)
        {
            Primitive.ConstructionData primData = prim.PrimData;
            int sides = 4;
            int hollowsides = 4;

            float profileBegin = primData.ProfileBegin;
            float profileEnd = primData.ProfileEnd;
            bool isSphere = false;

            if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.Circle)
            {
                switch (detail)
                {
                    case LevelOfDetail.Low:
                        sides = 6;
                        break;
                    case LevelOfDetail.Medium:
                        sides = 12;
                        break;
                    default:
                        sides = 24;
                        break;
                }
            }
            else if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.EqualTriangle)
            {
                sides = 3;
            }
            // half circle, prim is a sphere
            else if ((ProfileCurve)(primData.profileCurve & 0x07) == ProfileCurve.HalfCircle)
            {
                isSphere = true;
                switch (detail)
                {
                    case LevelOfDetail.Low:
                        sides = 6;
                        break;
                    case LevelOfDetail.Medium:
                        sides = 12;
                        break;
                    default:
                        sides = 24;
                        break;
                }
                profileBegin = 0.5f * profileBegin + 0.5f;
                profileEnd = 0.5f * profileEnd + 0.5f;
            }

            if ((HoleType)primData.ProfileHole == HoleType.Same)
            {
                hollowsides = sides;
            }
            else if ((HoleType)primData.ProfileHole == HoleType.Circle)
            {
                switch (detail)
                {
                    case LevelOfDetail.Low:
                        hollowsides = 6;
                        break;
                    case LevelOfDetail.Medium:
                        hollowsides = 12;
                        break;
                    default:
                        hollowsides = 24;
                        break;
                }

            }
            else if ((HoleType)primData.ProfileHole == HoleType.Triangle)
                hollowsides = 3;

            PrimMesh newPrim = new PrimMesh(sides, profileBegin, profileEnd, (float)primData.ProfileHollow, hollowsides);
            newPrim.viewerMode = true;
            newPrim.holeSizeX = primData.PathScaleX;
            newPrim.holeSizeY = primData.PathScaleY;
            newPrim.pathCutBegin = primData.PathBegin;
            newPrim.pathCutEnd = primData.PathEnd;
            newPrim.topShearX = primData.PathShearX;
            newPrim.topShearY = primData.PathShearY;
            newPrim.radius = primData.PathRadiusOffset;
            newPrim.revolutions = primData.PathRevolutions;
            newPrim.skew = primData.PathSkew;

            switch (detail)
            {
                case LevelOfDetail.Low:
                    newPrim.stepsPerRevolution = 6;
                    break;
                case LevelOfDetail.Medium:
                    newPrim.stepsPerRevolution = 12;
                    break;
                default:
                    newPrim.stepsPerRevolution = 24;
                    break;
            }

            

            if (primData.PathCurve == PathCurve.Line)
            {
                newPrim.taperX = 1.0f - primData.PathScaleX;
                newPrim.taperY = 1.0f - primData.PathScaleY;
                newPrim.twistBegin = (int)(180 * primData.PathTwistBegin);
                newPrim.twistEnd = (int)(180 * primData.PathTwist);
                newPrim.ExtrudeLinear();
            }
            else if (primData.PathCurve == PathCurve.Flexible)
            {
                newPrim.taperX = 1.0f - primData.PathScaleX;
                newPrim.taperY = 1.0f - primData.PathScaleY;
                newPrim.twistBegin = (int)(180 * primData.PathTwistBegin);
                newPrim.twistEnd = (int)(180 * primData.PathTwist);
                newPrim.ExtrudeLinear();
            }
            else
            {
                newPrim.taperX = primData.PathTaperX;
                newPrim.taperY = primData.PathTaperY;
                newPrim.twistBegin = (int)(360 * primData.PathTwistBegin);
                newPrim.twistEnd = (int)(360 * primData.PathTwist);
                newPrim.ExtrudeCircular();
            }

            int numViewerFaces = newPrim.viewerFaces.Count;

            for (uint i = 0; i < numViewerFaces; i++)
            {
                ViewerFace vf = newPrim.viewerFaces[(int)i];

                if (isSphere)
                {
                    vf.uv1.U = (vf.uv1.U - 0.5f) * 2.0f;
                    vf.uv2.U = (vf.uv2.U - 0.5f) * 2.0f;
                    vf.uv3.U = (vf.uv3.U - 0.5f) * 2.0f;
                }
            }

            return FacesToIrrMesh(newPrim.viewerFaces, newPrim.numPrimFaces);
        }

        public static Mesh SculptIrrMesh(System.Drawing.Bitmap bitmap, OpenMetaverse.SculptType omSculptType)
        {
            switch (omSculptType)
            {
                case OpenMetaverse.SculptType.Cylinder:
                    return SculptIrrMesh(bitmap, SculptMesh.SculptType.cylinder);
                case OpenMetaverse.SculptType.Plane:
                    return SculptIrrMesh(bitmap, SculptMesh.SculptType.plane);
                case OpenMetaverse.SculptType.Sphere:
                    return SculptIrrMesh(bitmap, SculptMesh.SculptType.sphere);
                case OpenMetaverse.SculptType.Torus:
                    return SculptIrrMesh(bitmap, SculptMesh.SculptType.torus);
                default:
                    return SculptIrrMesh(bitmap, SculptMesh.SculptType.plane);
            }
        }

        public static Mesh SculptIrrMesh(System.Drawing.Bitmap bitmap)
        {
            return SculptIrrMesh(bitmap, PrimMesher.SculptMesh.SculptType.plane);
        }

        public static Mesh SculptIrrMesh(System.Drawing.Bitmap bitmap, PrimMesher.SculptMesh.SculptType sculptType)
        {
            SculptMesh newSculpty = new SculptMesh(bitmap, sculptType, 32, true);

            //return FacesToIrrMesh(newSculpty.viewerFaces, 1);

            // experimental - build sculpt mesh using vertex, normal, and coord lists
            return SculptMeshToIrrMesh(newSculpty);
        }

        public static Mesh SculptIrrMesh(float[,] zMap, float minX, float maxX, float minY, float maxY)
        {
            return SculptMeshToIrrMesh(new SculptMesh(zMap, minX, maxX, minY, maxY, true));
        }
    }
}
