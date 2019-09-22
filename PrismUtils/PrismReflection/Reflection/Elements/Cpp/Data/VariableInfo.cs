using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class VariableInfo
	{
		public string m_Name;
		public string m_DefaultValue;
		public TypeInfo m_TypeInfo;

		public bool m_IsStatic;
		public bool m_IsVolatile;
		public bool m_IsMutable;
		public bool m_IsInlined;

		public static VariableInfo Generate(
			string name,
			TypeInfo typeInfo,
			string defaultValue = null,
			bool isStatic = false,
			bool isInlined = false
			)
		{
			VariableInfo info = new VariableInfo();
			info.m_Name = name;
			info.m_TypeInfo = typeInfo;
			info.m_DefaultValue = defaultValue;
			info.m_IsStatic = isStatic;
			info.m_IsInlined = isInlined;
			
			return info;
		}

		public override string ToString()
		{
			List<string> parts = new List<string>();

			if (m_IsInlined)
				parts.Add("inline");
			if (m_IsStatic)
				parts.Add("static");
			if (m_IsVolatile)
				parts.Add("volatile");
			if (m_IsMutable)
				parts.Add("mutable");

			parts.Add(m_TypeInfo.ToString());
			parts.Add(m_Name);
			
			return string.Join(" ", parts);
		}

		public string ToStringWithDefaultValue()
		{
			if (m_DefaultValue != null)
				return ToString() + " = " + m_DefaultValue;
			else
				return ToString();
		}
	}
}
