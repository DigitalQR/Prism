using Prism.Reflection.Behaviour;
using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Prism.Reflection.Behaviour.Default
{
	public class StructureGenerateBehaviour : GlobalReflectionBehaviour<StructureElement>
	{
		public StructureGenerateBehaviour()
			: base(BehaviourTarget.Structure, BehaviourController.StructureGenerationPriority)
		{
		}

		public override void RunBehaviour(StructureElement target)
		{
			GenerateIncludeContent(target);
			GenerateInlineContent(target);
			GenerateSourceContent(target);
		}

		private void GenerateIncludeContent(StructureElement target)
		{
			GenerationUtils.AppendChildIncludeContent(target);
		}

		private void GenerateInlineContent(StructureElement target)
		{
			GenerationUtils.AppendChildInlineContent(target);

			target.AppendInlineContent($@"
public:
	virtual Prism::TypeInfo GetTypeInfo() const;
private:
	class ClassInfo : public Prism::Class
	{{
	private:
		static ClassInfo s_AssemblyInstance;
		ClassInfo();
		virtual Prism::ClassInfo GetParentClass(int index) const override;
		virtual size_t GetParentCount() const override;
	}};

");
		}

		private void GenerateSourceContent(StructureElement target)
		{
			GenerationUtils.AppendChildSourceContent(target);

			target.AppendSourceContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}
{target.Name}::ClassInfo {target.Name}::ClassInfo::s_AssemblyInstance;

{target.Name}::ClassInfo::ClassInfo()
	: Prism::Class(
		Prism::TypeId::Get<{target.Name}>(),
		PRISM_STR(""{string.Join(".", target.Namespace)}""),
		PRISM_STR(""{target.Name}""),
		PRISM_DEVSTR(R""({target.Documentation})""),
		sizeof({target.Name}),
		{{{GenerationUtils.GetDataAttributeInstancesString(target)}}},
		std::is_abstract<{target.Name}>::value,
		{{ {GetConstructorInstances(target)} }},
		{{ {GetMethodInstances(target)} }},
		{{ {GetPropertyInstances(target)} 
		}}
	)
	{{}}

Prism::TypeInfo {target.Name}::GetTypeInfo() const
{{
	return Prism::Assembly::Get().FindTypeOf<{target.Name}>();
}}

Prism::ClassInfo {target.Name}::ClassInfo::GetParentClass(int searchIndex) const
{{
	size_t index = 0;
{GetParentClassBody(target)}
	return nullptr;
}}

size_t {target.Name}::ClassInfo::GetParentCount() const
{{
	size_t count = 0;
{GetParentCountBody(target)}
	return count;
}}

{GenerationUtils.GetNamespaceClose(target)}
#endif
");
		}

		private static string GetConstructorInstances(StructureElement target)
		{
			return GetInstances("FunctionInfo_", target.Functions.Where((f) => f.Details.m_IsConstructor));
		}

		private static string GetMethodInstances(StructureElement target)
		{
			return GetInstances("FunctionInfo_", target.Functions.Where((f) => !f.Details.m_IsConstructor));
		}

		private static string GetPropertyInstances(StructureElement target)
		{
			return GetInstances("VariableInfo_", target.Variables);
		}

		private static string GetInstances(string prefix, IEnumerable<IReflectionElement> elements)
		{
			StringBuilder builder = new StringBuilder();

			foreach (IReflectionElement elem in elements)
			{
				builder.Append($@"
#if {elem.PreProcessorCondition}
			new {prefix}{elem.UniqueName}(),
#endif
");
			}

			return builder.ToString();
		}

		private static string GetParentClassBody(StructureElement target)
		{
			StringBuilder builder = new StringBuilder();

			foreach (var parentInfo in target.Details.m_Parents)
			{
				builder.Append($@"
__if_exists({parentInfo.m_Name}::ClassInfo)
{{
	if(index++ == searchIndex)
		return Prism::Assembly::Get().FindTypeOf<{parentInfo.m_Name}>();
}}
");
			}

			return builder.ToString();
		}

		private static string GetParentCountBody(StructureElement target)
		{

			StringBuilder builder = new StringBuilder();

			foreach (var parentInfo in target.Details.m_Parents)
			{
				builder.Append($@"
__if_exists({parentInfo.m_Name}::ClassInfo)
{{
	++count;
}}
");
			}

			return builder.ToString();
		}
	}
}
