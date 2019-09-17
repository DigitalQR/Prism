using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements
{
	public class AttributeCollection
	{
		private List<AttributeData> m_Attributes;

		/// <summary>
		/// All the attributes that this collection currently contains
		/// </summary>
		public IEnumerable<AttributeData> Attributes
		{
			get { return m_Attributes; }
		}

		/// <summary>
		/// All the attributes that this collection which are data attributes
		/// </summary>
		public IEnumerable<AttributeData> DataAttributes
		{
			get { return m_Attributes.Where(a => a.Status == AttributeStatus.Data); }
		}

		/// <summary>
		/// All the attributes that this collection which are data attributes
		/// </summary>
		public IEnumerable<AttributeData> BehaviourAttributes
		{
			get { return m_Attributes.Where(a => a.Status == AttributeStatus.Behaviour); }
		}
		
		public AttributeCollection(string[] rawAttribs)
		{
			m_Attributes = new List<AttributeData>();

			foreach (string rawAttrib in rawAttribs)
			{
				if (!string.IsNullOrWhiteSpace(rawAttrib))
				{
					string attribName = rawAttrib;
					string[] attribParams = new string[0];

					if (rawAttrib.EndsWith(")"))
					{
						int splitIndex = rawAttrib.IndexOf('(');
						if (splitIndex != -1)
						{
							attribName = rawAttrib.Substring(0, splitIndex);

							string rawParams = rawAttrib.Substring(splitIndex + 1);
							rawParams = rawParams.Substring(0, rawParams.Length - 1);
							attribParams = TokenUtils.SplitSyntax(rawParams, ",");
						}
					}

					AddAttribute(new AttributeData(attribName.Trim(), attribParams));
				}
			}
		}
		
		public void AddAttribute(AttributeData data)
		{
			m_Attributes.Add(data);
		}
		
		/// <summary>
		/// Expand any macros relating to this function (Missing macros will be left)
		/// *Will only expand for data attributes
		/// $(AttributeCount)
		/// $(Attribute[i].Name)
		/// $(Attribute[i].Params)
		/// 
		/// $(ReflectHash)
		/// $(ReflectedName)
		/// $(TokenOriginFile)
		/// $(TokenOriginLine)
		/// $(Documentation)
		/// $(Name)
		/// $(StructureType)
		/// $(InternalType)
		/// $(NamespaceList[,])			e.g. Namespace { A, B, C } = "A,B,C"
		/// $(NamespaceList[.])			e.g. Namespace { A, B, C } = "A.B.C"
		/// $(NamespaceList[::])		e.g. Namespace { A, B, C } = "A::B::C"
		/// $(ValueCount)
		/// $(Value[i].Name)
		/// </summary>
		public StringBuilder ExpandAttributeMacros(StringBuilder builder, string prefix = "", string suffix = "")
		{
			builder.Replace("$(" + prefix + "AttributeCount" + suffix + ")", DataAttributes.Count().ToString());

			int i = 0;
			foreach (AttributeData attrib in DataAttributes)
			{
				//builder.Replace("$(" + prefix + "Attribute" + suffix + "[" + i + "].Name)", m_Attributes[i].Name);
				//builder.Replace("$(" + prefix + "Attribute" + suffix + "[" + i + "].Params)", m_Attributes[i].Params);
				++i;
			}

			return builder;
		}
	}
}
