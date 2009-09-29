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
 * $Author: jendave $
 * $Date: 2006-07-26 01:56:49 +0900 (Wed, 26 Jul 2006) $
 * $Revision: 132 $
 */
#endregion

using System;
using System.Xml;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Utilities;

namespace Prebuild.Core.Nodes
{
	/// <summary>
	/// 
	/// </summary>
	[DataNode("Reference")]
	public class ReferenceNode : DataNode, IComparable
	{
		#region Fields

		private string m_Name = "unknown";
		private string m_Path;
		private string m_LocalCopy;
		private string m_Version;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the name.
		/// </summary>
		/// <value>The name.</value>
		public string Name
		{
			get
			{
				return m_Name;
			}
		}

		/// <summary>
		/// Gets the path.
		/// </summary>
		/// <value>The path.</value>
		public string Path
		{
			get
			{
				return m_Path;
			}
		}

		/// <summary>
		/// Gets a value indicating whether [local copy specified].
		/// </summary>
		/// <value><c>true</c> if [local copy specified]; otherwise, <c>false</c>.</value>
		public bool LocalCopySpecified 
		{
			get 
			{
				return ( m_LocalCopy != null && m_LocalCopy.Length == 0);
			}
		}

		/// <summary>
		/// Gets a value indicating whether [local copy].
		/// </summary>
		/// <value><c>true</c> if [local copy]; otherwise, <c>false</c>.</value>
		public bool LocalCopy
		{
			get
			{
				if( m_LocalCopy == null)
				{
					return false;
				}
				return bool.Parse(m_LocalCopy);
			}
		}

		/// <summary>
		/// Gets the version.
		/// </summary>
		/// <value>The version.</value>
		public string Version
		{
			get
			{
				return m_Version;
			}
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Parses the specified node.
		/// </summary>
		/// <param name="node">The node.</param>
		public override void Parse(XmlNode node)
		{
			m_Name = Helper.AttributeValue(node, "name", m_Name);
			m_Path = Helper.AttributeValue(node, "path", m_Path);
			m_LocalCopy = Helper.AttributeValue(node, "localCopy", m_LocalCopy);
			m_Version = Helper.AttributeValue(node, "version", m_Version);
		}

		#endregion

        #region IComparable Members

        public int CompareTo(object obj)
        {
            ReferenceNode that = (ReferenceNode)obj;
            return this.m_Name.CompareTo(that.m_Name);
        }

        #endregion
	}
}
