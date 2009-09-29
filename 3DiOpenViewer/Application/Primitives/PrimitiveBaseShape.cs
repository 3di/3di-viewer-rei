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
 * from the OpenViewer project (URL http://openviewer.org/):
 * 
 * Copyright (c) Contributors, http://openviewer.org/
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
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using OpenMetaverse;

namespace OpenViewer
{
    public enum ProfileShape : byte
    {
        Circle = 0,
        Square = 1,
        IsometricTriangle = 2,
        EquilateralTriangle = 3,
        RightTriangle = 4,
        HalfCircle = 5
    }

    public enum HollowShape : byte
    {
        Same = 0,
        Circle = 16,
        Square = 32,
        Triangle = 48
    }

    public enum PCodeEnum : byte
    {
        Primitive = 9,
        Avatar = 47
    }

    public enum Extrusion : byte
    {
        Straight = 16,
        Curve1 = 32,
        Curve2 = 48,
        Flexible = 128
    }

    public enum PrimType
    {
        Unknown,
        Box,
        Cylinder,
        Prism,
        Sphere,
        Torus,
        Tube,
        Ring,
        Sculpt
    }

    [Serializable]
    public class PrimitiveBaseShape
    {
        public PrimType Type = PrimType.Box;
        public byte[] ExtraParams;
        private byte[] m_textureEntry;

        public ushort PathBegin;
        public byte PathCurve;
        public ushort PathEnd;
        public sbyte PathRadiusOffset;
        public byte PathRevolutions;
        public byte PathScaleX;
        public byte PathScaleY;
        public byte PathShearX;
        public byte PathShearY;
        public sbyte PathSkew;
        public sbyte PathTaperX;
        public sbyte PathTaperY;
        public sbyte PathTwist;
        public sbyte PathTwistBegin;
        public byte PCode;
        public ushort ProfileBegin;

        public byte ProfileCurve;

        public ushort ProfileEnd;
        public ushort ProfileHollow;
        public Vector3 Scale;
        public byte State;

        static PrimitiveBaseShape()
        {
        }

        public PrimitiveBaseShape()
        {
            PCode = (byte)PCodeEnum.Primitive;
            ExtraParams = new byte[1];
        }

        public byte[] TextureEntry
        {
            get { return m_textureEntry; }
            set { m_textureEntry = value; }
        }

        public ProfileShape ProfileShape
        {
            get { return (ProfileShape)(ProfileCurve & 0x0f); }
            set
            {
                byte oldValueMasked = (byte)(ProfileCurve & 0xf0);
                ProfileCurve = (byte)(oldValueMasked | (byte)value);
            }
        }

        public HollowShape HollowShape
        {
            get { return (HollowShape)(ProfileCurve & 0xf0); }
            set
            {
                byte oldValueMasked = (byte)(ProfileCurve & 0x0f);
                ProfileCurve = (byte)(oldValueMasked | (byte)value);
            }
        }

        public static PrimitiveBaseShape Default
        {
            get
            {
                PrimitiveBaseShape boxShape = CreateBox();
                boxShape.SetScale(0.5f);
                return boxShape;
            }
        }

        public static PrimitiveBaseShape Create()
        {
            PrimitiveBaseShape shape = new PrimitiveBaseShape();
            return shape;
        }

        public static PrimitiveBaseShape CreateBox()
        {
            PrimitiveBaseShape shape = Create();

            shape.PathCurve = (byte)Extrusion.Straight;
            shape.ProfileShape = ProfileShape.Square;
            shape.PathScaleX = 100;
            shape.PathScaleY = 100;

            return shape;
        }

        public static PrimitiveBaseShape CreateCylinder()
        {
            PrimitiveBaseShape shape = Create();

            shape.PathCurve = (byte)Extrusion.Curve1;
            shape.ProfileShape = ProfileShape.Square;

            shape.PathScaleX = 100;
            shape.PathScaleY = 100;

            return shape;
        }

        public void SetScale(float side)
        {
            Scale = new Vector3(side, side, side);
        }

        public void SetHeight(float height)
        {
            Scale.Z = height;
        }

        public void SetRadius(float radius)
        {
            Scale.X = Scale.Y = radius * 2f;
        }

        //void returns need to change of course
        public virtual void GetMesh()
        {
        }

        public PrimitiveBaseShape Copy()
        {
            return (PrimitiveBaseShape)MemberwiseClone();
        }

        public static PrimitiveBaseShape CreateCylinder(float radius, float height)
        {
            PrimitiveBaseShape shape = CreateCylinder();

            shape.SetHeight(height);
            shape.SetRadius(radius);

            return shape;
        }

        public void SetPathRange(Vector3 pathRange)
        {
            // PathBegin = LLObject.PackBeginCut(pathRange.X);
            // PathEnd = LLObject.PackEndCut(pathRange.Y);
        }

        public void SetProfileRange(Vector3 profileRange)
        {
            //  ProfileBegin = LLObject.PackBeginCut(profileRange.X);
            // ProfileEnd = LLObject.PackEndCut(profileRange.Y);
        }
    }
}
