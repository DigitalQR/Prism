using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Behaviour.Default
{
	public class EnumGenerateBehaviour : GlobalReflectionBehaviour<EnumTypeElement>
	{
		public EnumGenerateBehaviour()
			: base(BehaviourTarget.Enumurator, BehaviourController.StructureGenerationPriority)
		{
		}

		public override void RunBehaviour(EnumTypeElement target)
		{
			GenerateIncludeContent(target);
			GenerateInlineContent(target);
			GenerateSourceContent(target);
		}

		private void GenerateIncludeContent(EnumTypeElement target)
		{
			GenerationUtils.AppendChildIncludeContent(target);

			target.AppendIncludeContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}
class EnumInfo_{target.UniqueName} : public Prism::Enum
{{
private:
	static EnumInfo_{target.UniqueName} s_AssemblyInstance;
	EnumInfo_{target.UniqueName}();
}};
{GenerationUtils.GetNamespaceClose(target)}
#endif
");
		}

		private void GenerateInlineContent(EnumTypeElement target)
		{
			GenerationUtils.AppendChildInlineContent(target);
		}

		private void GenerateSourceContent(EnumTypeElement target)
		{
			GenerationUtils.AppendChildSourceContent(target);

			StringBuilder valueInstances = new StringBuilder();

			foreach (EnumValueElement elem in target.Values)
			{
				valueInstances.Append($@"
#if {elem.PreProcessorCondition}
			Prism::Enum::Value(PRISM_STR(""{elem.Name}""), static_cast<size_t>({target.Name}::{elem.Name})),
#endif
");
			}

			target.AppendSourceContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}
EnumInfo_{target.UniqueName} EnumInfo_{target.UniqueName}::s_AssemblyInstance;

EnumInfo_{target.UniqueName}::EnumInfo_{target.UniqueName}()
	: Prism::Enum(
		Prism::TypeId::Get<{target.Name}>(),
		PRISM_STR(""{string.Join(".", target.Namespace)}""), 
		PRISM_STR(""{target.Name}""), 
		PRISM_DEVSTR(R""({target.Documentation})""),
		sizeof({target.Name}),
		{{{GenerationUtils.GetDataAttributeInstancesString(target)}}},
		{{{valueInstances.ToString()}
		}}
	)
	{{}}
{GenerationUtils.GetNamespaceClose(target)}
#endif
");
		}
	}
}
