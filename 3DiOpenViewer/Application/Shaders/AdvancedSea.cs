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
    public class AdvancedSea : BaseShader
    {
        SceneNode WaterSceneNode;
        IrrlichtNETCP.Timer Timer;
        Dimension2Df Size;
        int ShaderMaterial;

        CameraSceneNode Camera = null;

        Texture RefractionMap = null;
        Texture ReflectionMap = null;

        float WindForce = 5.0f;
        Vector3D WindDirection = new Vector3D(0.0f, 1.0f, 0.0f);
        float WaveHeight = 0.1f;    // 0.3
        Colorf WaterColor = new Colorf(0.2f, 0.4f, 0.9f, 0.85f);
        Colorf SpecularColor = new Colorf(1.0f, 0.7f, 1.0f, 0.9f);
        float Specular = 96;
        float ColorBlendFactor = 0.2f;  // 0.2
        Dimension2D renderTargetSize = new Dimension2D(512, 512);

        public AdvancedSea(Viewer viewer, SceneNode _parentNode)
            : base(viewer, -1, _parentNode)
        {
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        public override void Update(uint frame)
        {
            if (!WaterSceneNode.Visible)
                return;

            Reference.Viewer.AvatarManager.VisibleName(false);

            float waterheight = 0;
            if (Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim != null)
            {
                waterheight = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.WaterHeight;
            }
            WaterSceneNode.Position = new Vector3D(128, waterheight, 128);
            WaterSceneNode.Visible = false;

            // *****************************************
            // REFRACTION MAP
            // *****************************************
            Reference.VideoDriver.SetRenderTarget(RefractionMap, true, true, Color.Blue);
            Plane3Df refractionClipPlane = new Plane3Df(0, WaterSceneNode.Position.Y + 0.1f, 0, 0, -1, 0);
            Reference.VideoDriver.SetClipPlane(0, refractionClipPlane, true);

            try
            {
                Reference.SceneManager.DrawAll();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "Update REFRACTION MAP");
            }

            // *****************************************
            // REFLECTION MAP
            // *****************************************
            Reference.VideoDriver.SetRenderTarget(ReflectionMap, true, true, Color.Blue);
            CameraSceneNode currentCamera = Reference.SceneManager.ActiveCamera;
            Camera.FarValue = currentCamera.FarValue;
            Camera.FOV = currentCamera.FOV;

            Vector3D position = currentCamera.Position;
            position.Y = -position.Y + 2 * WaterSceneNode.Position.Y;
            Camera.Position = position;

            Vector3D target = currentCamera.Target;
            target.Y = -target.Y + 2 * WaterSceneNode.Position.Y;
            Camera.Target = target;

            Reference.SceneManager.ActiveCamera = Camera;

            Plane3Df reflectionClipPlane = new Plane3Df(0, WaterSceneNode.Position.Y - 0.1f, 0, 0, 1, 0);
            Reference.VideoDriver.SetClipPlane(0, reflectionClipPlane, true);

            try
            {
                Reference.SceneManager.DrawAll();
            }
            catch (AccessViolationException)
            {
                VUtil.LogConsole(this.ToString() + "[ACCESSVIOLATION]", "Update REFLECTION MAP");
            }

            // *****************************************
            // RESUME
            // *****************************************
            Reference.SceneManager.ActiveCamera = currentCamera;
            Reference.VideoDriver.SetRenderTarget(null, true, true, Color.White);
            Reference.VideoDriver.EnableClipPlane(0, false);

            WaterSceneNode.Visible = true;

            Reference.Viewer.AvatarManager.VisibleName(true);

            base.Update(frame);
        }

        public override int Load()
        {
            if (WaterSceneNode == null)
            {
                const string filename = "advanced_sea_shader.fx";

                Timer = Reference.Device.Timer;
                Size = new Dimension2Df(256, 256);

                if (Reference.SceneManager.ActiveCamera != null)
                {
                    CameraSceneNode currentCamera = Reference.SceneManager.ActiveCamera;
                    Camera = Reference.SceneManager.AddCameraSceneNode(parentNode);
                    Camera.FarValue = currentCamera.FarValue;
                    Camera.FOV = currentCamera.FOV;
                    Reference.SceneManager.ActiveCamera = currentCamera;
                }
                else
                {
                    Camera = Reference.SceneManager.AddCameraSceneNode(parentNode);
                }

                AnimatedMesh mesh = Reference.SceneManager.AddHillPlaneMesh("terrain", new Dimension2Df(512, 512), new Dimension2D(256, 256), 0.0f, new Dimension2Df(0, 0), new Dimension2Df(1024, 1024));
                SceneNode amsn = Reference.SceneManager.AddOctTreeSceneNode(mesh, parentNode, -1, 128);
                amsn.Position = new Vector3D(128, 0, 128);
                amsn.SetMaterialTexture(0, Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/textures/sand01.jpg"));

                AnimatedMesh watermesh = Reference.SceneManager.AddHillPlaneMesh("realisticwater", Size, new Dimension2D(1, 1), 0f, new Dimension2Df(0, 0), new Dimension2Df(1, 1));
                WaterSceneNode = Reference.SceneManager.AddOctTreeSceneNode(watermesh, parentNode, -1, 128);

                Texture bumpTexture = Reference.VideoDriver.GetTexture(Util.ApplicationDataDirectory + @"/media/textures/waterbump.jpg");

                GPUProgrammingServices gpuProgrammingServices = Reference.VideoDriver.GPUProgrammingServices;

                string path = Util.ApplicationDataDirectory + @"/media/shaders/" + filename;
                if (System.IO.File.Exists(path))
                {
                    try
                    {
                        ShaderMaterial = Reference.VideoDriver.GPUProgrammingServices.AddHighLevelShaderMaterialFromFiles(
                        path, "VertexShaderFunction", VertexShaderType._2_0,
                        path, "PixelShaderFunction", PixelShaderType._2_0,
                        ShaderEvent, MaterialType.Lightmap, 0
                        );
                    }
                    catch (Exception e)
                    {
                        Reference.Log.Fatal("Load", e);
                    }

                    if (ShaderMaterial > 0)
                        Reference.Log.Debug("Load: Loaded" + path);
                    else
                        Reference.Log.Debug("Load: not Loaded:" + path);
                }
                else
                {
                    Viewer.Log.Warn("[SHADER] [SEA] Shader file was not found: " + Util.ApplicationDataDirectory + @"/media/shaders/" + filename);
                }
                float waterheight = 0;
                if (Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim != null)
                {
                    waterheight = Reference.Viewer.ProtocolManager.AvatarConnection.m_user.Network.CurrentSim.WaterHeight;
                }
                WaterSceneNode.Position = new Vector3D(128, waterheight, 128);
                WaterSceneNode.Scale = new Vector3D(200, 1, 200);
                WaterSceneNode.SetMaterialType(ShaderMaterial);
                WaterSceneNode.SetMaterialTexture(0, bumpTexture);
                RefractionMap = Reference.VideoDriver.CreateRenderTargetTexture(renderTargetSize);
                ReflectionMap = Reference.VideoDriver.CreateRenderTargetTexture(renderTargetSize);

                WaterSceneNode.SetMaterialTexture(1, RefractionMap);
                WaterSceneNode.SetMaterialTexture(2, ReflectionMap);
            }
            return ShaderMaterial;
        }

        public void SaveMaps()
        {
            ReflectionMap.Save(Util.LogFolder + @"/advanced_sea_reflection.bmp");
            RefractionMap.Save(Util.LogFolder + @"/advanced_sea_refraction.bmp");
        }

        public override void ShaderEvent(MaterialRendererServices services, int userData)
        {
            IrrlichtNETCP.VideoDriver driver = services.VideoDriver;

            // *****************************************
            // VERTEX SHADER CONSTANTS
            // *****************************************
            Matrix4 worldViewProj = driver.GetTransform(TransformationState.Projection);
            worldViewProj *= driver.GetTransform(TransformationState.View);
            worldViewProj *= driver.GetTransform(TransformationState.World);
            services.SetVertexShaderConstant("WorldViewProj", worldViewProj.ToShader(), 16);

            Matrix4 worldReflectionViewProj = driver.GetTransform(TransformationState.Projection);
            worldReflectionViewProj *= Camera.ViewMatrix;
            worldReflectionViewProj *= driver.GetTransform(TransformationState.World);
            services.SetVertexShaderConstant("WorldReflectionViewProj", worldReflectionViewProj.ToShader(), 16);

            services.SetVertexShaderConstant("WaveLength", 0.01f);   // 0.1

            float time = Reference.Device.Timer.Time / 1000000.0f;
            services.SetVertexShaderConstant("Time", time);

            services.SetVertexShaderConstant("WindForce", WindForce);
            services.SetVertexShaderConstant("WindDirection", WindDirection.ToShader(), 2);

            // *****************************************
            // PIXEL SHADER CONSTANTS
            // *****************************************
            Vector3D cameraPos = Reference.SceneManager.ActiveCamera.Position;
            services.SetPixelShaderConstant("CameraPosition", cameraPos.ToShader());
            services.SetPixelShaderConstant("WaveHeight", WaveHeight);
            services.SetPixelShaderConstant("WaterColor", WaterColor.ToShader());
            services.SetPixelShaderConstant("SpecularColor", SpecularColor.ToShader());
            services.SetPixelShaderConstant("Specular", Specular);
            services.SetPixelShaderConstant("ColorBlendFactor", ColorBlendFactor);

            base.ShaderEvent(services, userData);
        }
    }
}
