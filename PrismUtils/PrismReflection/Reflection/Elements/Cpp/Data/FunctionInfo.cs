using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class FunctionInfo
	{
		public string m_Name;
		public TypeInfo m_ReturnInfo;
		public VariableInfo[] m_Params;

		public bool m_IsConstructor;
		public bool m_IsDestructor;

		public bool m_IsConst;
		public bool m_IsStatic;
		public bool m_IsVirtual;
		public bool m_IsAbstract;
		public bool m_IsExplicit;
		public bool m_IsInlined;
		
		public override string ToString()
		{
			List<string> parts = new List<string>();

			if (m_IsStatic)
				parts.Add("static");
			if (m_IsInlined)
				parts.Add("inline");
			if (m_IsVirtual)
				parts.Add("virtual");

			if(!m_IsConstructor && !m_IsDestructor)
				parts.Add(m_ReturnInfo.ToString());
			
			if (m_IsExplicit)
				parts.Add("explicit");
			
			List<string> paramParts = new List<string>();
			foreach (VariableInfo varInfo in m_Params)
			{
				paramParts.Add(varInfo.ToString());
			}

			parts.Add(m_Name + "(" + string.Join(", ", paramParts) + ")");

			if (m_IsConst)
				parts.Add("const");

			return string.Join(" ", parts);;
		}
	}
}
