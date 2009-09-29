#region BSD License
/*
Copyright (c) 2004-2005 Matthew Holmes (matthew@wildfiregames.com), Dan Moorehead (dan05a@gmail.com)

Redistribution and use in source and binary forms, with or without modification, are permitted
provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions 
  and the following disclaimer. 
* Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
  and the following disclaimer in the documentation and/or other materials provided with the 
  distribution. 
* The name of the author may not be used to endorse or promote products derived from this software 
  without specific prior written permission. 

THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, 
BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS
OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY
OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING
IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

#region CVS Information
/*
 * $Source$
 * $Author: borrillis $
 * $Date: 2007-05-25 01:03:16 +0900 (Fri, 25 May 2007) $
 * $Revision: 243 $
 */
#endregion

using System;
using System.Collections.Specialized;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Utilities;
using System.Collections;

namespace Prebuild.Core.Nodes
{
	/// <summary>
	/// 
	/// </summary>
	[DataNode("Match")]
	public class MatchNode : DataNode
	{
		#region Fields

		private StringCollection m_Files;
		private Regex m_Regex;
		private BuildAction m_BuildAction = BuildAction.Compile;
		private SubType m_SubType = SubType.Code;
		string m_ResourceName = "";
		private CopyToOutput m_CopyToOutput;
		private bool m_Link;
		private string m_LinkPath;
        private bool m_PreservePath;
        private ArrayList m_Exclusions;

		#endregion

		#region Constructors

		/// <summary>
		/// 
		/// </summary>
		public MatchNode()
		{
			m_Files = new StringCollection();
            m_Exclusions = new ArrayList();
		}

		#endregion

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public StringCollection Files
		{
			get
			{
				return m_Files;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public BuildAction BuildAction
		{
			get
			{
				return m_BuildAction;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public SubType SubType
		{
			get
			{
				return m_SubType;
			}
		}

		public CopyToOutput CopyToOutput
		{
			get
			{
				return this.m_CopyToOutput;
			}
		}

		public bool IsLink
		{
			get
			{
				return this.m_Link;
			}
		}

		public string LinkPath
		{
			get
			{
				return this.m_LinkPath;
			}
		}
		/// <summary>
		/// 
		/// </summary>
		public string ResourceName
		{
			get
			{
				return m_ResourceName;
			}
		}

        public bool PreservePath
        {
            get
            {
                return m_PreservePath;
            }
        }

		#endregion

		#region Private Methods

		/// <summary>
		/// Recurses the directories.
		/// </summary>
		/// <param name="path">The path.</param>
		/// <param name="pattern">The pattern.</param>
		/// <param name="recurse">if set to <c>true</c> [recurse].</param>
		/// <param name="useRegex">if set to <c>true</c> [use regex].</param>
		private void RecurseDirectories(string path, string pattern, bool recurse, bool useRegex, ArrayList exclusions)
		{
			Match match;
            Boolean excludeFile;
			try
			{
				string[] files;

				if(!useRegex)
				{
					files = Directory.GetFiles(path, pattern);
					if(files != null)
					{
						string fileTemp;
						foreach (string file in files)
						{
                            excludeFile = false;
							if (file.Substring(0,2) == "./" || file.Substring(0,2) == ".\\")
							{
								fileTemp = file.Substring(2);
							}
							else
							{
								fileTemp = file;
							}

                            // Check all excludions and set flag if there are any hits.
                            foreach ( ExcludeNode exclude in exclusions )
                            {
                                Regex exRegEx = new Regex( exclude.Pattern );
                                match = exRegEx.Match( file );
                                excludeFile |= match.Success;
                            }

                            if ( !excludeFile )
                            {
                                m_Files.Add( fileTemp );
                            }

						}
					}
					else
					{
						return;
					}
				}
				else
				{
					files = Directory.GetFiles(path);
					foreach(string file in files)
					{
                        excludeFile = false;

						match = m_Regex.Match(file);
						if(match.Success)
						{
                            // Check all excludions and set flag if there are any hits.
                            foreach ( ExcludeNode exclude in exclusions )
                            {
                                Regex exRegEx = new Regex( exclude.Pattern );
                                match = exRegEx.Match( file );
                                excludeFile |= !match.Success;
                            }

                            if ( !excludeFile )
                            {
                                m_Files.Add( file );
                            }
						}
					}
				}
                
				if(recurse)
				{
					string[] dirs = Directory.GetDirectories(path);
					if(dirs != null && dirs.Length > 0)
					{
						foreach(string str in dirs)
						{
							RecurseDirectories(Helper.NormalizePath(str), pattern, recurse, useRegex, exclusions);
						}
					}
				}
			}
			catch(DirectoryNotFoundException)
			{
				return;
			}
			catch(ArgumentException)
			{
				return;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// 
		/// </summary>
		/// <param name="node"></param>
		public override void Parse(XmlNode node)
		{
			if( node == null )
			{
				throw new ArgumentNullException("node");
			}
			string path = Helper.AttributeValue(node, "path", ".");
			string pattern = Helper.AttributeValue(node, "pattern", "*");
			bool recurse = (bool)Helper.TranslateValue(typeof(bool), Helper.AttributeValue(node, "recurse", "false"));
			bool useRegex = (bool)Helper.TranslateValue(typeof(bool), Helper.AttributeValue(node, "useRegex", "false"));
			m_BuildAction = (BuildAction)Enum.Parse(typeof(BuildAction), 
				Helper.AttributeValue(node, "buildAction", m_BuildAction.ToString()));
			m_SubType = (SubType)Enum.Parse(typeof(SubType), 
				Helper.AttributeValue(node, "subType", m_SubType.ToString()));
			m_ResourceName = Helper.AttributeValue(node, "resourceName", m_ResourceName.ToString());
			this.m_CopyToOutput = (CopyToOutput) Enum.Parse(typeof(CopyToOutput), Helper.AttributeValue(node, "copyToOutput", this.m_CopyToOutput.ToString()));
			this.m_Link = bool.Parse(Helper.AttributeValue(node, "link", bool.FalseString));
			if ( this.m_Link == true )
			{
				this.m_LinkPath = Helper.AttributeValue( node, "linkPath", string.Empty );
			}
            this.m_PreservePath = bool.Parse( Helper.AttributeValue( node, "preservePath", bool.FalseString ) );


			if(path != null && path.Length == 0)
			{
				path = ".";//use current directory
			}
			//throw new WarningException("Match must have a 'path' attribute");

			if(pattern == null)
			{
				throw new WarningException("Match must have a 'pattern' attribute");
			}

			path = Helper.NormalizePath(path);
			if(!Directory.Exists(path))
			{
				throw new WarningException("Match path does not exist: {0}", path);
			}

			try
			{
				if(useRegex)
				{
					m_Regex = new Regex(pattern);
				}
			}
			catch(ArgumentException ex)
			{
				throw new WarningException("Could not compile regex pattern: {0}", ex.Message);
			}


			foreach(XmlNode child in node.ChildNodes)
			{
				IDataNode dataNode = Kernel.Instance.ParseNode(child, this);
				if(dataNode is ExcludeNode)
				{
					ExcludeNode excludeNode = (ExcludeNode)dataNode;
                    m_Exclusions.Add( dataNode );
				}
			}

            RecurseDirectories( path, pattern, recurse, useRegex, m_Exclusions );

			if(m_Files.Count < 1)
			{
				throw new WarningException("Match returned no files: {0}{1}", Helper.EndPath(path), pattern);
			}
			m_Regex = null;
		}

		#endregion
	}
}
