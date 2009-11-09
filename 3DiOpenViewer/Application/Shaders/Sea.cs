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
using IrrlichtNETCP;

namespace OpenViewer.Shaders
{
    public class Sea : BaseShader
    {
        private SceneNode node;

        // directory
        private string shaderDirectory;
        private string modelDirectory;
        private string textureDirectory;

        public Sea(Viewer viewer, SceneNode _parentNode)
            : base(viewer, -1, _parentNode)
        {
            shaderDirectory = Util.ApplicationDataDirectory + @"/media/shaders/";
            modelDirectory = Util.ApplicationDataDirectory + @"/media/models/";
            textureDirectory = Util.ApplicationDataDirectory + @"/media/textures/";
        }

        public override void Update(uint frame)
        {
            float waterheight = 0;
            if (Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim != null)
            {
                waterheight = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.WaterHeight;
            }
            node.Position = new Vector3D(128, waterheight, 128);

            base.Update(frame);
        }

        public override int Load()
        {
            const string filename = "sea_shader_low.fx";

            int res = -1;

            string path = shaderDirectory + filename;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    res = Reference.VideoDriver.GPUProgrammingServices.AddHighLevelShaderMaterialFromFiles(
                        path, "VS", VertexShaderType._2_0,
                        path, "PS", PixelShaderType._2_0,
                        ShaderEvent, MaterialType.TransparentAlphaChannel, 0
                        );
                }
                catch (Exception e)
                {
                    Reference.Log.Fatal("Load", e);
                }

                if (res > 0)
                    Reference.Log.Debug("Load: Loaded" + path);
                else
                    Reference.Log.Error("Load: not Loaded:" + path);
            }
            else
            {
                Reference.Log.Warn("Load: File not exsit:" + path);
            }


            if (res > 0)
            {
                const string meshName = "sea001.x";

                if (System.IO.File.Exists(modelDirectory + meshName))
                {
                    AnimatedMesh mesh = Reference.SceneManager.GetMesh(modelDirectory + meshName);

                    node = Reference.SceneManager.AddMeshSceneNode(mesh.GetMesh(0), parentNode, -1);
                    node.Position = new Vector3D(128, 0, 128);
                    node.SetMaterialFlag(MaterialFlag.BackFaceCulling, false);
                }
            }

            return res;
        }

        public override void ShaderEvent(IrrlichtNETCP.MaterialRendererServices services, int userData)
        {
            base.ShaderEvent(services, userData);
        }
    }
}
