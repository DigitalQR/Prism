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
	public class EnumTypeElement : ReflectionElementBase
	{
		private EnumTypeInfo m_Details;

		private List<EnumValueElement> m_Values;

		public EnumTypeElement(ReflectionMetaData meta, string[] attributes, EnumTypeInfo info)
			: base(BehaviourTarget.Enumurator, attributes, meta)
		{
			m_Details = info;
			m_Values = new List<EnumValueElement>();
		}

		public override string Name => m_Details.m_Name;
		public EnumTypeInfo Details { get => m_Details; }
		public IEnumerable<EnumValueElement> Values { get => m_Values; }
		public override IEnumerable<IReflectionElement> ChildElements { get => m_Values; }
		
		public void AddValue(EnumValueElement element)
		{
			m_Values.Add(element);
			element.ParentElement = this;
		}

		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
