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
	/// All reflected information found for a specific cpp structure 
	/// *class and struct
	/// </summary>
	public class StructureElement : ReflectionElementBase
	{
		private StructureInfo m_Details;

		private List<FunctionElement> m_Functions;
		private List<VariableElement> m_Variables;

		public StructureElement(ReflectionMetaData meta, string[] attributes, StructureInfo structure)
			: base(BehaviourTarget.Structure, attributes, meta)
		{
			m_Details = structure;
			m_Functions = new List<FunctionElement>();
			m_Variables = new List<VariableElement>();
		}

		public override string Name => m_Details.m_Name;
		public StructureInfo Details { get => m_Details; }
		public IEnumerable<FunctionElement> Functions { get => m_Functions; }
		public IEnumerable<VariableElement> Variables { get => m_Variables; }
		public override IEnumerable<IReflectionElement> ChildElements { get => m_Functions.AsEnumerable<IReflectionElement>().Union(m_Variables); }

		public void AddFunction(FunctionElement element)
		{
			m_Functions.Add(element);
			element.ParentElement = this;
		}

		public void AddVariable(VariableElement element)
		{
			m_Variables.Add(element);
			element.ParentElement = this;
		}

		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
