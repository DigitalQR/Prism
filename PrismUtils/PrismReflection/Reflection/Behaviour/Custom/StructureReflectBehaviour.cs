using System;
using System.Collections.Generic;
using System.Text;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour.Custom
{
	public class StructureReflectBehaviour : GlobalReflectionBehaviour
	{
		public StructureReflectBehaviour()
			: base(BehaviourTarget.Structure, 0)
		{

		}

		public override void RunBehaviour(IReflectableToken target)
		{
			StructureToken token = target as StructureToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. Internal_ReflectStructure expected a StructureToken target");
			}

			token.AppendDeclarationContent(GenerateDeclarationContent(token));
			token.AppendImplementationContent(GenerateImplementationContent(token));
		}

		private StringBuilder GenerateDeclarationContent(StructureToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
public:
	virtual Prism::TypeInfo GetTypeInfo() const;

private:
class ClassInfo : public Prism::Class
{
private:
	static ClassInfo s_AssemblyInstance;
	ClassInfo();
	virtual Prism::ClassInfo GetParentClass(int index) const override;
	virtual size_t GetParentCount() const override;
};
");

			return builder;
		}

		private StringBuilder GenerateImplementationContent(StructureToken target)
		{
			StringBuilder builder = new StringBuilder();

			builder.Append(@"
#if $(PreProcessorCondition)
$(Name)::ClassInfo $(Name)::ClassInfo::s_AssemblyInstance;

$(Name)::ClassInfo::ClassInfo()
	: Prism::Class(
		Prism::TypeId::Get<$(Name)>(),
		PRISM_STR(""$(NamespaceList[.])""), PRISM_STR(""$(Name)""), PRISM_DEVSTR(R""($(Documentation))""),
		sizeof($(Name)),
		{ $(AttributesInstances) },
		std::is_abstract<$(Name)>::value,
		{ $(ConstructorInstances)},
		{ $(MethodInstances) },
		{ $(PropertyInstances) }
	)
{
}

Prism::TypeInfo $(Name)::GetTypeInfo() const 
{ 
	return Prism::Assembly::Get().FindTypeOf<$(Name)>(); 
}

Prism::ClassInfo $(Name)::ClassInfo::GetParentClass(int searchIndex) const
{
	size_t index = 0;
$(FuncBody_GetParentClass)
	return nullptr;
}

size_t $(Name)::ClassInfo::GetParentCount() const
{
	size_t count = 0;
$(FuncBody_GetParentCount)
	return count;
}
#endif
");

			// Expand custom macros
			string template_AttributesInstances = "";
			string template_ConstructorInstances = "";
			string template_MethodInstances = "";
			string template_PropertyInstances = "";
			string template_GetParentClass = "";
			string template_GetParentCount = "";

			int a = 0;
			foreach (AttributeData attrib in target.DataAttributes)
			{
				string current = "new $(Attribute[%i].Name)Attribute($(Attribute[%i].Params)),\n";
				template_AttributesInstances += current.Replace("%i", a.ToString());
				++a;
			}

			for (int i = 0; i < target.Methods.Count; ++i)
			{
				if (target.Methods[i].IsDestructor)
					continue;
				
					string current = @"
#if $(Method[%i].PreProcessorCondition)
new MethodInfo_$(Method[%i].ReflectedName)(),
#endif
";
				if (target.Methods[i].IsConstructor)
					template_ConstructorInstances += current.Replace("%i", i.ToString());
				else
					template_MethodInstances += current.Replace("%i", i.ToString());
			}

			for (int i = 0; i < target.Properties.Count; ++i)
			{
				string current = @"
#if $(Property[%i].PreProcessorCondition)
new VariableInfo_$(Property[%i].ReflectedName)(),
#endif
";
				template_PropertyInstances += current.Replace("%i", i.ToString());
			}

			for (int i = 0; i < target.ParentStructures.Count; ++i)
			{
				string current_GetParentClass = @"
__if_exists($(Parent[%i].Name)::ClassInfo)
{
	if(index++ == searchIndex) 
	{
		return Prism::Assembly::Get().FindTypeOf<$(Parent[%i].Name)>();
	}
}
";
				string current_GetParentCount = "__if_exists($(Parent[%i].Name)::ClassInfo) { ++count; }\n";

				template_GetParentClass += current_GetParentClass.Replace("%i", i.ToString());
				template_GetParentCount += current_GetParentCount.Replace("%i", i.ToString());
			}

			builder.Replace("$(AttributesInstances)", template_AttributesInstances);
			builder.Replace("$(ConstructorInstances)", template_ConstructorInstances);
			builder.Replace("$(MethodInstances)", template_MethodInstances);
			builder.Replace("$(PropertyInstances)", template_PropertyInstances);
			builder.Replace("$(FuncBody_GetParentClass)", template_GetParentClass);
			builder.Replace("$(FuncBody_GetParentCount)", template_GetParentCount);

			return builder;
		}
	}
}
