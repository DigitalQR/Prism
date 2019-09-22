using Prism.Reflection.Behaviour;
using Prism.Reflection.Behaviour.Utils;
using Prism.Reflection.Elements.Cpp;
using Prism.Reflection.Elements.Cpp.Data;
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
			StructureElement parent = VariableContent.GetParent(target);
			
			// Create VarInfo Prism class
			{
				StructureInfo structInfo = StructureInfo.Generate(
					"VariableInfo_" + target.UniqueName,
					parents: new InheritanceInfo[] 
					{
						new InheritanceInfo{ m_Accessor = "public", m_Name = "Prism::Property" }
					}
				);
				var reflectStruct = parent.GenerateStructure(structInfo, target);

				// Add functions
				{
					// Constructor
					{
						FunctionInfo info = new FunctionInfo
						{
							m_Name = "VariableInfo_" + target.UniqueName,
							m_IsConstructor = true,
							m_Params = new VariableInfo[0]
						};
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $@"
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

					// GetTypeInfo
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "GetTypeInfo",
							returnInfo: new TypeInfo { m_Name = "Prism::TypeInfo" },
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $"{{ return Prism::Assembly::Get().FindTypeOf<{target.Details.m_TypeInfo.m_Name}>(); }}");
					}

					// Set
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "Set",
							paramInfos: new VariableInfo[]
							{
								new VariableInfo { m_Name="target", m_TypeInfo=new TypeInfo { m_Name="Prism::Object"} },
								new VariableInfo { m_Name="value", m_TypeInfo=new TypeInfo { m_Name="Prism::Object"} },
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $"{{{GetSetterBody(target)}}}");
					}

					// Get
					{
						FunctionInfo info = FunctionInfo.Generate(
							name: "Get",
							returnInfo: new TypeInfo { m_Name="Prism::Object" },
							paramInfos: new VariableInfo[]
							{
								new VariableInfo { m_Name="target", m_TypeInfo=new TypeInfo { m_Name="Prism::Object"} },
							},
							isVirtual: true,
							isConst: true
						);
						var elem = reflectStruct.GenerateFunction(info, target, "public");
						FunctionContent.GenerateFull(elem, $"{{{GetGetterBody(target)}}}");
					}
				}
				
				target.AppendInlineContent("private:");
				StructureContent.GenerateInlineFull(target, reflectStruct);
			}
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
