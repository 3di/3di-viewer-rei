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
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using OpenMetaverse;
using IrrlichtNETCP;
using IrrParseLib;
using Nwc.XmlRpc;
using System.Xml.Serialization;
using OpenSim.Framework;
using System.IO;
using Lib3Di;
using Amib.Threading;

namespace OpenViewer.Managers
{
    public class IrrManager : BaseComponent
    {
        private Dictionary<UUID, IrrDatas> dataList = new Dictionary<UUID, IrrDatas>();
        private Dictionary<UUID, List<VObject>> requestingList = new Dictionary<UUID,List<VObject>>();
        private UUID lastUUID = new UUID();
        private string workDirectory = string.Empty;
        public SmartThreadPool requestingThreadPool;
        private int asset_max_threads;
        private int asset_fetch_retry;
        private List<IrrWorkItem> irrWorkItems;
        private Thread irrWorkItemMaintainerThread;

        public IrrManager(Viewer viewer, int _id)
            : base(viewer, _id)
        {
            ChangeWorkDirectory(Util.ModelFolder);
            asset_max_threads = Reference.Viewer.Config.Source.Configs["Startup"].GetInt("asset_max_threads", 1);
            asset_fetch_retry = Reference.Viewer.Config.Source.Configs["Startup"].GetInt("asset_fetch_retry", 2);

            requestingThreadPool = new SmartThreadPool(120 * 1000, asset_max_threads);
            requestingThreadPool.Start();
        }

        public class IrrWorkItem
        {
            public string name;
            public WorkItemCallback callback;
            public object arg;
            public int retry;

            public IrrWorkItem(string name, WorkItemCallback callback, object arg)
            {
                this.name = name;
                this.callback = callback;
                this.arg = arg;
                this.retry = 0;
            }
        }

        public void IrrWorkItemQueue(IrrWorkItem item)
        {
            lock (irrWorkItems)
            {
                irrWorkItems.Add(item);
            }
        }

        private void PostIrrWorkItemHandler(IWorkItemResult wir)
        {
            if (wir.IsCanceled)
            {
                return;
            }

            object result = wir.GetResult();

            if (result != null)
            {
                IrrWorkItem item = (IrrWorkItem)result;
                lock (irrWorkItems)
                {
                    irrWorkItems.Add(item);
                }
            }
        }

        private void IrrWorkItemMaintainer()
        {
            while (true)
            {
                List<IrrWorkItem> items;

                items = null;
                lock (irrWorkItems)
                {
                    if (irrWorkItems.Count > 0)
                    {
                        items = new List<IrrWorkItem>(irrWorkItems);
                    }
                }
                if (items != null)
                {
                    foreach (IrrWorkItem item in items)
                    {
                        if (item.retry <= asset_fetch_retry)
                        {
                            WorkItemCallback callback = new WorkItemCallback(item.callback);
                            PostExecuteWorkItemCallback posthandler = new PostExecuteWorkItemCallback(PostIrrWorkItemHandler);
                            requestingThreadPool.QueueWorkItem(callback, item, posthandler);
                            item.retry++;
                        }
                        lock (irrWorkItems)
                        {
                            irrWorkItems.Remove(item);
                        }
                    }
                }
                System.Threading.Thread.Sleep(1000);
            }
        }
        
        public override void Initialize()
        {
            base.Initialize();

            DeleteTempMeshFile();

            //requestingThreadPool = new SmartThreadPool(60 * 1000, 10); 
            //requestingThreadPool.Start();
            irrWorkItems = new List<IrrWorkItem>();
            irrWorkItemMaintainerThread = new Thread(new ThreadStart(IrrWorkItemMaintainer));
            irrWorkItemMaintainerThread.Start();
        }

        public override void Cleanup()
        {
            if (irrWorkItemMaintainerThread != null)
            {
                irrWorkItemMaintainerThread.Abort();
                irrWorkItemMaintainerThread = null;
            }
            if (irrWorkItems != null)
            {
                irrWorkItems = null;
            }
            if (requestingThreadPool != null)
            {
                //requestingThreadPool.Dispose();
                //requestingThreadPool = null;
                requestingThreadPool.Cancel();
                requestingThreadPool.WaitForIdle();
            }

            lock (dataList)
            {
                dataList.Clear();
                lastUUID = new UUID();
            }
            lock (requestingList)
            {
                requestingList.Clear();
            }
            base.Cleanup();
        }

        public void AddNewObject(UUID _uuid, IrrDatas _data)
        {
            lock (dataList)
            {
                if (dataList.ContainsKey(_uuid))
                {
                    // we have a duplicate key - do nothing. what this means is that
                    // we fetched this asset twice, because e.g. two prims had the same
                    // mesh, each prim checked the dataList and saw that the mesh wasn't downloaded,
                    // and each prim started a download, then one prim finished its download and
                    // inserted it into the list, then later the 2nd prim finished and tried to
                    // insert but it was too late.
                    //
                    // solutions that we can't use:
                    //
                    // - we shouldn't lock the entire list during fetching an asset because that
                    //   would block fetching of other incoming asset requests that may be coming from
                    //   other mesh prims that are loading in from libomv.
                    //
                    // - we shouldn't do an atomic check-and-insert into dataList because the
                    //   current meaning of an entry in dataList is "this (.irr) asset and all 
                    //   its related data has been *completely downloaded* and is *ready to use*".
                    //   if we did an atomic check-and-insert we would be inserting into this list
                    //   before the data is actually downloaded and ready to use.
                    //
                    // this means that the solution is to maintain two separate lists:
                    // 1. inProgressIrrfileRequests
                    // 2. completelyDownloadedIrrfiles
                    //
                    // 1 is atomically check-and-updated at the time the mesh prim arrives.
                    // this prevents duplicate requests. 2 is updated after the download is
                    // complete, and the corresponding entry in 1 is removed.
                    //
                    // during the check-and-update of 1, if we find that the request already
                    // exists (from a different prim), but the asset isn't yet downloaded
                    // completely, we have to wait (e.g. for an event, or later poll a queue).
                    //
                    // currently the solution of 2 separate lists is not yet implemented.
                    // the above workaround should prevent errors but may cause wasted bandwidth
                    // in the case of multiple mesh prims arriving at the same time that use
                    // the same mesh and all issue the same download request.
                    //
                }
                else
                {
                    dataList.Add(_uuid, _data);
                }

                lastUUID = _uuid;
            }
        }

        public IrrDatas GetObject(UUID _uuid, bool checkTextures)
        {
            if (!dataList.ContainsKey(_uuid))
                return null;

            IrrParseLib.IrrDatas _datas;
            lock (dataList)
                _datas = dataList[_uuid];

            if (!CheckAsset(_datas, checkTextures))
                return null;

            return _datas;
        }

        public void RequestObject(VObject _obj)
        {
            Reference.Log.Debug(" Request:" + _obj.RequestIrrfileUUID.ToString());
            _obj.Requesting = true;

            lock (requestingList)
            {
                if (requestingList.ContainsKey(_obj.RequestIrrfileUUID))
                {
                    Reference.Log.Debug(" Already Requested:" + _obj.RequestIrrfileUUID.ToString());

                    requestingList[_obj.RequestIrrfileUUID].Add(_obj);
                    return;
                }
                else
                {
                    List<VObject> requestors = new List<VObject>();
                    requestors.Add(_obj);
                    requestingList.Add(_obj.RequestIrrfileUUID, requestors);
                }
            }

            IrrMeshThread ss = new IrrMeshThread(Reference.Viewer, _obj, workDirectory);
            IrrWorkItem item = new IrrWorkItem("IrrMeshThread.Requesting", new WorkItemCallback(ss.Requesting), null);
            IrrWorkItemQueue(item);
        }

        private bool CheckAsset(IrrParseLib.IrrDatas _datas, bool checkTextures)
        {
            bool flag = CheckAssetFileExsists(_datas.Mesh.Param.Mesh);

            if (!flag)
                return flag;

            if (checkTextures)
            {
                foreach (IrrMaterial material in _datas.Materials)
                {
                    flag &= CheckAssetFileExsists(material.Texture1);
                    flag &= CheckAssetFileExsists(material.Texture2);
                    flag &= CheckAssetFileExsists(material.Texture3);
                    flag &= CheckAssetFileExsists(material.Texture4);
                }
            }

            if (!flag || _datas.Childs == null)
                return flag;

            foreach (IrrParseLib.IrrDatas datas in _datas.Childs)
            {
                if (!CheckAsset(datas, checkTextures))
                    return false;
            }

            return flag;
        }

        private bool CheckAssetFileExsists(string _filename)
        {
            if (string.IsNullOrEmpty(_filename))
                return true;

            string path = workDirectory + @"\" + _filename.ToLower();

            if (System.IO.File.Exists(path))
                return true;

            return false;
        }

        public bool Contains(UUID _uuid)
        {
            lock (dataList)
            {
                return dataList.ContainsKey(_uuid);
            }
        }

        public int Count
        {
            get { lock (dataList) { return dataList.Count; } }
        }

        private void ChangeWorkDirectory(string _directory)
        {
            workDirectory = _directory;

            if (!Directory.Exists(workDirectory))
                Directory.CreateDirectory(workDirectory);
        }

        public string WorkDirectory
        {
            get { return workDirectory; }
            set { ChangeWorkDirectory(value); }
        }

        public static UUID GetIrrfileUUID(string _uri, UUID _avatarID)
        {
            if (!_uri.StartsWith("http://"))
                _uri = "http://" + _uri;

            VUtil.LogConsole("OpenViewer.Managers.IrrManager", "GetIrrfileUUID Req:" + _avatarID.ToString() + " Uri:" + _uri);

            // [YK:NEXT]
            Hashtable AvatarRequest_Params = new Hashtable();
            AvatarRequest_Params.Add("user_id", _avatarID.ToString());

            try
            {
                ArrayList XMLRPC_Params = new ArrayList();
                XMLRPC_Params.Add(AvatarRequest_Params);

                XmlRpcRequest Req = new XmlRpcRequest("GetAvatarDataID", XMLRPC_Params);
                XmlRpcResponse Resp = Req.Send(_uri, 6000); // 6000 is timeout(msec)

                if (Resp.IsFault)
                {
                    VUtil.LogConsole("OpenViewer.Managers.IrrManager", "GetIrrfileUUID fault");
                }

                Hashtable res = Resp.Value as Hashtable;

                string uuid = res["avatar_data_id"] as string;

                VUtil.LogConsole("OpenViewer.Managers.IrrManager", "GetIrrfileUUID Get:" + uuid);

                return new UUID(uuid);
            }
            catch(Exception e)
            {
                VUtil.LogConsole("OpenViewer.Managers.IrrManager", "GetIrrfileUUID ERROR:" + e.Message);

                return (UUID.Zero);
            }
        }

        public List<VObject> CloseRequest(UUID _uuid)
        {
            List<VObject> requestors = null;

            lock (requestingList)
            {
                if (requestingList.ContainsKey(_uuid))
                {
                    requestors = requestingList[_uuid];
                    requestingList.Remove(_uuid);
                }
            }
            return requestors;
        }


        /// <summary>
        /// Request asset from IrrDatas by TCP.
        /// </summary>
        /// <param name="_datas">IrrDatas</param>
        /// <param name="_slProtocol">SLProtocol</param>
        public void IrrFileTCPRequestToAssetServer_toplevel(IrrParseLib.IrrDatas _datas, SLProtocol _slProtocol, string _directory, bool fetchTextures)
        {
            IrrFileCreateCache(_datas.Mesh.Param.Name + ".xml", _directory);
            IrrFileTCPRequestToAssetServer_recursive(_datas, _slProtocol, true, _directory, fetchTextures);
        }

        private void IrrFileTCPRequestToAssetServer_recursive(IrrParseLib.IrrDatas _datas, SLProtocol _slProtocol, bool _root, string _directory, bool fetchTextures)
        {
            IrrFileCreateCache(_datas.Mesh.Param.Mesh, _directory);

            if (fetchTextures)
            {
                // Request texture - note no JPEG2000 conversion occurs here.
                foreach (IrrParseLib.IrrMaterial material in _datas.Materials)
                {
                    IrrFileCreateCache(material.Texture1, _directory);
                    IrrFileCreateCache(material.Texture2, _directory);
                    IrrFileCreateCache(material.Texture3, _directory);
                    IrrFileCreateCache(material.Texture4, _directory);
                }
            }

            if (_datas.Childs != null)
            {
                foreach (IrrParseLib.IrrDatas datas in _datas.Childs)
                    IrrFileTCPRequestToAssetServer_recursive(datas, _slProtocol, false, _directory, fetchTextures);
            }
        }

        public string IrrFileCreateCache(string _filename, string _directory)
        {
            if (string.IsNullOrEmpty(_filename))
                return "Filename is null or empty.";

            if (System.IO.File.Exists(_directory + "/" + _filename))
                return _filename + " is Already exist";

            string err = string.Empty;
            string uuid = System.IO.Path.GetFileNameWithoutExtension(_filename);
            string filenameWithoutPath = System.IO.Path.GetFileName(_filename);
            int timeout = Reference.Viewer.Config.Source.Configs["Startup"].GetInt("asset_timeout", 60000);

            try
            {
                RestClient rest = new RestClient(VUtil.assetServerUri);
                rest.RequestMethod = "GET";
                rest.AddResourcePath("assets");
                rest.AddResourcePath(uuid);
                rest.AddHeader("Authorization", "OpenGrid " + VUtil.authToken.ToString());

                System.IO.Stream stream = rest.Request(timeout);

                XmlSerializer xs = new XmlSerializer(typeof(AssetBase));
                AssetBase ab = (AssetBase)xs.Deserialize(stream);

                bool tryToDecompress = true;
                if (tryToDecompress
                    && ab.Data.Length >= 2
                    && ab.Data[0] == 0x1f // gzip header == 0x1f8b
                    && ab.Data[1] == 0x8b)
                {
                    try
                    {
                        // try to uncompress
                        string compressedFilename = _directory + "/" + filenameWithoutPath + ".gz";
                        System.IO.File.WriteAllBytes(compressedFilename, ab.Data);
                        string dstFile = _directory + "/" + filenameWithoutPath;

                        Lib3Di.Compress.AddDecompressionRequest(new DecompressionRequest(compressedFilename, dstFile));
                        Lib3Di.Compress.DecompressWaitingRequests(); // Handle all waiting decompression requests here - one at a time (to prevent file collisions and simultaneous decompression in separate IrrMeshThreads). We could also make a separate thread to handle all decompression requests in sequence, but that thread would need to live forever and would only be active at the start of the program when assets are being downloaded, so it makes more sense to handle decompression requests here (invoked from an IrrMeshThread), where DecompressWaitingRequests() locks the queue and processes everything serially.
                    }
                    catch (Exception e)
                    {
                        Reference.Log.Warn("[IRR MANAGER]: " + e.Message);
                        Reference.Log.Debug("[IRR MANAGER]: " + e.StackTrace);
                        File.WriteAllBytes(_directory + "/" + filenameWithoutPath, ab.Data);
                    }
                    finally
                    {
                        File.Delete(_directory + "/" + filenameWithoutPath + ".gz");
                    }
                }
                else
                {
                    System.IO.File.WriteAllBytes(_directory + "/" + filenameWithoutPath, ab.Data);
                }
            }
            catch (Exception e)
            {
                err = e.Message;
            }

            return err;
        }

        /// <summary>
        /// Request asset from IrrDatas.
        /// </summary>
        /// <param name="_datas">IrrDatas</param>
        /// <param name="_slProtocol">SLProtocol</param>
        private void IrrFileRequestToAssetServer(IrrParseLib.IrrDatas _datas, SLProtocol _slProtocol)
        {
            Reference.Viewer.ProtocolManager.RequestImage(Path.GetFileNameWithoutExtension(_datas.Mesh.Param.Mesh), true);

            foreach (IrrParseLib.IrrMaterial material in _datas.Materials)
            {
                Reference.Viewer.ProtocolManager.RequestImage(Path.GetFileNameWithoutExtension(material.Texture1), true);
                Reference.Viewer.ProtocolManager.RequestImage(Path.GetFileNameWithoutExtension(material.Texture2), true);
                Reference.Viewer.ProtocolManager.RequestImage(Path.GetFileNameWithoutExtension(material.Texture3), true);
                Reference.Viewer.ProtocolManager.RequestImage(Path.GetFileNameWithoutExtension(material.Texture4), true);
            }

            if (_datas.Childs == null)
                return;

            foreach (IrrParseLib.IrrDatas datas in _datas.Childs)
                IrrFileRequestToAssetServer(datas, _slProtocol);
        }

        /// <summary>
        /// Load to scene from Irrlicht file.
        /// </summary>
        /// <param name="_datas">IrrDatas</param>
        /// <param name="_smgr">Scene Manager</param>
        /// <param name="_vObj">VObject</param>
        /// <param name="_Entities">VObject list</param>
        public AnimatedMeshSceneNode IrrFileLoad(IrrParseLib.IrrDatas _datas, SceneManager _smgr, VObject _obj, string _prefix)
        {
            _obj.MeshNode = IrrMeshLoad(_datas, _smgr, _obj.Node, _prefix);

            // Copy base param.
            _obj.BaseParam = _datas.Mesh.Param;

            // Create animation key.
            _datas.CreateAnimationKey(workDirectory);
            foreach (IrrParseLib.KeyframeSet key in _datas.AnimationKey.Keys)
            {
                if (!_obj.FrameSetList.ContainsKey(key.Name))
                    _obj.FrameSetList.Add(key.Name, new IrrParseLib.KeyframeSet(key.Name, key.AnimationSpeed, key.StartFrame, key.EndFrame));
            }

            _obj.MeshNode.Position = new Vector3D(_obj.BaseParam.Position[0], _obj.BaseParam.Position[1], _obj.BaseParam.Position[2]);
            _obj.MeshNode.Rotation = new Vector3D(_obj.BaseParam.Rotation[0], _obj.BaseParam.Rotation[1], _obj.BaseParam.Rotation[2]);
            _obj.MeshNode.Scale = new Vector3D(_obj.BaseParam.Scale[0], _obj.BaseParam.Scale[1], _obj.BaseParam.Scale[2]);

            return _obj.MeshNode;
        }

        private AnimatedMeshSceneNode IrrMeshLoad(IrrParseLib.IrrDatas _datas, SceneManager _smgr, SceneNode _node, string _prefix)
        {
            string prefix = string.Empty;

            if (!string.IsNullOrEmpty(_prefix))
            {
                prefix = _prefix;

                try
                {
                    string copy = workDirectory + prefix + _datas.Mesh.Param.Mesh;
                    if (!File.Exists(copy))
                        System.IO.File.Copy(workDirectory + _datas.Mesh.Param.Mesh, copy);
                }
                catch (Exception e)
                {
                    Reference.Log.Error("IrrMeshLoad", e);
                }
            }

            AnimatedMesh mesh = _smgr.GetMesh(workDirectory + prefix + _datas.Mesh.Param.Mesh);
            AnimatedMeshSceneNode node = _smgr.AddAnimatedMeshSceneNode(mesh);
            Reference.Log.Info("[LOADING MESH]: " + workDirectory + prefix + _datas.Mesh.Param.Mesh);

            // Set material.
            for (int i = 0; i < node.MaterialCount; i++)
            {
                if (i < _datas.Materials.Count)
                {
                    node.GetMaterial(i).MaterialType = (MaterialType)_datas.Materials[i].Type;
                    node.GetMaterial(i).BackfaceCulling = _datas.Materials[i].BackfaceCulling;
                    node.GetMaterial(i).FogEnable = _datas.Materials[i].FogEnable;
                    node.GetMaterial(i).GouraudShading = _datas.Materials[i].GouraudShading;
                    node.GetMaterial(i).Lighting = _datas.Materials[i].Lighting;
                    node.GetMaterial(i).NormalizeNormals = _datas.Materials[i].NormalizeNormals;
                    node.GetMaterial(i).Shininess = _datas.Materials[i].Shininess;
                    node.GetMaterial(i).ZBuffer = (uint)_datas.Materials[i].ZBuffer;
                    node.GetMaterial(i).ZWriteEnable = _datas.Materials[i].ZWriteEnable;
                    node.GetMaterial(i).Wireframe = _datas.Materials[i].Wireframe;
                }
            }

            _node.AddChild(node);

            if (_datas.Childs != null)
            {
                foreach (IrrParseLib.IrrDatas datas in _datas.Childs)
                {
                    IrrMeshLoad(datas, _smgr, node, _prefix);
                }
            }

            return node;
        }

        private void DeleteTempMeshFile()
        {
            string[] files = System.IO.Directory.GetFiles(workDirectory);
            try
            {
                for (int i = 0; i < files.Length; i++)
                {
                    if (files[i].Contains("tmpmesh_"))
                        System.IO.File.Delete(files[i]);
                }
            }
            catch (Exception e)
            {
                Reference.Log.Error("DeleteTempMeshFile", e);
            }
        }
    }
}
