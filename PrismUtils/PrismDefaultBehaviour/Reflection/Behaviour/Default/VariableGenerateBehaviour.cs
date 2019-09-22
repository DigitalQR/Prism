using Prism.Reflection.Behaviour;
using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Behaviour.Default
{
	public class VariableGenerateBehaviour : GlobalReflectionBehaviour<VariableElement>
	{
		public VariableGenerateBehaviour()
			: base(BehaviourTarget.Variable, BehaviourController.FieldGenerationPriority)
		{
		}
		
		public override void RunBehaviour(VariableElement target)
		{
			GenerateIncludeContent(target);
			GenerateInlineContent(target);
			GenerateSourceContent(target);
		}

		private void GenerateIncludeContent(VariableElement target)
		{

		}

		private void GenerateInlineContent(VariableElement target)
		{
			target.AppendInlineContent($@"
private:
	class VariableInfo_{target.UniqueName} : public Prism::Property
	{{
	public:
		VariableInfo_{target.UniqueName}();

		virtual Prism::TypeInfo GetParentInfo() const override;
		virtual Prism::TypeInfo GetTypeInfo() const override;

		virtual void Set(Prism::Object, Prism::Object) const override;
		virtual Prism::Object Get(Prism::Object) const override;
	}};
");
		}

		private void GenerateSourceContent(VariableElement target)
		{
			target.AppendSourceContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}
{target.ParentElement.Name}::VariableInfo_{target.UniqueName}::VariableInfo_{target.UniqueName}()
	: Prism::Property(
		PRISM_STR(""{target.Name}""),
		PRISM_DEVSTR(R""({target.Documentation})""),
		{{ {GenerationUtils.GetDataAttributeInstancesString(target)} }},
		Prism::Accessor::{char.ToUpper(target.Accessor[0]) + target.Accessor.Substring(1).ToLower()},
		{(target.Details.m_TypeInfo.m_IsPointer ? 1 : 0)},
		{(target.Details.m_IsStatic ? 1 : 0)},
		{(target.Details.m_TypeInfo.m_IsConst ? 1 : 0)}
	)
	{{}}

Prism::TypeInfo {target.ParentElement.Name}::VariableInfo_{target.UniqueName}::GetParentInfo() const
{{
	return Prism::Assembly::Get().FindTypeOf<{target.ParentElement.Name}>();
}}

Prism::TypeInfo {target.ParentElement.Name}::VariableInfo_{target.UniqueName}::GetTypeInfo() const
{{
	return Prism::Assembly::Get().FindTypeOf<{target.Details.m_TypeInfo.m_Name}>();
}}

void {target.ParentElement.Name}::VariableInfo_{target.UniqueName}::Set(Prism::Object target, Prism::Object value) const
{{
	{GetSetterBody(target)}
}}

Prism::Object {target.ParentElement.Name}::VariableInfo_{target.UniqueName}::Get(Prism::Object target) const
{{
	{GetGetterBody(target)}
}}
{GenerationUtils.GetNamespaceClose(target)}
#endif
");
		}

		private string GetSetterBody(VariableElement target)
		{
			if (!target.Details.m_TypeInfo.m_IsConst)
			{
				string getFunc = (target.Details.m_TypeInfo.m_IsPointer ? "GetPtrAs" : "GetAs") + $"<{target.Details.m_TypeInfo.m_Name}>()";

				if (target.Details.m_IsStatic)
					return $"{target.ParentElement.Name}::{target.Name} = value.{getFunc};";
				else
					return $"{target.ParentElement.Name}* obj = target.GetPtrAs<{target.ParentElement.Name}>(); obj->{target.Name} = value.{getFunc};";
			}
			return "";
		}

		private string GetGetterBody(VariableElement target)
		{
			if (target.Details.m_IsStatic)
				return $"return {target.ParentElement.Name}::{target.Name};";
			else
				return $"{target.ParentElement.Name}* obj = target.GetPtrAs<{target.ParentElement.Name}>(); return obj->{target.Name};";
		}
	}
}
