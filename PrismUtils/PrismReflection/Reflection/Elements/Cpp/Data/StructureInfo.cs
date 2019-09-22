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

		public static StructureInfo Generate(
			string name,
			string structure = "class",
			InheritanceInfo[] parents = null
			)
		{
			StructureInfo info = new StructureInfo();
			info.m_Name = name;
			info.m_Structure = structure;
			info.m_Parents = parents;
			
			if (info.m_Parents == null)
				info.m_Parents = new InheritanceInfo[0];

			return info;
		}

	}
}
