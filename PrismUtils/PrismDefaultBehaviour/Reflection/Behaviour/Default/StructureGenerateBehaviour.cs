using Prism.Reflection.Behaviour;
using Prism.Reflection.Behaviour.Utils;
using Prism.Reflection.Elements;
using Prism.Reflection.Elements.Cpp;
using Prism.Reflection.Elements.Cpp.Data;
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
			GenerationUtils.AppendChildIncludeContent(target);
			GenerationUtils.AppendChildInlineContent(target);
			GenerationUtils.AppendChildSourceContent(target);

			// Add template info class
			if(target.IsTemplate)
			{
				StructureInfo structInfo = StructureInfo.Generate(
					"TemplateInfo_" + target.GetUntemplatedName(),
					parents: new InheritanceInfo[]
					{
						new InheritanceInfo{ m_Accessor = "public", m_Name = "Prism::TemplateInfo" }
					}
				);

				var reflectStruct = target.GenerateStructure(structInfo, target.ParentElement, inheritTemplate: false);
				reflectStruct.ParentElement = null;

				// Functions
				{
					// Constructor
					{
						FunctionInfo info = new FunctionInfo
						{
							m_Name = "TemplateInfo_" + target.GetUntemplatedName(),
							m_IsConstructor = true,
							m_Params = new VariableInfo[0]
						};
						var elem = reflectStruct.GenerateFunction(info, reflectStruct, "public");
						FunctionContent.GenerateFull(elem, $@"
: Prism::TemplateInfo(
		PRISM_STR(""{target.GetUntemplatedName()}""),
		{target.TemplateDetails.m_Params.Length}
	)
	{{}}
");
					}
				}

				// Variables
				{
					var elem = reflectStruct.GenerateVariable(VariableInfo.Generate(
						name: "s_BaseTemplateInstance",
						typeInfo: new TypeInfo { m_Name = "Prism::TemplateInfo", m_IsPointer = true, m_IsConst = true },
						isStatic: true,
						defaultValue: $"new {"TemplateInfo_" + target.GetUntemplatedName()}()"
					), reflectStruct, "public");
					VariableContent.GenerateFull(elem);
				}

				StructureContent.GenerateIncludeFull(target, reflectStruct);
			}

			// Create ClassInfo Prism class
			{
				StructureInfo structInfo = StructureInfo.Generate(
					"ClassInfo",
					parents: new InheritanceInfo[]
					{
						new InheritanceInfo{ m_Accessor = "public", m_Name = "Prism::Class" }
					}
				);
				var reflectStruct = target.GenerateStructure(structInfo, target);

				// Add functions
				{
					// Constructor
					{
						FunctionInfo info = new FunctionInfo
						{
							m_Name = "ClassInfo",
							m_IsConstructor = true,
							m_Params = new VariableInfo[0]
						};
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
: Prism::Class(
		Prism::TypeId::Get<{target.Name}>(),
		PRISM_STR(""{string.Join(".", target.Namespace)}""),
		{GetDisplayTypename(target)},
		PRISM_DEVSTR(R""({target.Documentation})""),
		sizeof({target.Name}),
		{(target.IsTemplate ? "TemplateInfo_" + target.GetUntemplatedName() + "::s_BaseTemplateInstance" : "nullptr")},
		{{{GenerationUtils.GetDataAttributeInstancesString(target)}}},
		std::is_abstract<{target.Name}>::value,
		{{ {GetConstructorInstances(target)} }},
		{{ {GetMethodInstances(target)} }},
		{{ {GetPropertyInstances(target)} 
		}}
	)
	{{}}
");
					}

					// GetParentClass
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetParentClass",
							returnInfo: new TypeInfo { m_Name = "Prism::ClassInfo" },
							paramInfos: new VariableInfo[]
							{
								new VariableInfo { m_Name="searchIndex", m_TypeInfo=new TypeInfo{ m_Name="int" } }
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "private");
						FunctionContent.GenerateFull(elem, $@"
{{
	size_t index = 0;
{GetParentClassBody(target)}
	return nullptr;
}}
");
					}

					// GetParentCount
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetParentCount",
							returnInfo: new TypeInfo { m_Name = "size_t" },
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "private");
						FunctionContent.GenerateFull(elem, $@"
{{
	size_t count = 0;
{GetParentCountBody(target)}
	return count;
}}
");
					}

					// GetParentCount
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetTemplateParam",
							returnInfo: new TypeInfo { m_Name = "Prism::TypeInfo" },
							paramInfos: new VariableInfo[]
							{
								new VariableInfo { m_Name="index", m_TypeInfo=new TypeInfo{ m_Name="int" } }
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "private");
						FunctionContent.GenerateFull(elem, $@"
{{
{GetTemplateParamBody(target)}
}}
");
					}
				}
				target.AppendInlineContent("private:");
				StructureContent.GenerateInlineFull(target, reflectStruct);
			}

			// RetrievePrismInfo (Use for templates to instantiate the type info)
			if (target.IsTemplate)
			{
				FunctionInfo info = FunctionInfo.Generate(
					name: "RetrievePrismInfo",
					returnInfo: new TypeInfo { m_Name = "Prism::Type", m_IsConst = true, m_IsPointer = true },
					isStatic: true
				);
				var elem = target.GenerateFunction(info, target, "public");
				FunctionContent.GenerateFull(elem, $"{{ static {target.Name}::ClassInfo instance; return &instance; }}");
			}
			else
			{
				// Non templated just need to instantiate once
				var elem = target.GenerateVariable(VariableInfo.Generate(
						name: "s_AssemblyInstance",
						typeInfo: new TypeInfo { m_Name = "Prism::Type", m_IsPointer = true, m_IsConst = true },
						isStatic: true,
						defaultValue: $"new {target.Name}::ClassInfo()"
					//isInlined: true // C++17 required for inlined variables (This will work for templates too)
					), target, "private");
				VariableContent.GenerateFull(elem);
			}

			// Type info getter
			{
				FunctionInfo info = FunctionInfo.Generate(
					name: "GetTypeInfo",
					returnInfo: new TypeInfo { m_Name = "Prism::TypeInfo" },
					isConst: true,
					isVirtual: true
				);
				var elem = target.GenerateFunction(info, target, "public");
				FunctionContent.GenerateFull(elem, $"{{ return Prism::Assembly::Get().FindTypeOf<{target.Name}>(); }}");
			}
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
				string typeName = prefix + elem.UniqueName;
				builder.Append($"__if_exists({typeName}) {{ new {typeName}(), }}\n");
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
				builder.Append($"\t__if_exists({parentInfo.m_Name}::ClassInfo) {{ ++count; }}\n");
			}

			return builder.ToString();
		}

		private static string GetDisplayTypename(StructureElement target)
		{
			if (target.IsTemplate)
			{
				var typeNames = target.TemplateDetails.m_Params.Select((p) => "Prism::String(typeid(" + p.m_Name + ").name())");
				return "Prism::String(PRISM_STR(\"" + target.GetUntemplatedName() + "<\")) + " + string.Join(" + PRISM_STR(\",\") + ", typeNames) + " + PRISM_STR(\">\")";
			}
			else
				return "PRISM_STR(\"" + target.Name + "\")";
		}


		private static string GetTemplateParamBody(StructureElement target)
		{
			StringBuilder builder = new StringBuilder();

			if (target.IsTemplate)
			{
				builder.Append($@"
switch (index)
{{
");
				for (int i = 0; i < target.TemplateDetails.m_Params.Length; ++i)
				{
					var templateParam = target.TemplateDetails.m_Params[i];
					builder.Append($@"
	case {i}:
	{{
		return Prism::Assembly::Get().FindTypeOf<{templateParam.m_Name}>();
	}}
");
				}

				builder.Append($@"

default:
	return nullptr;
}}
");
				return builder.ToString();
			}
			else
			{
				return "return nullptr;";
			}
			
		}
	}
}
