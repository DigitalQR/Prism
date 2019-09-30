using Prism.Reflection.Behaviour.Utils;
using Prism.Reflection.Elements.Cpp;
using Prism.Reflection.Elements.Cpp.Data;
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
			GenerateSourceContent(target);

			StructureElement parent = FunctionContent.GetParent(target);

			// Create MethodInfo_ Prism class
			{
				StructureInfo structInfo = StructureInfo.Generate(
					"FunctionInfo_" + target.UniqueName,
					parents: new InheritanceInfo[]
					{
						new InheritanceInfo{ m_Accessor = "public", m_Name = "Prism::Method" }
					}
				);
				var reflectStruct = parent.GenerateStructure(structInfo, target);

				// Add functions
				{
					// Constructor
					{
						FunctionInfo info = new FunctionInfo
						{
							m_Name = "FunctionInfo_" + target.UniqueName,
							m_IsConstructor = true,
							m_Params = new VariableInfo[0]
						};
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
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
");
					}

					// GetParentInfo
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetParentInfo",
							returnInfo: new TypeInfo { m_Name = "Prism::TypeInfo" },
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $"{{ return Prism::Assembly::Get().FindTypeOf<{target.ParentElement.Name}>(); }}");
					}

					// GetReturnInfo
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetReturnInfo",
							returnInfo: new TypeInfo { m_Name = "Prism::ParamInfo", m_IsPointer = true },
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
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
	return &info;
#endif
#endif
}}
");
					}

					// GetParentInfo
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetParamInfo",
							returnInfo: new TypeInfo { m_Name = "Prism::ParamInfo", m_IsPointer = true },
							paramInfos: new VariableInfo[] 
							{
								new VariableInfo { m_Name="index", m_TypeInfo=new TypeInfo{ m_Name="size_t" } }
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
{{
#if {target.Details.m_Params.Length}
	switch(index)
	{{
{GetParamInfoBody(target)}
	}};
#endif
	return nullptr;
}}
");
					}

					// GetParamCount
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetParamCount",
							returnInfo: new TypeInfo { m_Name = "size_t" },
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $"{{ return {target.Details.m_Params.Length}; }}");
					}

					// Call
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "Call",
							returnInfo: new TypeInfo { m_Name = "Prism::Object" },
							paramInfos: new VariableInfo[]
							{
								new VariableInfo { m_Name="target", m_TypeInfo=new TypeInfo{ m_Name="Prism::Object" } },
								new VariableInfo { m_Name="params", m_TypeInfo=new TypeInfo{ m_Name="std::vector<Prism::Object>", m_IsReference=true, m_IsConst=true } }
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
{{
	std::vector<Prism::Object>& safeParams = *const_cast<std::vector<Prism::Object>*>(&params);
	{GetCallString(target)}
}}
");
					}
				}

				target.AppendInlineContent("private:");
				StructureContent.GenerateInlineFull(target, reflectStruct);
			}
		}
		
		private void GenerateSourceContent(FunctionElement target)
		{
			target.AppendSourceContent($@"
#if {target.PreProcessorCondition}
{GenerationUtils.GetNamespaceOpen(target)}

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
