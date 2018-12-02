using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	/// <summary>
	/// Reflection structure for enums
	/// </summary>
	public class EnumStructureReflection : StructureReflectionBase
	{
		public class Value
		{
			public string Name;
			public string HardcodedValue;
			public string DocString;
			public string PreProcessorCondition;
		}

		/// <summary>
		/// The name that this enum was setup with
		/// </summary>
		private string m_EnumName;

		/// <summary>
		/// The internal type used to store this enum (If declared)
		/// </summary>
		private string m_EnumType;

		/// <summary>
		/// The values belonging to this enum
		/// </summary>
		private List<Value> m_Values;

		public EnumStructureReflection(StructureSignature.ImplementationBeginData data, string[] tokenNamespace, ConditionState conditionState, string bodyFile, int bodyLine, string tokenParams, string docString)
			: base(data.DeclareName, tokenNamespace, conditionState, bodyFile, bodyLine, tokenParams, docString)
		{
			m_EnumName = data.DeclareName;
			m_EnumType = string.Join(" ", data.ParentStructures.Select(v => v.DeclareName));
			m_Values = new List<Value>();
		}

		public override string StructureType
		{
			get { return "enum"; }
		}

		public Value[] Values
		{
			get { return m_Values.ToArray(); }
		}

		public override void AddInternalSignature(SignatureInfo sigInfo, string accessor, ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString)
		{
			// Ignore other values (Will get caught by compiler anyway)
			if (sigInfo.SignatureType == SignatureInfo.SigType.EnumValueEntry)
			{
				var data = (EnumEntrySignature.EnumValueData)sigInfo.AdditionalParam;

				Value value = new Value();
				value.Name = data.Name;
				value.HardcodedValue = data.HardcodedValue;
				value.DocString = docString;
				value.PreProcessorCondition = conditionState.CurrentCondition;
				m_Values.Add(value);
			}
		}

		public override string GenerateHeaderReflectionContent()
		{
			return "";
		}

		public override string GenerateIncludeReflectionContent()
		{
			string content = "";

			// Enter into the existing namespace
			foreach (string space in TokenNamespace)
				content += "namespace " + space + " {\n";

			content += @"
namespace Private { namespace Generated {
class EnumInfo_%ENUM_NAME% : public Prism::Enum
{
private:
	static EnumInfo_%ENUM_NAME% s_AssemblyInstance;
	EnumInfo_%ENUM_NAME%();
};
}}
";
			// Exit into the existing namespace
			foreach (string space in TokenNamespace)
				content += "}\n";

			string forwardDeclare = m_EnumName;
			if (m_EnumType != "")
				forwardDeclare += " : " + m_EnumType;

			return content
				.Replace("%ENUM_NAME%", m_EnumName)
				.Replace("%ENUM_FORWARD_DECLARE%", forwardDeclare);
		}

		public override string GenerateSourceReflectionContent()
		{
			string content = "";

			// Enter into the existing namespace
			foreach (string space in TokenNamespace)
				content += "namespace " + space + " {\n";

			content += @"
namespace Private { namespace Generated {
EnumInfo_%ENUM_NAME%::EnumInfo_%ENUM_NAME%()
	: Prism::Enum(
		Prism::TypeId::Get<%ENUM_NAME%>(), 
		PRISM_STR(""%NAMESPACE_STR%""), PRISM_STR(""%ENUM_NAME%""), PRISM_DEVSTR(R""(%DOC_STRING%)""),
		sizeof(%ENUM_NAME%),
		{ %ATTRIBUTES_VALUES% },
		{ %ENUM_VALUES% }
	)
{}

EnumInfo_%ENUM_NAME% EnumInfo_%ENUM_NAME%::s_AssemblyInstance;
}}
";
			string enumValues = "";
			foreach (var value in m_Values)
			{
				enumValues += "\n#if " + value.PreProcessorCondition + "\n";
				enumValues += "Prism::Enum::Value(PRISM_STR(\"" + value.Name + "\"), (size_t)" + m_EnumName + "::" + value.Name + "),\n";
				enumValues += "#endif\n";
			}

			// Exit into the existing namespace
			foreach (string space in TokenNamespace)
				content += "}\n";

			return content
				.Replace("%ENUM_NAME%", m_EnumName)
				.Replace("%ATTRIBUTES_VALUES%", GenerateAttributeInstancesString("Enum"))
				.Replace("%ENUM_VALUES%", enumValues)
				.Replace("%NAMESPACE_STR%", TokenNamespaceFormatted.Replace("::", "."))
				.Replace("%DOC_STRING%", SafeDocString);
		}
	}
}
