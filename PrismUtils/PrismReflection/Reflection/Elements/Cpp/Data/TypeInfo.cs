using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class TypeInfo
	{
		public string m_Name;
		public string m_ForwardDeclaredType;
		public bool m_IsConst;
		public bool m_IsPointer;
		public bool m_IsReference;

		public bool IsVoid()
		{
			return !m_IsPointer && m_Name == "void";
		}

		public override string ToString()
		{
			List<string> parts = new List<string>();
			
			if(m_ForwardDeclaredType != null)
				parts.Add(m_ForwardDeclaredType);

			parts.Add(m_Name);

			if(m_IsConst)
				parts.Add("const");

			string name = string.Join(" ", parts);

			if (m_IsPointer)
				name += '*';
			if (m_IsReference)
				name += '&';

			return name;
		}
	}
}
