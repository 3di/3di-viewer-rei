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
using System.IO;
using System.Xml;

namespace IrrParseLib
{
    public class IrrParser
    {
        // test file.
        //public const string IRR_FILE = "test_man30";
        public const string IRR_FILE = "irr_man";

        private List<IrrDatas> datas = null;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_stream">irrlife stream.</param>
        public IrrParser(Stream stream)
        {
            Run(stream, null);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_stream">irrlife stream.</param>
        /// <param name="_stream">animation's xml stream.</param>
        public IrrParser(Stream stream, Stream animStream)
        {
            Run(stream, animStream);
        }

        /// <summary>
        /// Load xml.
        /// </summary>
        /// <param name="_stream">irrlife stream.</param>
        /// <param name="_stream">animation's xml stream.</param>
        private void Run(Stream stream, Stream animStream)
        {
            using (XmlTextReader reader = new XmlTextReader(stream))
            {
                XmlTextReader animReader = null;
                if (animStream != null)
                    animReader = new XmlTextReader(animStream);

                while (reader.Read())
                {
                    if (reader.NodeType == XmlNodeType.Element)
                    {
                        // root
                        if (reader.Name == "attributes")
                        {
                        }

                        // node data.
                        else if (reader.Name == "node")
                        {
                            if (datas == null)
                                datas = new List<IrrDatas>();

                            IrrDatas data = new IrrDatas(reader, true);
                            if (animReader != null)
                                data.CreateAnimationKey(animReader);

                            datas.Add(data);
                        }
                    }
                }

                if (animReader != null)
                    animReader.Close();

                reader.Close();
            }
        }

        /// <summary>
        /// Mesh and materials list.
        /// </summary>
        public List<IrrDatas> Datas
        {
            get { return datas; }
        }
    }
}
