using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class EnumTypeInfo
	{
		public string m_Name;
		public string m_ScopeStructure;
		public string m_BaseType;

		public override string ToString()
		{
			List<string> parts = new List<string>();

			parts.Add("enum");

			if (m_ScopeStructure != null)
				parts.Add(m_ScopeStructure);

			parts.Add(m_Name);

			if (m_BaseType != null)
			{
				parts.Add(":");
				parts.Add(m_BaseType);
			}

			return string.Join(" ", parts);
		}
	}

	public class EnumValueInfo
	{
		public string m_Name;
		public string m_DefaultValue;

		public override string ToString()
		{
			return m_Name;
		}
	}
}
