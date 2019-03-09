using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Tokens
{
	public class AttributeCollection
	{
		private List<AttributeData> m_Attributes;

		/// <summary>
		/// All the attributes that this collection currently contains
		/// </summary>
		public IReadOnlyList<AttributeData> Attributes
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


		private class RawAttribute
		{
			public string Name = "";
			public string Params = "";

			public void Trim()
			{
				Name = Name.Trim();
				Params = Params.Trim();
			}
		}

		public AttributeCollection(string rawParams)
		{
			m_Attributes = new List<AttributeData>();

			// Parse raw params by doing context aware split
			if (!string.IsNullOrWhiteSpace(rawParams))
			{
				foreach (RawAttribute raw in ParseRawParams(rawParams))
				{
					AddAttribute(new AttributeData(raw.Name, raw.Params));
				}
				return;
			}
		}

		public void AddAttribute(AttributeData data)
		{
			m_Attributes.Add(data);
		}

		private List<RawAttribute> ParseRawParams(string rawParams)
		{
			List<RawAttribute> rawAttributes = new List<RawAttribute>();
			RawAttribute currentParam = new RawAttribute();
			char lastChar = '\0';
			bool readingName = true;
			bool readingString = false;
			int depth = 0;

			foreach (char c in rawParams)
			{
				if (readingName)
				{
					// Found attribute with no params
					if (c == ',')
					{
						if (!string.IsNullOrWhiteSpace(currentParam.Name))
						{
							currentParam.Trim();
							rawAttributes.Add(currentParam);
						}
						currentParam = new RawAttribute();
					}
					// Found begining of params
					else if (c == '(')
					{
						readingName = false;
						depth = 1;
					}
					else
					{
						currentParam.Name += c;
					}
				}
				else
				{
					if (c == '"' && lastChar != '\\')
						readingString = !readingString;

					if (!readingString)
					{
						if (c == '(')
						{
							++depth;
						}
						else if (c == ')')
						{
							--depth;
						}
						else if (c == ',')
						{
							// Found attribute with params
							if (depth == 0)
							{
								if (!string.IsNullOrWhiteSpace(currentParam.Name))
								{
									currentParam.Trim();
									rawAttributes.Add(currentParam);
								}

								currentParam = new RawAttribute();
								readingName = true;
							}
							else
							{
								currentParam.Params += c;
							}
						}
						else
						{
							currentParam.Params += c;
						}
					}
					else
					{
						currentParam.Params += c;
					}
				}

				lastChar = c;
			}

			// Add last attribute
			if (!string.IsNullOrWhiteSpace(currentParam.Name))
			{
				currentParam.Trim();
				rawAttributes.Add(currentParam);
			}

			return rawAttributes;
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
				builder.Replace("$(" + prefix + "Attribute" + suffix + "[" + i + "].Name)", m_Attributes[i].Name);
				builder.Replace("$(" + prefix + "Attribute" + suffix + "[" + i + "].Params)", m_Attributes[i].Params);
				++i;
			}
			
			return builder;
		}
	}
}
