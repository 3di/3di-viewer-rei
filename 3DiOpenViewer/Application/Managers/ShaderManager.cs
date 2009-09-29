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
using System.Collections.Generic;
using IrrlichtNETCP;
using IrrlichtNETCP.Extensions;

namespace OpenViewer.Managers
{
    using Shaders;

    public class ShaderManager : BaseManager
    {
        private Dictionary<ShaderManager.ShaderType, int> shaderIndex = new Dictionary<ShaderManager.ShaderType, int>();

        public enum ShaderType
        {
            Sky,
            AdvancedSea,
            Sea,
            Shadow,

            Count,
        }

        private Dictionary<ShaderType, BaseShader> shaders = new Dictionary<ShaderType, BaseShader>();

        public ShaderManager(Viewer viewer)
            : base(viewer, -1)
        {
        }

        ~ShaderManager()
        {
            lock (shaders)
            {
                foreach (BaseShader shader in shaders.Values)
                    shader.Cleanup();

                shaders.Clear();
            }

            lock (shaderIndex)
            {
                shaderIndex.Clear();
            }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Cleanup()
        {
            /*
            lock (shaders)
            {
                foreach (BaseShader shader in shaders.Values)
                    shader.Cleanup();

                shaders.Clear();
            }

            lock (shaderIndex)
            {
                shaderIndex.Clear();
            }
            */
            //base.Cleanup();
        }

        public override void Update(uint frame)
        {
            try
            {
                lock (shaders)
                {
                    foreach (BaseShader shader in shaders.Values)
                        shader.Update(frame);
                }
            }
            catch (Exception e)
            {
                
                Reference.Log.Debug("Update: " + e.Message);
            }

            base.Update(frame);
        }

        public void LoadAll()
        {
            LoadAll(ParentNode);
        }

        private void LoadAll(SceneNode _node)
        {
            for (int i = 0; i < (int)ShaderType.Count; i++)
                Load(_node, (ShaderType)i);
        }

        public int Load(SceneNode _node, ShaderType type)
        {
            BaseShader shader = null;
            switch (type)
            {
                case ShaderType.Sky:
                    if (Reference.Viewer.IsDrawSky)
                    {
                        shader = new Sky(Reference.Viewer, ParentNode);
                    }
                    break;

                case ShaderType.Sea:
                    if (Reference.Viewer.IsDrawSea)
                    {
                        if (Reference.Viewer.SeaQuality == Viewer.ShaderLevelType.Low) 
                            shader = new Sea(Reference.Viewer, ParentNode);
                    }
                    break;

                case ShaderType.AdvancedSea:
                    if (Reference.Viewer.IsDrawSea)
                    {
                        if (Reference.Viewer.SeaQuality == Viewer.ShaderLevelType.High)
                            shader = new AdvancedSea(Reference.Viewer, ParentNode);
                    }
                    break;

                case ShaderType.Shadow:
                    if (Reference.Viewer.IsDrawSky)
                    {
                    }
                    break;
            }

            int res = -1;
            if (shader != null)
            {
                // Load shader.
                //Reference.Device.FileSystem.WorkingDirectory = Util.ApplicationDataDirectory + @"\media";
                res = shader.Load();
                if (res > 0)
                {
                    if (shaders.ContainsKey(type))
                    {
                        Reference.Log.Debug("[SHADER]: " + type.ToString() + " Read multi.");
                    }
                    else
                    {
                        lock (shaders)
                        {
                            shaders.Add(type, shader);
                        }

                        lock (shaderIndex)
                        {
                            shaderIndex.Add(type, res);
                        }
                    }
                }
                else
                {
                    Reference.Log.Debug("[SHADER]: " + type.ToString() + " Shader don't load.");
                }
            }

            return res;
        }

        public int GetShader(ShaderType type)
        {
            int index = -1;

            lock (shaderIndex)
            {
                if (shaderIndex.ContainsKey(type))
                    index = shaderIndex[type];
            }

            return index;
        }

        public BaseShader GetShaderObject(ShaderType type)
        {
            return(shaders[type]);
        }
    }
}
