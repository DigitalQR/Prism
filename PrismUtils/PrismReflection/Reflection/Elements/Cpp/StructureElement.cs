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
		private TemplateInfo m_TemplateInfo;

		private List<FunctionElement> m_Functions;
		private List<VariableElement> m_Variables;

		public StructureElement(ReflectionMetaData meta, string[] attributes, StructureInfo structure, TemplateInfo templateInfo)
			: base(BehaviourTarget.Structure, attributes, meta)
		{
			m_Details = structure;
			m_TemplateInfo = templateInfo;
			m_Functions = new List<FunctionElement>();
			m_Variables = new List<VariableElement>();
		}

		public override string Name => GetTemplateSafeName();
		public override string UniqueName { get => GetUntemplatedName() + "_" + Origin.LineNumber.ToString("X"); }
		public StructureInfo Details { get => m_Details; }
		public TemplateInfo TemplateDetails { get => m_TemplateInfo; }
		public bool IsTemplate { get => m_TemplateInfo != null; }
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

		public StructureElement GenerateStructure(StructureInfo info, IReflectionElement source, string[] attribs = null, bool inheritTemplate = true, bool reflect = true)
		{
			if (attribs == null)
				attribs = new string[0];

			StructureElement elem = new StructureElement(GetGeneratedMetaData(source), attribs, info, inheritTemplate ? m_TemplateInfo : null);
			elem.ParentElement = this;
			return elem;
		}

		public FunctionElement GenerateFunction(FunctionInfo info, IReflectionElement source, string accessor, string[] attribs = null, bool reflect = true)
		{
			if (attribs == null)
				attribs = new string[0];
			
			FunctionElement elem = new FunctionElement(GetGeneratedMetaData(source), attribs, info, accessor);
			elem.ParentElement = this;

			if (reflect)
				AddFunction(elem);

			return elem;
		}

		public VariableElement GenerateVariable(VariableInfo info, IReflectionElement source, string accessor, string[] attribs = null, bool reflect = true)
		{
			if (attribs == null)
				attribs = new string[0];

			VariableElement elem = new VariableElement(GetGeneratedMetaData(source), attribs, info, accessor);
			elem.ParentElement = this;

			if (reflect)
				AddVariable(elem);

			return elem;
		}

		private ReflectionMetaData GetGeneratedMetaData(IReflectionElement source)
		{
			ReflectionMetaData metaData = ReflectionMetaData.NonParsedInfo;
			metaData.m_Origin = new ElementOrigin(-source.UniqueName.GetHashCode());
			metaData.m_Namespace = source.Namespace;
			metaData.m_PreProcessorCondition = source.PreProcessorCondition;
			return metaData;
		}

		public string GetUntemplatedName()
		{
			return m_Details.m_Name;
		}

		private string GetTemplateSafeName()
		{
			return m_Details.m_Name + GetTemplatePostfix();
		}

		public string GetTemplatePostfix()
		{
			if (IsTemplate)
			{
				var tempParams = m_TemplateInfo.m_Params.Select((p) => p.m_Name);
				return "<" + string.Join(",", tempParams) + ">";
			}
			else
				return "";
		}


		public override string ToString()
		{
			return m_Details.ToString();
		}
	}
}
