using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp.Data
{
	public class InheritanceInfo
	{
		public string m_Name;
		public string m_Accessor;
	}

	public class StructureInfo
	{
		public string m_Structure;
		public string m_Name;
		public InheritanceInfo[] m_Parents;
	}
}
