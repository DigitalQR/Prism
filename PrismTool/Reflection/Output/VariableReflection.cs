using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class VariableReflection : TokenReflection
	{
		/// <summary>
		/// The structure to which this variable info belongs to
		/// </summary>
		private StructureReflection m_ParentStructure;

		/// <summary>
		/// The info about this variable
		/// </summary>
		private VariableInfo m_ReflectionInfo;

		public VariableReflection(StructureReflection parentStructure, VariableInfo variable, ConditionState conditionState, int tokenLine, string tokenParams, string docString)
			: base(conditionState, tokenLine, tokenParams, docString)
		{
			m_ParentStructure = parentStructure;
			m_ReflectionInfo = variable;
		}

		public VariableInfo ReflectionInfo
		{
			get { return m_ReflectionInfo; }
		}

		public override string GenerateHeaderReflectionContent()
		{
			string content = "";

			// Setup Prism Class
			content += @"
class VariableInfo_%VARIABLE_NAME% : public Prism::Property
{
public:
	VariableInfo_%VARIABLE_NAME%();

	virtual Prism::TypeInfo GetParentInfo() const override;
	virtual Prism::TypeInfo GetTypeInfo() const override;

	virtual void Set(Prism::Holder, Prism::Holder) const override;
	virtual Prism::Holder Get(Prism::Holder) const override;
};
";

			return content
				.Replace("%VARIABLE_NAME%", m_ReflectionInfo.VariableName);
		}

		public override string GenerateIncludeReflectionContent()
		{
			return "";
		}

		public override string GenerateSourceReflectionContent()
		{
			string content = "";

			// Setup Prism Class
			content += @"
%PARENT_STRUCTURE%::VariableInfo_%VARIABLE_NAME%::VariableInfo_%VARIABLE_NAME%()
	: Prism::Property(
		PRISM_STR(""%VARIABLE_NAME%""), PRISM_DEVSTR(R""(%DOC_STRING%)""),
		%IS_POINTER%, %IS_STATIC%, %IS_CONST%
	)
{
}

Prism::TypeInfo %PARENT_STRUCTURE%::VariableInfo_%VARIABLE_NAME%::GetParentInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<%PARENT_STRUCTURE%>();
}

Prism::TypeInfo %PARENT_STRUCTURE%::VariableInfo_%VARIABLE_NAME%::GetTypeInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<%VARIABLE_TYPE%>();
}

void %PARENT_STRUCTURE%::VariableInfo_%VARIABLE_NAME%::Set(Prism::Holder target, Prism::Holder value) const
{
#if !(%IS_CONST%)
#if %IS_STATIC%
	%PARENT_STRUCTURE%::%VARIABLE_NAME% = value.GetAs<%VARIABLE_TYPE%>();
#else
	%PARENT_STRUCTURE%* obj = target.GetPtrAs<%PARENT_STRUCTURE%>();
	obj->%VARIABLE_NAME% = value.GetAs<%VARIABLE_TYPE%>();
#endif
#endif
}

Prism::Holder %PARENT_STRUCTURE%::VariableInfo_%VARIABLE_NAME%::Get(Prism::Holder target) const
{
#if %IS_STATIC%
	return %PARENT_STRUCTURE%::%VARIABLE_NAME%;
#else
	%PARENT_STRUCTURE%* obj = target.GetPtrAs<%PARENT_STRUCTURE%>();
	return obj->%VARIABLE_NAME%;
#endif
}
";
			
			return content
				.Replace("%PARENT_STRUCTURE%", m_ParentStructure.DeclerationName)
				.Replace("%VARIABLE_NAME%", m_ReflectionInfo.VariableName)
				.Replace("%VARIABLE_TYPE%", m_ReflectionInfo.TypeInfo.InnerType.TypeName)
				.Replace("%IS_POINTER%", m_ReflectionInfo.TypeInfo.PointerCount != 0 ? "1" : "0")
				.Replace("%IS_STATIC%", m_ReflectionInfo.TypeInfo.IsStatic ? "1" : "0")
				.Replace("%IS_CONST%", m_ReflectionInfo.TypeInfo.IsConst ? "1" : "0")
				.Replace("%DOC_STRING%", SafeDocString);
		}
	}
}
