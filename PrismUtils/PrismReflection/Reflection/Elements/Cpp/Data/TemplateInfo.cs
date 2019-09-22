using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class TemplateInfo
	{
		public TemplateParamInfo[] m_Params;

		public override string ToString()
		{
			return "template<" + string.Join(",", m_Params.Select((p) => p.ToString())) + ">";
		}
	}

	public class TemplateParamInfo
	{
		public string m_Prefix;
		public string m_Name;
		public string m_DefaultValue;

		public override string ToString()
		{
			List<string> parts = new List<string>();

			parts.Add(m_Prefix);
			parts.Add(m_Name);

			if (m_DefaultValue != null)
			{
				parts.Add("=");
				parts.Add(m_DefaultValue);
			}
			
			return string.Join(" ", parts);
		}
	}
}
