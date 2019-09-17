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
	/// All reflected information found for a specific cpp function
	/// </summary>
	public class FunctionElement : ReflectionElementBase
	{
		private FunctionInfo m_Details;
		private string m_Accessor;

		public FunctionElement(ReflectionMetaData meta, string[] attributes, FunctionInfo info, string accessor)
			: base(BehaviourTarget.Function, attributes, meta)
		{
			m_Details = info;
			m_Accessor = accessor;
		}

		public override string Name => m_Details.m_Name;
		public override string UniqueName => (m_Details.m_IsConstructor ? "Construct_" : "") + base.UniqueName;
		public FunctionInfo Details { get => m_Details; }
		public string Accessor { get => m_Accessor; }

		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
