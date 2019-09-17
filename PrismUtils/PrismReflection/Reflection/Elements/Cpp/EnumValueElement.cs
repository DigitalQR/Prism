using Prism.Reflection.Behaviour;
using Prism.Reflection.Elements.Cpp.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection.Elements.Cpp
{
	/// <summary>
	/// All reflected information found for a specific cpp enum type
	/// </summary>
	public class EnumValueElement : ReflectionElementBase
	{
		private EnumValueInfo m_Details;

		public EnumValueElement(ReflectionMetaData meta, string[] attributes, EnumValueInfo info)
			: base(BehaviourTarget.EnumuratorValue, attributes, meta)
		{
			m_Details = info;
		}

		public override string Name => m_Details.m_Name;
		public EnumValueInfo Details { get => m_Details; }

		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
