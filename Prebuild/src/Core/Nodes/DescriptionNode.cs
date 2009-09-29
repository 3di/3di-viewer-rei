#region BSD License
/*
Copyright (c) 2007 C.J. Adams-Collier (cjac@colliertech.org)

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
 * $Source $
 * $Author:  $
 * $Date:  $
 * $Revision:  $
 */
#endregion

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Xml;

using Prebuild.Core.Attributes;
using Prebuild.Core.Interfaces;
using Prebuild.Core.Utilities;

namespace Prebuild.Core.Nodes
{
	/// <summary>
	/// The object representing the /Prebuild/Solution/Project/Description element
	/// </summary>
	[DataNode("Description")]
	public class DescriptionNode : DataNode
	{
		#region Fields

		private string m_Value;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the description Value.
		/// </summary>
		/// <value>The description Value.</value>
		public string Value
		{
			get
			{
				return m_Value;
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
			if( node == null )
			{
				throw new ArgumentNullException("node");
			}

			m_Value = Helper.InterpolateForEnvironmentVariables(node.InnerText);
			if(m_Value == null)
			{
				m_Value = "";
			}

			m_Value = m_Value.Trim();
		}

		#endregion
	}
}
