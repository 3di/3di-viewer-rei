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
    public class Sky : BaseShader
    {
        private const float CLOUD_CHANGE_SPAN = 6;
        private const float SUN_RISE = (float)(Math.PI * 1d / 12d);
        private const float SUN_SET = (float)(Math.PI * 1d / -10d);
        private const float LATITUDE = 0.61086524f;      // Tokyo's latitude (35ｰN), in radians
        private const float SKY_RADIUS = 180f;          // Fit it to your world's needs

        private SceneNode sunNode;
        private SceneNode moonNode;
        private SceneNode skyNode;
        private Color lastAtomColor = new Color(255, 187, 121, 8);

        private DateTime worldTime = new DateTime(2009, 2, 27, 15, 0, 0);
        private DirectionalLight sunLight;             // Directional lights wrapper for the sun
        private float julianDay;                       // Needed for the sun position calculation
        private Vector3D sunPosition, moonPosition;// Updated in Update, rendered in Render...
        private float moonIntensity;                   // And what intensity (for the moon)?

        // Sky shader value-holders
        private static float turbidity = 2.25f;        // Between 2 and 6, how dense the atmosphere is        

        // Those should not be changed, but could if you want to tweak the colors
        private static Vector3D FOG_NIGHT_COLOR = new Vector3D(0.08f, 0.08f, 0.12f);

        // directory
        private string shaderDirectory;
        private string modelDirectory;
        private string textureDirectory;

        private bool updateOnce = false;
        private float cloudChangeSec = ((CLOUD_CHANGE_SPAN - 1) * 60 * 60 + (60 - 1) * 60 + 60);

        private Colorf[] directionalColorTable = new Colorf[] {
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 00
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 0030
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 01
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 0130
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 02
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 0230
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 03
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 0330
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 04
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 0430
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 05
            new Colorf(1, 0.05f, 0.05f, 0.10f), // 0530
            new Colorf(1, 0.10f, 0.10f, 0.10f), // 06
            new Colorf(1, 0.15f, 0.15f, 0.10f), // 0630
            new Colorf(1, 0.20f, 0.10f, 0.20f), // 07
            new Colorf(1, 0.70f, 0.30f, 0.30f), // 0730 sunrise & moonset 7:15
            new Colorf(1, 0.70f, 0.50f, 0.50f), // 08
            new Colorf(1, 0.70f, 0.60f, 0.75f), // 0830
            new Colorf(1, 0.90f, 0.70f, 0.80f), // 09
            new Colorf(1, 0.90f, 0.90f, 0.90f), // 0930
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 10
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1030
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 11
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1130
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 12
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1230
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 13
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1330
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 14
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1430
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 15
            new Colorf(1, 1.00f, 1.00f, 1.00f), // 1530
            new Colorf(1, 0.90f, 0.90f, 0.90f), // 16
            new Colorf(1, 0.90f, 0.50f, 0.60f), // 1630 
            new Colorf(1, 0.90f, 0.30f, 0.40f), // 1700 sunset & moonrise 16:50
            new Colorf(1, 0.50f, 0.20f, 0.30f), // 1730
            new Colorf(1, 0.30f, 0.20f, 0.20f), // 18
            new Colorf(1, 0.20f, 0.10f, 0.20f), // 1830
            new Colorf(1, 0.20f, 0.10f, 0.20f), // 19
            new Colorf(1, 0.10f, 0.10f, 0.20f), // 1930
            new Colorf(1, 0.05f, 0.05f, 0.20f), // 20
            new Colorf(1, 0.05f, 0.05f, 0.20f), // 2030
            new Colorf(1, 0.05f, 0.05f, 0.20f), // 21
            new Colorf(1, 0.05f, 0.05f, 0.20f), // 2130
            new Colorf(1, 0.02f, 0.02f, 0.20f), // 22
            new Colorf(1, 0.02f, 0.02f, 0.20f), // 2230
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 23
            new Colorf(1, 0.02f, 0.02f, 0.10f), // 2330
        };

        /**
         * Get Julian Day of world time. 
         */
        private float getJulianDay()
        {
            float julian = 0.0f;
            // Commented out this part to show same sun and moon everytime so far.. 
            //int a = (14 - worldTime.Month) / 12, y = 1975 + 4800 - a, m = worldTime.Month + 12 * a - 3;
            //julian = worldTime.DayOfYear + (153 * m + 2) / 5 + y * 365 + y / 4 - y / 100 + y / 400 - 32045;
            //julian -= 2442414;
            //julian -= 1f / 24f;
            return julian;
        }

        public Sky(Viewer _viewer, SceneNode _parentNode)
            : base(_viewer, -1, _parentNode)
        {
            julianDay = getJulianDay();

            shaderDirectory = Util.ApplicationDataDirectory + @"\media\shaders\";
            modelDirectory = Util.ApplicationDataDirectory + @"\media\models\";
            textureDirectory = Util.ApplicationDataDirectory + @"\media\textures\";
        }

        public override void Update(uint frame)
        {
            if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High)
            {
                worldTime = Reference.Viewer.WorldTime;
                julianDay = getJulianDay();
            }

            updateOnce = false;


            // Clamp the direction... could probably use better interpolation
            if (sunLight != null)
            {
                float diff = 0.2f; // diffuse --> ambient.
                float amount = 0;
                int index = 0;

                if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High)
                {
                    amount = worldTime.Minute < 30 ? (float)worldTime.Minute / 30.0f : ((float)worldTime.Minute - 30) / 30.0f;
                    index = worldTime.Minute < 30 ? worldTime.Hour * 2 : worldTime.Hour * 2 + 1;

                    if (Reference.Viewer.IsTickOn)
                        sunLight.UpdateDirection();
                }
                else if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.Low)
                {
                    // [YK:NEXT]
                    index = 13 * 2; // Fix 13h.
                    sunLight.Rotation = new Vector3D(0.50f, 3.14f, 0) * OpenMetaverse.Utils.RAD_TO_DEG;
                }

                Colorf cBefore = directionalColorTable[index];
                Colorf cAfter = directionalColorTable[0];
                if (0 < index && index < directionalColorTable.Length - 1)
                {
                    cAfter = directionalColorTable[index + 1];
                }
                Colorf cc = new Colorf(1, Util.Lerp(cBefore.R, cAfter.R, amount), Util.Lerp(cBefore.G, cAfter.G, amount), Util.Lerp(cBefore.B, cAfter.B, amount));
                sunLight.Diffuse = cc;
                sunLight.Ambient = new Colorf(1, cc.R * diff, cc.G * diff, cc.B * diff);

                if (Reference.Viewer.IsFixDirectional)
                {
                    sunLight.Rotation = Reference.Viewer.DirectionalRotation;
                    sunLight.Diffuse = Reference.Viewer.DirectionalDiffuseColor;
                    sunLight.Ambient = Reference.Viewer.DirectionalAmbientColor;
                }

                sunLight.Update();
            } 

            base.Update(frame);
        }

        public override int Load()
        {
            string filename = "sky_shader.fx";

            if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.Low)
            {
                filename = "sky_shader_low.fx";
            }

            int res = -1;

            string path = shaderDirectory + filename;
            if (System.IO.File.Exists(path))
            {
                try
                {
                    res = Reference.VideoDriver.GPUProgrammingServices.AddHighLevelShaderMaterialFromFiles(
                        path, "VS", VertexShaderType._2_0,
                        path, "PS", PixelShaderType._2_0,
                        ShaderEvent, MaterialType.Solid, 0
                        );
                }
                catch (Exception e)
                {
                    Reference.Log.Fatal("Load", e);
                    res = -1;
                }

                if (res > 0)
                {
                    Reference.Log.Debug("Load: Loaded" + path);
                }
                else
                {
                    Reference.Log.Debug("Load: not Loaded:" + path);
                }
            }
            else
            {
                Reference.Log.Warn("Load: File not exsit:" + path);
            }

            if (res > 0)
            {
                // Create sun.
                sunNode = Reference.SceneManager.AddBillboardSceneNode(parentNode, new Dimension2Df(256, 256), -1);
                sunNode.SetMaterialTexture(0, Reference.VideoDriver.GetTexture(textureDirectory + @"sun.bmp"));
                sunNode.SetMaterialType(MaterialType.TransparentAddColor);
                sunNode.Position = new Vector3D(1, 1, 0) * 8000;
                sunNode.SetMaterialType(MaterialType.TransparentAddColor);
                sunNode.SetMaterialFlag(MaterialFlag.Lighting, false);

                // Create moon.
                moonNode = Reference.SceneManager.AddBillboardSceneNode(parentNode, new Dimension2Df(512, 512), -1);
                moonNode.SetMaterialTexture(0, Reference.VideoDriver.GetTexture(textureDirectory + @"moon.bmp"));
                moonNode.SetMaterialType(MaterialType.TransparentAddColor);
                moonNode.Position = new Vector3D(-1, 1, 0) * 8000;
                moonNode.SetMaterialType(MaterialType.TransparentAddColor);
                moonNode.SetMaterialFlag(MaterialFlag.Lighting, false);

                if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High)
                {
                    const string memisphereMeshName = "sky.x";

                    if (System.IO.File.Exists(modelDirectory + memisphereMeshName))
                    {
                        AnimatedMesh animMesh = Reference.SceneManager.GetMesh(modelDirectory + memisphereMeshName);
                        if (animMesh != null)
                        {
                            Mesh mesh = animMesh.GetMesh(0);
                            skyNode = Reference.SceneManager.AddMeshSceneNode(mesh, parentNode, -1);
                            skyNode.Position = new Vector3D(128, 0, 128);
                            skyNode.Scale = new Vector3D(1.1f, 1.1f, 1.1f);
                            skyNode.SetMaterialFlag(MaterialFlag.BackFaceCulling, false);
                            skyNode.SetMaterialFlag(MaterialFlag.FogEnable, false);
                            skyNode.SetMaterialType(res);

#if YK_UNUSE_MULTI_TEXTURE_STAR
                            const string starTextureName = "star.tga";
                            Texture tex = Reference.VideoDriver.GetTexture(modelDirectory + starTextureName);
                            skyNode.SetMaterialTexture(1, tex);
#endif
                        }
                    }
                }
                else if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.Low)
                {
                    SceneNode node = Reference.SceneManager.AddSkyBoxSceneNode(
                        parentNode,
                        new Texture[]
                        {
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_UP.jpg"),
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_DN.jpg"),
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_LF.jpg"),
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_RT.jpg"),
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_FR.jpg"),
                            Reference.VideoDriver.GetTexture(textureDirectory + "sea_sky_BK.jpg"),
                        },
                        -1
                        );

                    for (int i = 0; i < node.MaterialCount; i++)
                    {
#if YK_VIDEO_WIREFRAME
                        node.GetMaterial(i).Wireframe = true;
#endif
                    }
                    

                    // Unuse gold sun and silver moon.
                    sunNode.Visible = false;
                    moonNode.Visible = false;
                }

                // Create  light
                sunLight = new DirectionalLight(Reference.Viewer, parentNode, "SunLight");
                sunLight.Load(Colorf.White, Colorf.Black, Colorf.Black);
            }
            else
            {
                switch (Reference.Viewer.SkyQuality)
                {
                    case Viewer.ShaderLevelType.High:
                    case Viewer.ShaderLevelType.Middle:
                        Reference.Viewer.SkyQuality = Viewer.ShaderLevelType.Low;
                        res = Load();
                        break;

                    case Viewer.ShaderLevelType.Low:
                        Reference.Viewer.ShowMessageBox(DialogText.DisabledSkyShader);
                        break;
                }
            }

            return res;
        }

        public override void Cleanup()
        {
            base.Cleanup();
        }

        public override void ShaderEvent(IrrlichtNETCP.MaterialRendererServices services, int userData)
        {
            if (updateOnce)
            {
                return;
            }
            else
            {
                updateOnce = true;
            }

            if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High)
            {
                Matrix4 worldViewProj = Reference.VideoDriver.GetTransform(TransformationState.Projection);
                worldViewProj *= Reference.VideoDriver.GetTransform(TransformationState.View);
                worldViewProj *= Reference.VideoDriver.GetTransform(TransformationState.World);
                services.SetVertexShaderConstant("WVPMatrix", worldViewProj.ToShader());
            }

            Vector3D camPos, camPosXZ;
            camPosXZ = camPos = new Vector3D(Reference.Viewer.Camera.Position.X, Reference.Viewer.Camera.Position.Y, Reference.Viewer.Camera.Position.Z);
            camPosXZ.Y = 0f;

            //// Sun position calculation
            SkyMaths.AltAzAngles sunAngles = SkyMaths.CalculateSunPosition(julianDay + (float)worldTime.TimeOfDay.TotalDays, LATITUDE);
            sunPosition = SkyMaths.MoveAroundPoint(camPosXZ, 12500f, sunAngles.azimuth, -sunAngles.altitude);
            if (sunNode != null)
                sunNode.Position = sunPosition * 0.4f;

            //// Moon position (approximate inverse of the sun, l0lz.)
            moonPosition = sunPosition;
            moonPosition.X *= -1.0f;
            moonPosition.Y *= -1.0f;
            moonPosition.Z *= -1.0f;
            moonPosition = moonPosition.Normalize();
            if (moonNode != null)
                moonNode.Position = moonPosition * 9000.0f;

            // Set the sun normalized vector and the sunTheta to the shader
            Vector3D sunNormedPos = new Vector3D();
            sunNormedPos = sunPosition - camPosXZ;
            sunNormedPos = sunNormedPos.Normalize();
            float sunTheta = SkyMaths.VectorToTheta(sunNormedPos);

            // Sun lightning direction
            Vector3D sunDirection = new Vector3D();
            sunDirection = sunPosition - camPosXZ;
            sunDirection = sunDirection.Normalize();

            // Stars and moon opacity
            moonIntensity = SkyMaths.Saturate(SkyMaths.Lerp(sunAngles.altitude, 0f, SUN_SET)) * 0.95f;
            moonIntensity += 0.05f;

            // A hack to kill the orange tones in the nightsky
            float darkness = 1f - (moonIntensity - 0.05f);

            // Calculate the constant matrices
            SkyMaths.xyYColor _zenithColors = SkyMaths.SkyZenithColor(turbidity, sunTheta);
            Vector3D _zenithColorsVector = new Vector3D(_zenithColors.x, _zenithColors.y, _zenithColors.Y);
            SkyMaths.xyYCoeffs _distribCoeffs = SkyMaths.DistributionCoefficients(turbidity);

            if (Reference.Viewer.SkyQuality == Viewer.ShaderLevelType.High)
            {
                services.SetVertexShaderConstant("SunTheta", sunTheta);
                services.SetVertexShaderConstant("SunVector", sunNormedPos.ToShader());
                services.SetVertexShaderConstant("NightDarkness", darkness);
                services.SetVertexShaderConstant("ZenithColor", _zenithColorsVector.ToShader());

                for (int i = 0; i < 5; i++)
                {
                    services.SetVertexShaderConstant("_xDistribCoeffs[" + i + "]", _distribCoeffs.x[i]);
                    services.SetVertexShaderConstant("_yDistribCoeffs[" + i + "]", _distribCoeffs.y[i]);
                    services.SetVertexShaderConstant("_YDistribCoeffs[" + i + "]", _distribCoeffs.Y[i]);
                }

                // Set the adaptative luminance and gamma corrections
                float gamma = 1f / (1.6f + (turbidity - 2f) * 0.1f);
                services.SetVertexShaderConstant("InvGammaCorrection", 1.5f * gamma);
                services.SetVertexShaderConstant("InvPowLumFactor", gamma);
                services.SetVertexShaderConstant("InvNegMaxLum", -1.25f / SkyMaths.MaximumLuminance(turbidity, sunTheta, _zenithColors, _distribCoeffs));

                float calcAlpha = ((12 - 1) * 60 * 60 + (60 - 1) * 60 + 60) - ((worldTime.Hour - 1) * 60 * 60 + (worldTime.Minute - 1) * 60 + worldTime.Second);
                calcAlpha = 1 - Math.Abs(calcAlpha) / cloudChangeSec;
                calcAlpha = Util.Clamp<float>(calcAlpha, 0, 1);
                calcAlpha = calcAlpha * calcAlpha;

                float minAlpha = 0.05f;
                calcAlpha = (calcAlpha * 1.5f) + minAlpha;
                calcAlpha = Util.Clamp<float>(calcAlpha, 0, 1);

                services.SetPixelShaderConstant("CloudAlpha", calcAlpha);
                services.SetPixelShaderConstant("BlendingRate", 0);
            }

            // Clouds coloring
            float[] atmoCol = SkyMaths.AtmosphereColor(turbidity, sunTheta, _zenithColors, _distribCoeffs);
            Vector3D atmoColVec = new Vector3D(atmoCol[0], atmoCol[1], atmoCol[2]);
            float dayState = SkyMaths.Saturate(SkyMaths.Lerp(sunAngles.altitude, (float)(Math.PI * 1f / (6f - turbidity / 2f)), SUN_RISE));

            // Sun lightning intensity
            float sunIntensity = SkyMaths.Saturate(SkyMaths.Lerp(sunAngles.altitude, SUN_SET, SUN_RISE));
            
            base.ShaderEvent(services, userData);
        }
    }
}
