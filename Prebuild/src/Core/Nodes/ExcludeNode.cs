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
using System.Xml;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Utilities;

namespace Prebuild.Core.Nodes
{
	/// <summary>
	/// 
	/// </summary>
	[DataNode("Exclude")]
	public class ExcludeNode : DataNode
	{
		#region Fields

        private string m_Pattern = "";

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
				return m_Pattern;
			}
		}

        /// <summary>
        /// Gets the pattern.
        /// </summary>
        /// <value>The pattern.</value>
        public string Pattern
        {
            get
            {
                return m_Pattern;
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
            m_Pattern = Helper.AttributeValue( node, "name", m_Pattern );
            m_Pattern = Helper.AttributeValue(node, "pattern", m_Pattern );
		}

		#endregion
	}
}
