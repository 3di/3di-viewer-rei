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
using System.Xml;

namespace IrrParseLib
{
    public class IrrAnimationKeys
    {
        public List<KeyframeSet> Keys = new List<KeyframeSet>();

        public IrrAnimationKeys()
        {
        }

        public void Load(XmlTextReader reader)
        {
            while (reader.Read())
            {
                // end node.
                if (reader.Name == "attributes" && (reader.NodeType == XmlNodeType.EndElement))
                {
                    break;
                }

                // read attributes.
                else if (reader.Name == "animation")
                {
                    KeyframeSet key = LoadAnimationKey(reader);
                    Keys.Add(key);
                }
            }
        }

        private KeyframeSet LoadAnimationKey(XmlTextReader reader)
        {
            KeyframeSet key = new KeyframeSet();

            while (reader.Read())
            {
                // end node.
                if (reader.Name == "animation" && (reader.NodeType == XmlNodeType.EndElement))
                {
                    break;
                }

                // read attributes.
                else if (reader.AttributeCount > 0)
                {
                    if (reader.GetAttribute("name") == "Name")
                    {
                        key.Name = reader.GetAttribute("value");
                    }

                    else if (reader.GetAttribute("name") == "Speed")
                    {
                        key.AnimationSpeed = int.Parse(reader.GetAttribute("value"));
                    }

                    else if (reader.GetAttribute("name") == "Start")
                    {
                        key.StartFrame = int.Parse(reader.GetAttribute("value"));
                    }

                    else if (reader.GetAttribute("name") == "End")
                    {
                        key.EndFrame = int.Parse(reader.GetAttribute("value"));
                    }
                }
            }

            return key;
        }
    }
}
