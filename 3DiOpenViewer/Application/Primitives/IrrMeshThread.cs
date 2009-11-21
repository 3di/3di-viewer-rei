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

using System.Collections.Generic;
using OpenMetaverse;

namespace OpenViewer
{
    class IrrMeshThread : BaseComponent
    {
        private VObject obj;
        private string workDirectory = string.Empty;

        public IrrMeshThread(Viewer viewer, VObject _obj, string _directory)
            : base(viewer, -1)
        {
            obj = _obj;
            workDirectory = _directory;
        }
        
        public object Requesting(object arg)
        {
            OpenViewer.Managers.IrrManager.IrrWorkItem workitem = (OpenViewer.Managers.IrrManager.IrrWorkItem)arg; 
            string irrfileName = workDirectory + "/" + obj.RequestIrrfileUUID.ToString() + ".irr";
            if (System.IO.File.Exists(irrfileName) == false)
            {
                string res = Reference.Viewer.IrrManager.IrrFileCreateCache(irrfileName, workDirectory);
                if (res == string.Empty)
                {
                }
                else
                {
                    return workitem; // retry
                }
            }

            using (System.IO.StreamReader reader = new System.IO.StreamReader(irrfileName))
            {
                // Parse irr file.
                IrrParseLib.IrrParser paser = new IrrParseLib.IrrParser(reader.BaseStream);

                if (paser != null)
                {
                    if (paser.Datas.Count > 0)
                    {
                        //---------------------------------------------
                        // [YK:NEXT]
                        Reference.Viewer.IrrManager.IrrFileTCPRequestToAssetServer_toplevel(paser.Datas[0], Reference.Viewer.ProtocolManager.AvatarConnection, workDirectory, obj.requestTexturesDirectlyFromAssetServerWithoutJ2KConversion);
                        Reference.Viewer.IrrManager.AddNewObject(obj.RequestIrrfileUUID, paser.Datas[0]);
                        //
                        //---------------------------------------------
                    }
                }

            }

            List<VObject> requestors = Reference.Viewer.IrrManager.CloseRequest(obj.RequestIrrfileUUID);

            if (!(obj.Prim is Avatar))
            {
                // mesh prim
                if (requestors != null)
                {
                    foreach (VObject v in requestors)
                    {
                        if (v == null)
                        {
                            continue;
                        }

                        v._3DiIrrfileUUID = obj.RequestIrrfileUUID; // ensure the mesh loading code in EntityManager has the mesh UUID
                        v.NeedToReload3DiMesh = true;
                        v.UpdateFullYN = true;

                        Reference.Viewer.EntityManager.AddObject(v);
                    }
                }
            }
            if (requestors != null)
            {
                foreach (VObject v in requestors)
                {
                    if (v == null)
                    {
                        continue;
                    }

                    v.Requesting = false;
                    Reference.Viewer.AvatarManager.UpdateObject(v.Prim.ID);
                }
            }
            return null; // OK
        }
    }
}
