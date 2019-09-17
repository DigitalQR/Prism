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
	/// All reflected information found for a specific cpp variable
	/// </summary>
	public class VariableElement : ReflectionElementBase
	{
		private VariableInfo m_Details;
		private string m_Accessor;

		public VariableElement(ReflectionMetaData meta, string[] attributes, VariableInfo info, string accessor)
			: base(BehaviourTarget.Variable, attributes, meta)
		{
			m_Details = info;
			m_Accessor = accessor;
		}

		public override string Name => m_Details.m_Name;
		public VariableInfo Details { get => m_Details; }
		public string Accessor { get => m_Accessor; }

		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
