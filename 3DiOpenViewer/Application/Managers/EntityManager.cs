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

#define DebugTexturePipeline
#define DebugObjectPipeline
using System;
using System.Collections.Generic;
using IrrlichtNETCP;
using OpenMetaverse;
using IrrParseLib;
using OpenViewer.Primitives;

namespace OpenViewer.Managers
{
    public enum Operations { ADD, UPDATE, DELETE};

    public struct Action<T>
    {
        T obj;
        Operations operation;

        public T Object { get { return (obj); } set { obj = value; } }
        public Operations Operation { get { return (operation); } set { operation = value; } }

        public Action(T obj, Operations op)
        {
            this.obj = obj; operation = op;
        }
    }

    public struct TextureCompleteNotification
    {
        public VObject vObj;
        public string texture;
        public string extension;
        public UUID textureID;
    }

    public class EntityManager : BaseManager
    {
        #region Properties
        // Picker
        private TrianglePickerMapper trianglePickerMapper = null;
        public TrianglePickerMapper TrianglePickerMapper { get; set; }

        private MetaTriangleSelector metaTriangleSelector = null;
        public MetaTriangleSelector MetaTriangleSelector { get; set; }

        private Primitive primitiveUnderMouse = null;
        public Primitive PrimitiveUnderMouse { get { return (primitiveUnderMouse); } }

        private Dictionary<string, VObject> entities = new Dictionary<string, VObject>();
        public Dictionary<string, VObject> Entities { get { return (this.entities); } }

        public int EntitiesCount { get { lock (entities) { return entities.Count; } } }

        #endregion

        #region Private members

        #region Queues
        private Queue<TextureCompleteNotification> textureQueue = new Queue<TextureCompleteNotification>();

        private Queue<Action<VObject>> pipeline = new Queue<Action<VObject>>();
        private Queue<EventQueueParam> eventQueue = new Queue<EventQueueParam>();


#if DEBUG_QUEUE
        public int TextureQueueLength { get { return (textureQueue.Count); } }
        public int PipelineQueueLength { get { return (pipeline.Count); } }
#endif
        #endregion

        private uint objectUpdateRate = 3;
        private uint textureUpdateRate = 10;

        private uint objectUpdateCount = 2000;
        private uint textureUpdateCount = 500;

        private MeshFactory meshFactory = null;
        private Dictionary<string, VObject> interpolationTargets = new Dictionary<string, VObject>();
        private List<SetTextureParam> requestTextureList = new List<SetTextureParam>();
        #endregion

        public EntityManager(Viewer viewer)
            : base(viewer, -1)
        {
            // MeshFactory needs to stay and not be cleaned up between sessions to speed up construction
            // of meshes (needs to be properly released at the end of its lifecycle though)
            meshFactory = new MeshFactory(Reference.SceneManager.MeshManipulator, Reference.Device);
        }

        #region Public Interface

        public override void Initialize()
        {
            base.Initialize();

            trianglePickerMapper = new TrianglePickerMapper(Reference.SceneManager.CollisionManager);
            metaTriangleSelector = Reference.SceneManager.CreateMetaTriangleSelector();

            Reference.Viewer.ProtocolManager.OnTextureFromWebLoaded -= ProtocolManager_OnTextureFromWebLoaded;
            Reference.Viewer.ProtocolManager.OnTextureFromWebLoaded += ProtocolManager_OnTextureFromWebLoaded;
            Reference.Viewer.TextureManager.OnTextureLoaded -= TextureManager_OnTextureLoaded;
            Reference.Viewer.TextureManager.OnTextureLoaded += TextureManager_OnTextureLoaded;
        }

        public override void Cleanup()
        {
            lock (textureQueue)
            {
                textureQueue.Clear();
            }
            lock (pipeline)
            {
                pipeline.Clear();
            }

            lock (entities)
            {
                foreach (VObject v in entities.Values)
                {
                    if (v.Node != null)
                    {
                        // If we're interpolating this object, stop
                        lock (interpolationTargets)
                        {
                            if (interpolationTargets.ContainsKey(v.Prim.RegionHandle.ToString() + v.Prim.LocalID.ToString()))
                            {
                                interpolationTargets.Remove(v.Prim.RegionHandle.ToString() + v.Prim.LocalID.ToString());
                            }

                        }

                        DeleteNode(v.Node);
                        v.Node = null; // maybe can be removed later
                    }
                }
                entities.Clear();
            }

            metaTriangleSelector.Dispose();

            base.Cleanup();
        }

        public override void Update(uint frame)
        {
            try
            {
                if (frame % 10 == 0)
                    DetectObjectUnderMouse();

                if (frame % 10 == 0)
                    EventControlProcess();

                if (frame % objectUpdateRate == 0)
                    ProcessObjectQueue();

                if (frame % textureUpdateRate == 0)
                    ProcessTextureQueue();
            }
            catch (Exception e)
            {
                Reference.Log.Debug("Update: " + e.Message);
            }

            base.Update(frame);
        }

        #region Prims

        public void HandleNewPrimEvent_warning(OpenMetaverse.Primitive prim, ulong regionHandle, ushort timeDilation) 
        // warning: libomv invokes OnNewPrim event both for new prims and updates to prims
        {
            VObject vObj = new VObject();
            vObj.Prim = prim;


            int numOpaqueBlocks = vObj.Prim.OpaqueExtraData.Count;
            if (numOpaqueBlocks > 0)
            {
                for (int b = 0; b < numOpaqueBlocks; b++)
                {
                    byte[] opaqueData = vObj.Prim.OpaqueExtraData[b];
                    int blockID = opaqueData[0] + 256 * opaqueData[1];
                    if (blockID == 0x200) // 3Di data
                    {
                        int iByte = 4 + 2; // skip type and length
                        UUID irrFileUUID = new UUID(opaqueData, iByte);
                        iByte += 16;
                        // FIXME: colMeshUUID is unused
                        UUID colMeshUUID = new UUID(opaqueData, iByte);
                        vObj._3DiIrrfileUUID = irrFileUUID;

                        if (Reference.Viewer.IrrManager.Contains(irrFileUUID) == false)
                        {
                            vObj.RequestIrrfileUUID = irrFileUUID;
                            vObj.requestTexturesDirectlyFromAssetServerWithoutJ2KConversion = false;
                            Reference.Viewer.IrrManager.RequestObject(vObj); // will enqueue vObj when all data is downloaded
                        }
                        else
                        {
                            vObj.NeedToReload3DiMesh = true;
                            vObj.UpdateFullYN = false;
                        }
                    }
                }
            }


            AddObject(vObj); // always AddObject, never UpdateObject(). Later we can optimize.
        }

        public void AddObject(VObject vObj)
        {
            lock (pipeline) { pipeline.Enqueue(new Action<VObject>(vObj, Operations.ADD)); }
        }

        public void DeleteObject(ulong regionHandle, uint localId)
        {
            string objId = regionHandle.ToString() + localId.ToString();
            if (entities.ContainsKey(objId))
            {
                VObject obj = entities[objId];
                lock (pipeline) { pipeline.Enqueue(new Action<VObject>(obj, Operations.DELETE)); }
            }
        }

        public void UpdateObject(VObject obj)
        {
            lock (pipeline) { pipeline.Enqueue(new Action<VObject>(obj, Operations.UPDATE)); }
        }

        public void AddTexture(TextureCompleteNotification tx)
        {
            lock (textureQueue) { textureQueue.Enqueue(tx); }
        }
        #endregion

        #endregion

        #region Internals

        #region Prims

        private void ProcessTextureQueue()
        {
            int processed = 0;
            while (textureQueue.Count > 0 && processed < textureUpdateCount)
            {
                TextureCompleteNotification tx;
                TextureExtended tex = null;

                lock (textureQueue)
                {
                    tx = textureQueue.Dequeue();

                    // Try not to double load the texture first.
                    if (!Reference.Viewer.TextureManager.tryGetTexture(tx.textureID, out tex))
                    {
                        // Nope, we really don't have that texture loaded yet.  Load it now.
                        tex = new TextureExtended(Reference.VideoDriver.GetTexture(tx.texture + tx.extension).Raw, tx.extension);
                    }
                }

                if (tx.vObj != null && tex != null)
                {
                    Primitive.SculptData sculpt = tx.vObj.Prim.Sculpt;
                    if (sculpt != null && tx.textureID == sculpt.SculptTexture)
                    {
                        continue;
                        // applyTexture will skip over textures that are not 
                        // defined in the textureentry
                    }

                    Reference.Viewer.TextureManager.applyTexture(tex, tx.vObj, tx.textureID);
                }
            }
        }

        private void ProcessObjectQueue() 
        {
            lock (pipeline)
            {
                uint processed = 0;
                while (pipeline.Count > 0 && processed < objectUpdateCount)
                {
                    Action<VObject> action = pipeline.Dequeue();
                    processed++;

                    PCode pCode = action.Object.Prim.PrimData.PCode;
                    if (pCode == PCode.Grass || pCode == PCode.NewTree || pCode == PCode.Tree)
                    {
                        continue;
                    }

                    // [YK:NEXT]
                    //ProcessObjectQueue(action.Object);
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
        }

        private void ProcessObjectQueueAddToNode(VObject _obj)
        {
            
            string objId = _obj.Prim.RegionHandle.ToString() + _obj.Prim.LocalID.ToString();
            if (Entities.ContainsKey(objId))
            {
                VObject oldObj = Entities[objId];
                ProcessObjectQueueDeleteToNode(oldObj);
            }

            if (_obj._3DiIrrfileUUID != UUID.Zero)
            {
                _obj.NeedToReload3DiMesh = true;
            }
            ProcessObjectQueue(_obj, Operations.ADD);
        }

        private void ProcessObjectQueueUpdateToNode(VObject _obj)
        {
            if (_obj._3DiIrrfileUUID != UUID.Zero)
            {
                _obj.NeedToReload3DiMesh = true;
            }

            ProcessObjectQueue(_obj, Operations.UPDATE);
        }

        public void DeleteNode(SceneNode node)
        {
            if (node == null)
                return;

            // Remove this object from our picker.
            try
            {
                if (node.TriangleSelector != null)
                {
                    if (metaTriangleSelector != null)
                    {
                        // NOTE: This call will drop() the triangle selector, but that is only to
                        // decrease the refcount to the state it was before the trisel was added
                        // to the metaTriSel's array, that grab()-ed it.
                        metaTriangleSelector.RemoveTriangleSelector(node.TriangleSelector);
                    }
                    if (trianglePickerMapper != null)
                    {
                        trianglePickerMapper.RemTriangleSelector(node.TriangleSelector);
                    }
                    // ~ISceneNode will drop the triangle selector associated with this scenenode!
                    // However it will not be removed from NativeElement.Elements because we are
                    // calling SceneManager.AddToDeletionQueue() the deletion will execute later 
                    // in native code only.
                    lock (NativeElement.Elements) { if (NativeElement.Elements.ContainsKey(node.TriangleSelector.Raw)) { NativeElement.Elements.Remove(node.TriangleSelector.Raw); } }
                }
            }
            catch (AccessViolationException e)
            {
                // TrianglePicker was release before
                Reference.Log.Warn("[ENTITY MANAGER]: " + e.Message);
                Reference.Log.Debug("[ENTITY MANAGER]: " + e.StackTrace);
            }
            try
            {
                if (node.MaterialCount > 0)
                {
                    for (int i = 0; i < node.MaterialCount; i++)
                    {
                        // TODO: Check if materials are released properly on the C++ side
                        lock (NativeElement.Elements) { if (NativeElement.Elements.ContainsKey(node.GetMaterial(i).Raw)) { NativeElement.Elements.Remove(node.GetMaterial(i).Raw); } }
                    }
                }
            }
            catch (Exception e)
            {
                Reference.Log.Debug("[ERROR]: " + e.Message + " " + e.StackTrace);
            }

            try
            {
                Reference.SceneManager.AddToDeletionQueue(node);
            }
            catch (Exception e)
            {
                Reference.Log.Error("DeleteNode", e);
            }
        }

        private void DeleteObject(VObject _obj)
        {
            if (_obj.Node != null)
            {
                // If we're interpolating this object, stop
                lock (interpolationTargets)
                {
                    if (interpolationTargets.ContainsKey(_obj.Prim.RegionHandle.ToString() + _obj.Prim.LocalID.ToString()))
                    {
                        interpolationTargets.Remove(_obj.Prim.RegionHandle.ToString() + _obj.Prim.LocalID.ToString());
                    }

                }
                DeleteNode(_obj.Node);
                _obj.Node = null;
            }
        }

        private void ProcessObjectQueueDeleteToNode(VObject _obj)
        {
            DeleteObject(_obj);
            // Remove this object from the known entities.
            Entities.Remove(_obj.Prim.RegionHandle.ToString() + _obj.Prim.LocalID.ToString());
        }

        private void ProcessObjectQueue(VObject vObj, Operations op) // // TODO: refactor different operations into totally different functions
        {
            string objId = VUtil.GetEntitiesKeyFromPrim(vObj.Prim);
            string parId = VUtil.GetEntitiesParentKeyFromPrim(vObj.Prim);

            // Check if object can be immediately added to the scene (ie we have the parent)
            // If we don't have the parent, defer dealing with this object altogether
            VObject parentObj = null;
            SceneNode workNode = ParentNode;

            if (vObj.Prim.ParentID == 0)
            {
                // No parent, continue
                if (op == Operations.ADD)
                {
                    vObj.UpdateFullYN = true;
                }
                else
                {
                    // update. Don't change UpdateFullYN status. Originally this whole function was responsible for deciding
                    // whether or not to recreate the SceneNode, but this decision is now done outside and is passed in here
                    // through the Operations op parameter.
                }
            }
            else
            {
                if (op == Operations.ADD)
                {
                    if (Entities.ContainsKey(parId))
                    {
                        parentObj = Entities[parId];
                        //workNode = parentObj.Node;    // Don't add parent relationship twice
                        vObj.UpdateFullYN = true;
                    }
                    else
                    {
                        // Wait for the parents to arrive first
                        pipeline.Enqueue(new Action<VObject>(vObj, op));
                        return;
                    }
                }
            }

            // Generate SceneNode for the object
            bool isMeshCopied = false;

            // SCULPTED PRIM
            // NOTE: Sculpt meshes will not appear anymore while the texture is downloading
            if (vObj.Prim.Sculpt != null && vObj.Prim.Sculpt.SculptTexture != UUID.Zero)
            {
                TextureExtended sculptTexture = null;
                if (Reference.Viewer.TextureManager.tryGetTexture(vObj.Prim.Sculpt.SculptTexture, out sculptTexture))
                {
                    // Sculpt texture was already downloaded
                    // Apply it here
                    vObj.Mesh = meshFactory.GetSculptMesh(vObj.Prim.Sculpt.SculptTexture, sculptTexture, vObj.Prim.Sculpt.Type, vObj.Prim);
                    if (vObj.Node != null)
                    {
                        DeleteNode(vObj.Node);
                    }
                    vObj.Node = Reference.SceneManager.AddMeshSceneNode(vObj.Mesh, workNode, (int)vObj.Prim.LocalID);
                }
                else
                {
                    // 1) Request the sculpt texture
                    Reference.Viewer.TextureManager.RequestImage(vObj.Prim.Sculpt.SculptTexture, vObj);
                    // 2) Wait for sculpt texture to download
                    pipeline.Enqueue(new Action<VObject>(vObj, op));
                    return;
                }
            }
            // STANDARD PRIM
            else if (vObj._3DiIrrfileUUID == UUID.Zero)
            {
                isMeshCopied = meshFactory.GetMeshInstance(vObj.Prim, out vObj.Mesh);
                // from now on we no longer change the UpdateFullYN status - we just read it.
                if (vObj.UpdateFullYN)
                {
                    vObj.Node = Reference.SceneManager.AddMeshSceneNode(vObj.Mesh, workNode, (int)vObj.Prim.LocalID);
                }
                else
                {
                    isMeshCopied = false;
                }
            }
            // MESH PRIM
            else if (vObj._3DiIrrfileUUID != UUID.Zero && vObj.NeedToReload3DiMesh)
            {
                // ProcessIrrMesh will update vObj.Node as well
                ProcessIrrMesh(vObj);
            }
            else
            {
                Reference.Log.Warn(@"[ENTITYMANAGER]: An unknown type of Prim was inserted into the queue: PrimID:" + vObj.Prim.ID + @" IrrFileUUID:" + vObj._3DiIrrfileUUID + @" ScultTexture:" + vObj.Prim.Sculpt.SculptTexture);
                // Break out of this function without further modifications
                return;
            }

            // A SceneNode was generated by either of the above processes
            // Double check the SceneNode and return if it is actually empty
            if (vObj.Node != null && vObj.Node.Raw != IntPtr.Zero)
            {
                lock (entities)
                {
                    if (entities.ContainsKey(objId))
                    {
                        entities.Remove(objId);
                    }
                    entities.Add(objId, vObj);
                }

                // 1) SCALE
                vObj.Node.Scale = new Vector3D(vObj.Prim.Scale.X, vObj.Prim.Scale.Z, vObj.Prim.Scale.Y);

                // 2) POSITION
                Vector3 worldOffsetPos = Vector3.Zero;  // TODO: Multiregion support needs proper offset coordinates
                if (vObj.Prim.ParentID == 0)
                {
                    // Offset the node by it's world position
                    vObj.Node.Position = new Vector3D(worldOffsetPos.X + vObj.Prim.Position.X, worldOffsetPos.Z + vObj.Prim.Position.Z, worldOffsetPos.Y + vObj.Prim.Position.Y);
                }
                else
                {
                    // Apply rotation and position reported from LibOMV
                    vObj.Prim.Position = vObj.Prim.Position * parentObj.Prim.Rotation;
                    vObj.Prim.Rotation = parentObj.Prim.Rotation * vObj.Prim.Rotation;

                    vObj.Node.Position = new Vector3D(worldOffsetPos.X + parentObj.Prim.Position.X + vObj.Prim.Position.X, worldOffsetPos.Z + parentObj.Prim.Position.Z + vObj.Prim.Position.Z, worldOffsetPos.Y + parentObj.Prim.Position.Y + vObj.Prim.Position.Y);
                }

                // 3) ROTATION
                // Convert Cordinate space
                IrrlichtNETCP.Quaternion iqu = new IrrlichtNETCP.Quaternion(vObj.Prim.Rotation.X, vObj.Prim.Rotation.Z, vObj.Prim.Rotation.Y, vObj.Prim.Rotation.W);

                iqu.makeInverse();

                IrrlichtNETCP.Quaternion finalpos = iqu;

                //finalpos = Viewer.Coordinate_XYZ_XZY * finalpos;
                Vector3D baseRotation = new Vector3D(vObj.BaseParam.Rotation[0], vObj.BaseParam.Rotation[1], vObj.BaseParam.Rotation[2]);
                vObj.Node.Rotation = finalpos.Matrix.RotationDegrees + baseRotation;

                // If the object is physical, add to interpolation targets
                UpdateInterpolationTargets(vObj, objId);

                if (op == Operations.ADD)
                {
                    TriangleSelector trisel = Reference.SceneManager.CreateTriangleSelector(vObj.Mesh, vObj.Node);
                    vObj.Node.TriangleSelector = trisel;
                    AddTriangleSelector(trisel, vObj.Node);
                    trisel.Drop();
                }

                RequestTexturesForObject(vObj);
            }
            if (isMeshCopied)
            {
                for (int i = 0; i < vObj.Mesh.MeshBufferCount; i++)
                    vObj.Mesh.GetMeshBuffer(i).Drop();
                vObj.Mesh.Drop();
            }
        }

        private void RequestTexturesForObject(VObject vObj)
        {
            if (vObj._3DiIrrfileUUID != UUID.Zero)
            {
                // this node has 3Di mesh attached, so ignore prim textures (we already applied mesh textures)
            }
            else // handle normal prim texturing
            {
                if (vObj.Prim.Textures != null)
                {
                    if (vObj.Prim.Textures.DefaultTexture != null)
                    {
                        if (vObj.Prim.Textures.DefaultTexture.TextureID != UUID.Zero)
                        {
                            UUID textureID = vObj.Prim.Textures.DefaultTexture.TextureID;

                            // Only request texture if texture downloading is enabled.
                            Reference.Viewer.TextureManager.RequestImage(textureID, vObj);
                        }
                    }

                    // If we have individual face texture settings
                    if (vObj.Prim.Textures.FaceTextures != null)
                    {
                        Primitive.TextureEntryFace[] objfaces = vObj.Prim.Textures.FaceTextures;
                        for (int i2 = 0; i2 < objfaces.Length; i2++)
                        {
                            if (objfaces[i2] == null)
                                continue;

                            UUID textureID = objfaces[i2].TextureID;

                            if (textureID != UUID.Zero)
                            {
                                Reference.Viewer.TextureManager.RequestImage(textureID, vObj);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateInterpolationTargets(VObject vObj, string objId)
        {
            if (vObj.UpdateFullYN)
            {
                // If the prim is physical, add it to the interpolation targets.
                if ((vObj.Prim.Flags & PrimFlags.Physics) == PrimFlags.Physics)
                {
                    lock (interpolationTargets)
                    {
                        if (!interpolationTargets.ContainsKey(objId))
                            interpolationTargets.Add(objId, vObj);
                    }
                }
                else
                {
                    lock (interpolationTargets)
                    {
                        if ((vObj.Prim is Avatar) == false)
                        {
                            if (interpolationTargets.ContainsKey(objId))
                                interpolationTargets.Remove(objId);
                        }
                    }
                }
            }
        }

        private void ProcessIrrMesh(VObject vObj)
        {
            SceneNode node = vObj.Node;
            vObj.NeedToReload3DiMesh = false;

            // load the 3Di Mesh into a new node and set THAT new node as the current node.
            // also update the mesh (so later operations like generating the triangle picker
            // use the new mesh and not the old one). discard the old node/mesh.

            SceneNode loadedmeshNode = Reference.SceneManager.AddEmptySceneNode(ParentNode, -1);
            string irrFilename = vObj._3DiIrrfileUUID.ToString() + @".irr";
            Reference.SceneManager.LoadScene(Util.ModelFolder + irrFilename, loadedmeshNode);

            Mesh loadedMesh = null;
            for (int iChild = 0; iChild < loadedmeshNode.Children.Length; iChild++)
            {
                if (loadedmeshNode.Children[iChild] is AnimatedMeshSceneNode)
                {
                    AnimatedMeshSceneNode amnode = loadedmeshNode.Children[iChild] as AnimatedMeshSceneNode;
                    if (amnode.AnimatedMesh != null)
                    {
                        loadedMesh = amnode.AnimatedMesh.GetMesh(0);
                        break;
                    }
                }
            }
            if (loadedMesh != null)
            {
                if (node != null) // delete newly added prim node
                {
                    node.Parent.AddChild(loadedmeshNode);
                    DeleteNode(vObj.Node);
                }
                //vObj.Node.SetMaterialFlag(MaterialFlag.PointCloud, true);
                //vObj.Node.AddChild(loadedmeshNode);
                vObj.Mesh = loadedMesh;
                vObj.Node = loadedmeshNode;
                if (loadedmeshNode.Children.Length > 0)
                    vObj.NodeStaticMesh = loadedmeshNode.Children[0];

                //vObj.Mesh = null;
                //vObj.Node = null;
                //node = null;
                // request textures for this just-loaded node. This not only
                // fetches textures, but also applies them. Even if we already have
                // all textures in the cache (and thus loaded them with the .irr 
                // file automatically), we need to check if there is an alpha texture
                // and if we need to make the node alpha. This check is done during
                // texture application, so we force texture re-application in any
                // event (regardless of whether or not the textures are already applied)
                if (vObj._3DiIrrfileUUID != UUID.Zero && Reference.Viewer.TextureManager != null)
                {
                    // This node is a 3Di mesh node prim. So don't request the normal prim texture(s);
                    // instead request the textures of the .irr file.

                    if (Reference.Viewer.IrrManager.Contains(vObj._3DiIrrfileUUID))
                    {
                        IrrDatas irrData = Reference.Viewer.IrrManager.GetObject(vObj._3DiIrrfileUUID, false);
                        vObj.IrrData = irrData;
                        if (irrData != null)
                        {
                            foreach (IrrMaterial irrmat in irrData.Materials)
                            {
                                if (irrmat.Texture1.Trim().Length > 0)
                                {
                                    string reqUUIDstring = System.IO.Path.GetFileNameWithoutExtension(irrmat.Texture1.Trim());
                                    UUID reqUUID = new UUID(reqUUIDstring);
                                    Reference.Viewer.TextureManager.RequestImage(reqUUID, vObj);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // we couldn't find the mesh underneath the loaded scene node.
                // This means we couldn't load the .irrmesh file in the .irr file.
                // This means the .irrmesh file wasn't (yet) downloaded into the assets/ directory.
                // Since we are fetching assets asynchronously, this means that the .irrmesh 
                // file may still be downloading, or it really doesn't exist on the asset server
                // (it may have been deleted, and the reX viewer used its local cached mesh copy 
                // to assign the mesh UUID).
                // So, we couldn't load the mesh, so just ignore it for now and display it as
                // a normal prim.
                DeleteNode(loadedmeshNode);
                // If and when the irr file arrives from its asynchronous download thread, it
                // will enqueue itself again (see end of IrrMeshThread.Requesting), so we will
                // come here again. At that time we will use the vObj._3DiIrrFileUUID and
                // again realize the mesh needs to be reloaded.
            }
        }


        public void AddTriangleSelector(TriangleSelector trisel, SceneNode node)
        {
            trianglePickerMapper.AddTriangleSelector(trisel, node);
            if (metaTriangleSelector != null)
            {
                lock (metaTriangleSelector)
                {
                    metaTriangleSelector.AddTriangleSelector(trisel);
                }
            }
        }


        #endregion

        // Picker
        private void DetectObjectUnderMouse()
        {
            if (Reference.SceneManager != null
                && Reference.Device != null
                && Reference.SceneManager.CollisionManager != null)
            {
                // first do a quick bounding box check. note that the irrlicht bounding boxes seem for some reason
                // to be larger than necessary (use irrNodeUnderMouse.DebugDataVisible = DebugSceneType.BoundingBox; 
                // to see bounding boxes). however we ignore this problem and just use the (overly large) bounding
                // box as an early-out rejection test; actual scene node determination is then done with a more precise
                // triangle picker.

                // TODO: need to test if this early-out rejection is actually faster or not than just doing the
                // triangle selection only. This GetSceneNodeFromSceneCoordinates actually loops over all
                // child scene nodes of the root and checks each bounding box in turn. Is this, plus the
                // potential to skip the triangle test, faster than just doing the triangle test?
                //
                // The answer is probably YES, because the simple irrlicht TriangleSelector (CTriangleSelector.cpp)
                // returns ALL triangles in the mesh. For normal prims, we attach a simple triangle selector to
                // them with CreateTriangleSelector. Therefore, a bounding-box early-out test will save some work here,
                // even if we have to loop over all bounding boxes.
                SceneNode irrNodeUnderMouse =
                    Reference.SceneManager.CollisionManager.GetSceneNodeFromScreenCoordinates
                        (Reference.Device.CursorControl.Position,
                        0,
                        false);
                if (irrNodeUnderMouse == null)
                {
                    // if no bounding box collision with the mouse ray, then we skip the more expensive triangle test
                    primitiveUnderMouse = null;
                }
                else
                {
                    // there was a bounding box collision. do the more expensive triangle test now.

                    // TODO: the triPicker.GetSceneNodeFromRay loops through ALL triangle selectors,
                    // including the terrain triangle selector. This might cause a performance slowdown
                    // here, although the terrain triangle selector (CTerrainTriangleSelector::getTriangles)
                    // does do bounding box checks to reduce the amount of work.
                    //
                    // if performance is a problem we can add a bool flag to triPicker.GetSceneNodeRay;
                    // set it to false to ignore terrain when we are only interested in clicking on objects;
                    // set it to true when we are also interested in clicking on terrain (e.g. for "go here").
                    Reference.Viewer.Camera.ResetMouseOffsets();
                    Line3D projectedray = Reference.SceneManager.CollisionManager.GetRayFromScreenCoordinates
                         (Reference.Device.CursorControl.Position + Reference.Viewer.CursolOffset,
                         Reference.Viewer.Camera.SNCamera);
                    irrNodeUnderMouse =
                        trianglePickerMapper.GetSceneNodeFromRay(projectedray, 0x0128, true, Reference.Viewer.Camera.SNCamera.Position); //smgr.CollisionManager.GetSceneNodeFromScreenCoordinates(new Position2D(p_event.MousePosition.X, p_event.MousePosition.Y), 0, false);
                    bool foundRegionPrimCorrespondingToIrrNodeId = false;
                    VObject vobj = null;
                    if (irrNodeUnderMouse != null
                        && irrNodeUnderMouse.ID != -1)
                    {
                        //irrNodeUnderMouse.DebugDataVisible = DebugSceneType.BoundingBox;

                        //FIXME: inefficient. Is there a better way to get the VObject given the Irrlicht Scene Node ID?
                        //we may need to declare and manage our own dictionary.
                        lock (Entities)
                        {
                            foreach (string vobjkey in Entities.Keys)
                            {
                                vobj = Entities[vobjkey];
                                if (vobj != null
                                    && vobj.Node != null
                                    && vobj.Node.ID == irrNodeUnderMouse.ID)
                                {
                                    primitiveUnderMouse = vobj.Prim; // NOTE: no lock here. Lock needed if multithreaded access to this variable.

                                    foundRegionPrimCorrespondingToIrrNodeId = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (!foundRegionPrimCorrespondingToIrrNodeId)
                    {
                        primitiveUnderMouse = null;
                    }
                }
            }
        }

        #region UUID/LocalID Function

        public string GetUUIDFromLocalID(uint _localid)
        {
            string uuid = string.Empty;

            lock (entities)
            {
                foreach (VObject obj in entities.Values)
                {
                    if (obj.Prim == null)
                        continue;

                    if (obj.Prim.LocalID == _localid)
                    {
                        uuid = obj.Prim.ID.ToString();
                        break;
                    }
                }
            }

            return uuid;
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

        public uint GetLocalIDFromUUID(string _uuid)
        {
            uint localid = 0;

            lock (entities)
            {
                foreach (VObject obj in entities.Values)
                {
                    if (obj.Prim == null)
                        continue;

                    if (obj.Prim.ID == new UUID(_uuid))
                    {
                        localid = obj.Prim.LocalID;
                        break;
                    }
                }
            }

            return localid;
        }

        #endregion

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

            if (vObject.NodeStaticMesh == null)
                return false;

            int count = (int)vObject.NodeStaticMesh.MaterialCount;
            if (_materialIndex > (count - 1))
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
            vObject.NodeStaticMesh.GetMaterial(_materialIndex).Texture1 = tex;
            

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
