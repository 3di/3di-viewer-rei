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

using System.Xml;

namespace IrrParseLib
{
    public class IrrMesh
    {
        public string DumpDirectory = string.Empty;
        public IrrMeshParam Param = new IrrMeshParam(string.Empty);

        public IrrMesh()
        {
        }

        public void Load(XmlTextReader reader, bool isDumpDirectory)
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
                    if (reader.GetAttribute("name") == "Name")
                    {
                        Param.Name = reader.GetAttribute("value");
                    }

                    else if (reader.GetAttribute("name") == "Id")
                    {
                        Param.Id = int.Parse(reader.GetAttribute("value"));
                    }

                    else if (reader.GetAttribute("name") == "Position")
                    {
                        string[] values = reader.GetAttribute("value").Split(new char[]{','});
                        if (values.Length >= 3)
                        {
                            Param.Position[0] = float.Parse(values[0]);
                            Param.Position[1] = float.Parse(values[1]);
                            Param.Position[2] = float.Parse(values[2]);
                        }
                    }

                    else if (reader.GetAttribute("name") == "Rotation")
                    {
                        string[] values = reader.GetAttribute("value").Split(new char[]{','});
                        if (values.Length >= 3)
                        {
                            Param.Rotation[0] = float.Parse(values[0]);
                            Param.Rotation[1] = float.Parse(values[1]);
                            Param.Rotation[2] = float.Parse(values[2]);
                        }
                    }
                    
                    else if (reader.GetAttribute("name") == "Scale")
                    {
                        string[] values = reader.GetAttribute("value").Split(new char[]{','});
                        if (values.Length >= 3)
                        {
                            Param.Scale[0] = float.Parse(values[0]);
                            Param.Scale[1] = float.Parse(values[1]);
                            Param.Scale[2] = float.Parse(values[2]);
                        }
                    }

                    else if (reader.GetAttribute("name") == "Visible")
                    {
                        Param.Visible = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "DebugDataVisible")
                    {
                        Param.DebugDataVisible = int.Parse(reader.GetAttribute("value"));
                    }

                    else if (reader.GetAttribute("name") == "Mesh")
                    {
                        Param.Mesh = reader.GetAttribute("value");

                        // dump directory name from file path.
                        string dir = System.IO.Path.GetDirectoryName(Param.Mesh);

                        // "\\" to "/"
                        dir = dir.Replace("\\", "/") + "/";

                        Param.Mesh = Param.Mesh.Replace(dir, "");

                        if (isDumpDirectory)
                            DumpDirectory = dir;
                    }

                    else if (reader.GetAttribute("name") == "IsDebugObject")
                    {
                        Param.IsDebugObject = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "Looping")
                    {
                        Param.Looping = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "ReadOnlyMaterials")
                    {
                        Param.ReadOnlyMaterials = (reader.GetAttribute("value") == "true" ? true : false);
                    }

                    else if (reader.GetAttribute("name") == "FramesPerSecond")
                    {
                        Param.FramesPerSecond = float.Parse(reader.GetAttribute("value"));
                    }
                }
            }
        }
    }


    public struct IrrMeshParam
    {
        public string Name;
        public int Id;
        public float[] Position;
        public float[] Rotation;
        public float[] Scale;
        public bool Visible;

        public enum AutomaticCullincType
        {
            Box,
        }
        public AutomaticCullincType AutomaticCulling;

        public int DebugDataVisible;
        public string Mesh;
        public bool IsDebugObject;
        public bool Looping;
        public bool ReadOnlyMaterials;
        public float FramesPerSecond;

        public IrrMeshParam(string name)
        {
            Name = name;
            Id = 0;
            Position = new float[3];
            Rotation = new float[3];
            Scale = new float[3];
            Visible = true;

            AutomaticCulling = AutomaticCullincType.Box;

            DebugDataVisible = 0;
            Mesh = string.Empty;
            IsDebugObject = false;
            Looping = true;
            ReadOnlyMaterials = true;
            FramesPerSecond = 0.0f;
        }
    }
}
