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
using System.Drawing;
using System.Xml;

namespace IrrParseLib
{
    public class IrrMaterial
    {
        public IrrMaterial(XmlTextReader reader, string dumpDirectory)
        {
            while (reader.Read())
            {
                // end node.
                if (reader.Name == "attributes" && (reader.NodeType == XmlNodeType.EndElement))
                {
                    break;
                }

                // read attributes.
                else if (reader.AttributeCount > 0)
                {
                    if (reader.GetAttribute("name") == "Type")
                    {
                        Type = MaterialType.Solid;

                        switch (reader.GetAttribute("value"))
                        {
                            case "solid":
                                Type = MaterialType.Solid;
                                break;

                            case "solid_2layer":
                                Type = MaterialType.Solid2Layer;
                                break;

                            case "lightmap":
                                Type = MaterialType.Lightmap;
                                break;

                            case "ligthmap_add":
                                Type = MaterialType.LightmapAdd;
                                break;

                            case "ligthmap_m2":
                                Type = MaterialType.LightmapM2;
                                break;

                            case "ligthmap_m4":
                                Type = MaterialType.LightmapM4;
                                break;

                            case "ligthmap_light":
                                Type = MaterialType.LightmapLighting;
                                break;

                            case "ligthmap_light_m2":
                                Type = MaterialType.LightmapLightingM2;
                                break;

                            case "ligthmap_light_m4":
                                Type = MaterialType.LightmapLightingM4;
                                break;

                            case "detail_map":
                                Type = MaterialType.DetailMap;
                                break;

                            case "sphere_map":
                                Type = MaterialType.SphereMap;
                                break;

                            case "reflection_2layer":
                                Type = MaterialType.Reflection2Layer;
                                break;

                            case "trans_add":
                                Type = MaterialType.TransparentAddColor;
                                break;

                            case "trans_alphach":
                                Type = MaterialType.TransparentAlphaChannel;
                                break;

                            case "trans_alphach_ref":
                                Type = MaterialType.TransparentAlphaChannelRef;
                                break;

                            case "trans_reflection_2layer":
                                Type = MaterialType.TransparentReflection2Layer;
                                break;

                            case "normalmap_solid":
                                Type = MaterialType.NormalMapSolid;
                                break;

                            case "normalmap_trans_add":
                                Type = MaterialType.NormalMapTransparentAddColor;
                                break;

                            case "normalmap_trans_vertexalpha":
                                Type = MaterialType.NormalMapTransparentVertexAlpha;
                                break;

                            case "parallaxmap_solid":
                                Type = MaterialType.ParallaxMapSolid;
                                break;

                            case "parallaxmap_trans_add":
                                Type = MaterialType.ParallaxMapTransparentAddColor;
                                break;

                            case "parallaxmap_trans_vertexalpha":
                                Type = MaterialType.ParallaxMapTransparentVertexAlpha;
                                break;

                            case "onetexture_blend":
                                Type = MaterialType.OneTextureBlend;
                                break;
                        }
                    }

                    else if (reader.GetAttribute("name") == "Ambient")
                    {
                        Ambient = Color.Red;
                    }

                    else if (reader.GetAttribute("name") == "Diffuse")
                    {
                        Diffuse = Color.Red;
                    }

                    else if (reader.GetAttribute("name") == "Emissive")
                    {
                        Emissive = Color.Red;
                    }

                    else if (reader.GetAttribute("name") == "Specular")
                    {
                        Specular = Color.Red;
                    }

                    else if (reader.GetAttribute("name") == "Shininess")
                    {
                        Shininess = 3.0f;
                    }

                    else if (reader.GetAttribute("name") == "Param1")
                    {
                        Param1 = 3.0f;
                    }

                    else if (reader.GetAttribute("name") == "Param2")
                    {
                        Param2 = 3.0f;
                    }

                    else if (reader.GetAttribute("name") == "Texture1")
                    {
                        Texture1 = reader.GetAttribute("value").Replace(dumpDirectory, "");
                    }

                    else if (reader.GetAttribute("name") == "Texture2")
                    {
                        Texture2 = reader.GetAttribute("value").Replace(dumpDirectory, "");
                    }

                    else if (reader.GetAttribute("name") == "Texture3")
                    {
                        Texture3 = reader.GetAttribute("value").Replace(dumpDirectory, "");
                    }

                    else if (reader.GetAttribute("name") == "Texture4")
                    {
                        Texture4 = reader.GetAttribute("value").Replace(dumpDirectory, "");
                    }

                    else if (reader.GetAttribute("name") == "Wireframe")
                    {
                        Wireframe = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "GouraudShading")
                    {
                        GouraudShading = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "Lighting")
                    {
                        Lighting = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "ZWriteEnable")
                    {
                        ZWriteEnable = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "ZBuffer")
                    {
                        ZBuffer = int.Parse(reader.GetAttribute("value"));
                    }

                    else if (reader.GetAttribute("name") == "BackfaceCulling")
                    {
                        BackfaceCulling = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "FogEnable")
                    {
                        FogEnable = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "NormalizeNormals")
                    {
                        NormalizeNormals = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "BilinearFilter1")
                    {
                        BilinearFilter1 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "BilinearFilter2")
                    {
                        BilinearFilter2 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "BilinearFilter3")
                    {
                        BilinearFilter3 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "BilinearFilter4")
                    {
                        BilinearFilter4 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "TrilinearFilter1")
                    {
                        TrilinearFilter1 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "TrilinearFilter2")
                    {
                        TrilinearFilter2 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "TrilinearFilter3")
                    {
                        TrilinearFilter3 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "TrilinearFilter4")
                    {
                        TrilinearFilter4 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "AnisotropicFilter1")
                    {
                        AnisotropicFilter1 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "AnisotropicFilter2")
                    {
                        AnisotropicFilter2 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "AnisotropicFilter3")
                    {
                        AnisotropicFilter3 = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "AnisotropicFilter4")
                    {
                        AnisotropicFilter4 = (reader.GetAttribute("value") == "true" ? true : false);
                    }
                }
            }
        }

        public enum MaterialType
        {
            Solid = 0,
            Solid2Layer = 1,
            Lightmap = 2,
            LightmapAdd = 3,
            LightmapM2 = 4,
            LightmapM4 = 5,
            LightmapLighting = 6,
            LightmapLightingM2 = 7,
            LightmapLightingM4 = 8,
            DetailMap = 9,
            SphereMap = 10,
            Reflection2Layer = 11,
            TransparentAddColor = 12,
            TransparentAlphaChannel = 13,
            TransparentAlphaChannelRef = 14,
            TransparentVertexAlpha = 15,
            TransparentReflection2Layer = 16,
            NormalMapSolid = 17,
            NormalMapTransparentAddColor = 18,
            NormalMapTransparentVertexAlpha = 19,
            ParallaxMapSolid = 20,
            ParallaxMapTransparentAddColor = 21,
            ParallaxMapTransparentVertexAlpha = 22,
            OneTextureBlend = 23,
        };

        public MaterialType Type = MaterialType.Solid;

        public Color Ambient = Color.White;
        public Color Diffuse = Color.White;
        public Color Emissive = Color.White;
        public Color Specular = Color.White;
        public float Shininess = 0;

        public float Param1 = 0;
        public float Param2 = 0;

        public String Texture1 = string.Empty;
        public String Texture2 = string.Empty;
        public String Texture3 = string.Empty;
        public String Texture4 = string.Empty;

        public bool Wireframe = false;
        public bool GouraudShading = true;
        public bool Lighting = false;
        public bool ZWriteEnable = false;
        public int ZBuffer = 1;
        public bool BackfaceCulling = true;
        public bool FogEnable = true;
        public bool NormalizeNormals = true;
        public bool BilinearFilter1 = true;
        public bool BilinearFilter2 = true;
        public bool BilinearFilter3 = true;
        public bool BilinearFilter4 = true;
        public bool TrilinearFilter1 = false;
        public bool TrilinearFilter2 = false;
        public bool TrilinearFilter3 = false;
        public bool TrilinearFilter4 = false;
        public bool AnisotropicFilter1 = false;
        public bool AnisotropicFilter2 = false;
        public bool AnisotropicFilter3 = false;
        public bool AnisotropicFilter4 = false;

        public enum ClampType
        {
            Repeat,
        };
        public ClampType TextureWrap1 = ClampType.Repeat;
        public ClampType TextureWrap2 = ClampType.Repeat;
        public ClampType TextureWrap3 = ClampType.Repeat;
        public ClampType TextureWrap4 = ClampType.Repeat;
    }
}
