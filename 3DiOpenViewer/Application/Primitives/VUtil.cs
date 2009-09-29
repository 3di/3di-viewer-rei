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
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using OpenMetaverse;
using IrrlichtNETCP;
using OpenSim.Framework;
using Lib3Di;

namespace OpenViewer
{
    public static class VUtil
    {
        public static VObject NewVObject(Primitive pPrim, VObject pOldObj)
        {   
            VObject returnVObject = null;

            if (pOldObj == null)
            {
                returnVObject = new VObject();
                returnVObject.Node = null;
            }
            else
            {
                returnVObject = pOldObj;
            }

            returnVObject.Prim = pPrim;
            returnVObject.Mesh = null;
            
            return returnVObject;
        }

        public static string GetHashId(VObject pObj)
        {
            string returnString = string.Empty;
            
            if (pObj.Prim != null)
            {
                ulong simhandle = pObj.Prim.RegionHandle;
                ulong TESTNEIGHBOR = 1099511628032256;
                if (simhandle == 0)
                        simhandle = TESTNEIGHBOR;

                returnString =simhandle.ToString() + pObj.Prim.LocalID.ToString();
            }
           

            return returnString;
        }

        public static Vector3D CreateMyVectorFromRotaionY(Vector3D _worldFront, float _yRadian)
        {
            IrrlichtNETCP.Matrix4 mat = IrrlichtNETCP.Matrix4.Identity;
            mat.Translation = _worldFront;
            mat.RotationRadian = new Vector3D(0, _yRadian, 0);

            return mat.Translation;
        }

        public static Vector3D WorldToLocalVector(Vector3D _worldVector, Vector3D _rotation)
        {
            IrrlichtNETCP.Matrix4 transTarget = IrrlichtNETCP.Matrix4.Identity;
            transTarget.Translation = _worldVector;

            IrrlichtNETCP.Matrix4 calcMat = IrrlichtNETCP.Matrix4.Identity;
            calcMat.RotationDegrees = _rotation;

            IrrlichtNETCP.Matrix4 inv = IrrlichtNETCP.Matrix4.Identity;
            calcMat.GetInverse(out inv);

            transTarget = inv * transTarget;

            return transTarget.Translation;
        }

        #region IrrFile utility

        public static string assetServerUri;
        public static UUID authToken;


        #endregion // IrrFile utility

        #region Debug function.

        //--------------------------------------------------------
        // Debug function.
        //--------------------------------------------------------
        private static readonly log4net.ILog m_log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
#if DEBUG
        public static List<string> LogFilterList = new List<string>();
#endif

        public static void LogConsole(string _classInfo, string _text)
        {
            string message = "[" + _classInfo + "]:" + _text;
            m_log.Debug(message);
#if DEBUG
            if (LogFilterList.Contains(_classInfo) == false)
            {
                System.Diagnostics.Debug.WriteLine(message);
            }
#endif
        }
        #endregion
    }
}
