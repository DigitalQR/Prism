using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Reflection.Tokens;

namespace Prism.Reflection.Behaviour.Custom
{
	public class EnumReflectBehaviour : GlobalReflectionBehaviour
	{
		public EnumReflectBehaviour()
			: base(BehaviourTarget.Enumurator, 0)
		{
		}

		public override void RunBehaviour(IReflectableToken target)
		{
			EnumToken token = target as EnumToken;

			if (token == null)
			{
				throw new Exception("Invalid state reached. Internal_ReflectEnum expected a EnumToken target");
			}

			token.AppendIncludeContent(GenerateIncludeContent(token));
			token.AppendImplementationContent(GenerateImplementationContent(token));
		}

		private StringBuilder GenerateIncludeContent(EnumToken target)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(@"
#if $(PreProcessorCondition)
class EnumInfo_$(ReflectedName) : public Prism::Enum
{
private:
	static EnumInfo_$(ReflectedName) s_AssemblyInstance;
	EnumInfo_$(ReflectedName)();
};
#endif
");

			return builder;
		}

		private StringBuilder GenerateImplementationContent(EnumToken target)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(@"
#if $(PreProcessorCondition)
EnumInfo_$(ReflectedName) EnumInfo_$(ReflectedName)::s_AssemblyInstance;

EnumInfo_$(ReflectedName)::EnumInfo_$(ReflectedName)()
	: Prism::Enum(
		Prism::TypeId::Get<$(Name)>(),
		PRISM_STR(""$(NamespaceList[.])""), PRISM_STR(""$(Name)""), PRISM_DEVSTR(R""($(Documentation))""),
		sizeof($(Name)),
		{ $(AttributesInstances) },
		{ $(EnumValueInstances) }
	)
{
}
#endif
");
			string template_AttributesInstances = "";
			string template_ValueInstances = "";

			int a = 0;
			foreach (AttributeData attrib in target.DataAttributes)
			{
				string current = "new $(Attribute[%i].Name)Attribute($(Attribute[%i].Params)),\n";
				template_AttributesInstances += current.Replace("%i", a.ToString());
				++a;
			}

			for (int i = 0; i < target.Values.Count; ++i)
			{
				string current_ValueInstance = @"
#if $(Value[%i].PreProcessorCondition)
Prism::Enum::Value(PRISM_STR(""$(Value[%i].Name)""), (size_t)$(Name)::$(Value[%i].Name)), 
#endif
";
				template_ValueInstances += current_ValueInstance.Replace("%i", i.ToString());
			}

			builder.Replace("$(AttributesInstances)", template_AttributesInstances);
			builder.Replace("$(EnumValueInstances)", template_ValueInstances);

			return builder;
		}
	}
}
