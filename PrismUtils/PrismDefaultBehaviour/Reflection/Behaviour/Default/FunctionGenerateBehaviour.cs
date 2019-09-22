using Prism.Reflection.Elements.Cpp;
using Prism.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace Prism.Reflection.Behaviour.Default
{
	public class FunctionGenerateBehaviour : GlobalReflectionBehaviour<FunctionElement>
	{
		public FunctionGenerateBehaviour()
			: base(BehaviourTarget.Function, BehaviourController.FieldGenerationPriority)
		{
		}
		
		public override void RunBehaviour(FunctionElement target)
		{
			GenerateIncludeContent(target);
			GenerateInlineContent(target);
			GenerateSourceContent(target);
		}

		private void GenerateIncludeContent(FunctionElement target)
		{
		}

		private void GenerateInlineContent(FunctionElement target)
		{
			target.AppendInlineContent($@"
private:
class MethodInfo_{target.UniqueName} : public Prism::Method
{{
public:
	MethodInfo_{target.UniqueName}();

	virtual Prism::TypeInfo GetParentInfo() const override;
	virtual Prism::ParamInfo* GetReturnInfo() const override;
	virtual Prism::ParamInfo* GetParamInfo(size_t index) const override;
	virtual size_t GetParamCount() const override;
	virtual Prism::Object Call(Prism::Object target, const std::vector<Prism::Object>& params) const override;
}};
");
		}

		private void GenerateSourceContent(FunctionElement target)
		{
			target.AppendSourceContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}
{target.ParentElement.Name}::MethodInfo_{target.UniqueName}::MethodInfo_{target.UniqueName}()
	: Prism::Method(
		PRISM_STR(""{target.Name}""),
		PRISM_DEVSTR(R""({target.Documentation})""),
		{{ {GenerationUtils.GetDataAttributeInstancesString(target)} }},
		Prism::Accessor::{char.ToUpper(target.Accessor[0]) + target.Accessor.Substring(1).ToLower()},
		{(target.Details.m_IsStatic ? 1 : 0)},
		{(target.Details.m_IsConst ? 1 : 0)},
		{(target.Details.m_IsVirtual ? 1 : 0)}
	)
	{{}}

const Prism::ParamInfo* {target.ParentElement.Name}::MethodInfo_{target.UniqueName}::GetReturnInfo() const
{{
#if {(target.Details.m_IsConstructor ? 1 : 0)}
	static Prism::ParamInfo info = {{
		PRISM_STR(""ReturnValue""),
		Prism::Assembly::Get().FindTypeOf<{target.ParentElement.Name}>(),
		1, 0
	}};
	return &info;
#else
#if {(target.Details.m_ReturnInfo.IsVoid() ? 1 : 0)}
	return nullptr;
#else
	static Prism::ParamInfo info = {{
		PRISM_STR(""ReturnValue""),
		Prism::Assembly::Get().FindTypeOf<{target.Details.m_ReturnInfo.m_Name}>(),
		{(target.Details.m_ReturnInfo.m_IsPointer ? 1 : 0)},
		{(target.Details.m_ReturnInfo.m_IsConst ? 1 : 0)},
	}};
#endif
#endif
}}

Prism::TypeInfo {target.ParentElement.Name}::MethodInfo_{target.UniqueName}::GetParentInfo() const
{{
	return Prism::Assembly::Get().FindTypeOf<{target.ParentElement.Name}>();
}}

const Prism::ParamInfo* {target.ParentElement.Name}::MethodInfo_{target.UniqueName}::GetParamInfo(size_t index) const
{{
#if {target.Details.m_Params.Length}
	switch(index)
	{{
{GetParamInfoBody(target)}
	}};
#endif
	return nullptr;
}}

size_t {target.ParentElement.Name}::MethodInfo_{target.UniqueName}::GetParamCount() const
{{
	return {target.Details.m_Params.Length}
}}

#if {(target.Details.m_IsConstructor ? 1 : 0)}
namespace PrismInternal
{{
	template<typename T>
	struct SafeConstruct_{target.UniqueName}
	{{
	private:
		template<typename A, typename = std::enable_if_t<std::is_abstract<A>::value>>
		static std::true_type AbstractCheck(...)
		{{
			return std::true_type;
		}}

		template<typename A, typename = std::enable_if_t<!std::is_abstract<A>::value>>
		static std::false_type AbstractCheck(...)
		{{
			return std::false_type;
		}}

		typedef decltype(AbstractCheck<T>(0)) IsAbstract;

		static T* ConstructInternal(Prism::Object& target, std::vector<Prism::Object>& params, std::true_type)
		{{
			return nullptr;
		}}

		static T* ConstructInternal(Prism::Object& target, std::vector<Prism::Object>& safeParams, std::true_type)
		{{
			return new{target.ParentElement.Name}({GetCallParams(target)});
		}}
	public:
		static T* Construct(Prism::Object& target, std::vector<Prism::Object>& params)
		{{
			return ConstructInternal(target, params, IsAbstract());
		}}
	}}; 
}}
#endif

Prism::Object {target.ParentElement.Name}::MethodInfo_{target.UniqueName}::Call(Prism::Object target, const std::vector<Prism::Object>& params) const
{{
	std::vector<Prism::Object>& safeParams = *const_cast<std::vector<Prism::Object>*>(&params);
	{GetCallString(target)}
}}
{GenerationUtils.GetNamespaceClose(target)}
#endif
");
		}

		private string GetParamInfoBody(FunctionElement target)
		{
			StringBuilder builder = new StringBuilder();

			for (int i = 0; i < target.Details.m_Params.Length; ++i)
			{
				var param = target.Details.m_Params[i];
				builder.Append($@"
	case {i}:
	{{
		static Prism::ParamInfo info = {{
			PRISM_STR(""{param.m_Name}""),
			Prism::Assembly::Get().FindTypeOf<{param.m_TypeInfo.m_Name}>(),
			{(param.m_TypeInfo.m_IsPointer ? 1 : 0)},
			{(param.m_TypeInfo.m_IsConst ? 1 : 0)},
		}};
		return &info;
	}}
");
			}

			return builder.ToString();
		}

		private string GetCallParams(FunctionElement target)
		{
			StringBuilder builder = new StringBuilder();

			List<string> outParams = new List<string>();

			for (int i = 0; i < target.Details.m_Params.Length; ++i)
			{
				var param = target.Details.m_Params[i];
				if (param.m_TypeInfo.m_IsReference)
					outParams.Add($"*safeParams[{i}].GetPtrAs<{param.m_TypeInfo.m_Name}>()");
				else if(param.m_TypeInfo.m_IsPointer)
					outParams.Add($"safeParams[{i}].GetPtrAs<{param.m_TypeInfo.m_Name}>()");
				else
					outParams.Add($"*safeParams[{i}].GetAs<{param.m_TypeInfo.m_Name}>()");
			}

			return string.Join(", ", outParams);
		}

		private string GetCallString(FunctionElement target)
		{
			if (target.Details.m_IsConstructor)
			{
				return $"return PrismInternal::Construct<{target.ParentElement.Name}>(target, safeParams);";
			}
			
			if (target.Details.m_IsStatic)
				return $"{(target.Details.m_ReturnInfo.IsVoid() ? "" : "return")} {target.ParentElement.Name}::{target.Name}({GetCallParams(target)}); {(target.Details.m_ReturnInfo.IsVoid() ? "return nullptr;" : "")}";
			else
				return $@"{target.ParentElement.Name}* obj = target.GetPtrAs<{target.ParentElement.Name}>();
	{(target.Details.m_ReturnInfo.IsVoid() ? "" : "return")} obj->{target.Name}({GetCallParams(target)}); {(target.Details.m_ReturnInfo.IsVoid() ? "return nullptr;" : "")}";
		}
	}
}
