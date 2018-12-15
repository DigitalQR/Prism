﻿using Prism.CodeParsing;
using Prism.CodeParsing.Signatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Prism.Reflection
{
	public class FunctionReflection : TokenReflection
	{
		/// <summary>
		/// The structure to which this variable info belongs to
		/// </summary>
		private StructureReflection m_ParentStructure;

		/// <summary>
		/// The info about this function
		/// </summary>
		private FunctionInfo m_ReflectionInfo;

		/// <summary>
		/// Is this currrent function a constructor
		/// </summary>
		private bool m_IsConstructor = false;

		/// <summary>
		/// The accessor of this function
		/// </summary>
		private string m_Accessor;
		
		// Constructor reflection
		public FunctionReflection(StructureReflection parentStructure, string accessor, ConstructorInfo constructor, ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString)
			: this(parentStructure, accessor, constructor.ToFunctionInfo(), conditionState, tokenFile, tokenLine, tokenParams, docString)
		{
			m_IsConstructor = true;
		}

		// Normal function reflection
		public FunctionReflection(StructureReflection parentStructure, string accessor, FunctionInfo function, ConditionState conditionState, string tokenFile, int tokenLine, string tokenParams, string docString)
			: base(conditionState, tokenFile, tokenLine, tokenParams, docString)
		{
			m_ParentStructure = parentStructure;
			m_ReflectionInfo = function;
			m_Accessor = accessor.ToLower();

			if (function.ReturnType != null && function.ReturnType.PointerCount > 1)
			{
				SignatureInfo dudInfo = new SignatureInfo(tokenLine, "", SignatureInfo.SigType.FunctionDeclare);
				throw new ReflectionException(ReflectionErrorCode.TokenUnsupportedUsage, dudInfo, "Prism currently only supported single pointer types (Check return type)");
			}
			if (function.ParamCount != 0)
			{
				int i = 0;
				foreach (var paramType in function.ParamTypes)
				{
					if (paramType.TypeInfo.PointerCount > 1)
					{
						SignatureInfo dudInfo = new SignatureInfo(tokenLine, "", SignatureInfo.SigType.FunctionDeclare);
						throw new ReflectionException(ReflectionErrorCode.TokenUnsupportedUsage, dudInfo, "Prism currently only supported single pointer types (Check argument " + (i + 1) + ")");
					}
					++i;
				}
			}
		}

		public FunctionInfo ReflectionInfo
		{
			get { return m_ReflectionInfo; }
		}

		public bool IsConstructor
		{
			get { return m_IsConstructor; }
		}

		public override string GenerateHeaderReflectionContent()
		{
			string content = "";

			// Setup Prism Class
			content += @"
class MethodInfo_%FUNCTION_NAME% : public Prism::Method
{
public:
	MethodInfo_%FUNCTION_NAME%();

	virtual Prism::TypeInfo GetParentInfo() const override;
	virtual const Prism::ParamInfo* GetReturnInfo() const override;
	virtual const Prism::ParamInfo* GetParamInfo(size_t index) const override;
	virtual size_t GetParamCount() const override;

	virtual Prism::Holder Call(Prism::Holder target, const std::vector<Prism::Holder>& params) const override;
};
";
			return content
				.Replace("%FUNCTION_NAME%", m_ReflectionInfo.SafeFunctionName)
				.Replace("%FUNCTION_INTERNAL_NAME%", m_ReflectionInfo.FunctionName);
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

%PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::MethodInfo_%FUNCTION_NAME%()
	: Prism::Method(
		PRISM_STR(""%FUNCTION_INTERNAL_NAME%""), PRISM_DEVSTR(R""(%DOC_STRING%)""), 
		{ %ATTRIBUTES_VALUES% }, 
		Prism::Accessor::%ACCESSOR%,
		%IS_STATIC%, %IS_CONST%, %IS_VIRTUAL%
	)
{}

const Prism::ParamInfo* %PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::GetReturnInfo() const
{
#if %HAS_RETURN%
	static Prism::ParamInfo info = {
		PRISM_STR(""ReturnValue""),
		Prism::Assembly::Get().FindTypeOf<%RETURN_TYPE%>(),
		%RETURN_TYPE_PTR%,
		%RETURN_TYPE_CONST%
	};
	return &info;
#else
	return nullptr;
#endif
}

Prism::TypeInfo %PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::GetParentInfo() const
{
	return Prism::Assembly::Get().FindTypeOf<%PARENT_STRUCTURE%>();
}

const Prism::ParamInfo* %PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::GetParamInfo(size_t index) const
{
#if %PARAM_COUNT% != 0
	switch (index)
	{
	%SELECT_PARAM%
	}
#endif
	return nullptr;	
}
size_t %PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::GetParamCount() const
{
	return %PARAM_COUNT%;
}

Prism::Holder %PARENT_STRUCTURE%::MethodInfo_%FUNCTION_NAME%::Call(Prism::Holder target, const std::vector<Prism::Holder>& params) const
{
	std::vector<Prism::Holder>& safeParams = *const_cast<std::vector<Prism::Holder>*>(&params);
#if %IS_CONSTRUCTOR%
	return new %PARENT_STRUCTURE%(%CALL_PARAMS%);
#else
#if %IS_STATIC%
#if %HAS_RETURN%
	return %PARENT_STRUCTURE%::%FUNCTION_INTERNAL_NAME%(%CALL_PARAMS%);
#else
	%PARENT_STRUCTURE%::%FUNCTION_INTERNAL_NAME%(%CALL_PARAMS%);
	return nullptr;
#endif
#else
	%PARENT_STRUCTURE%* obj = target.GetPtrAs<%PARENT_STRUCTURE%>();
#if %HAS_RETURN%
	return obj->%FUNCTION_INTERNAL_NAME%(%CALL_PARAMS%);
#else
	obj->%FUNCTION_INTERNAL_NAME%(%CALL_PARAMS%);
	return nullptr;
#endif
#endif
#endif
}
";

			// Make param select string and call params
			string paramSelect = "";
			string callParams = "";
			if (m_ReflectionInfo.ParamCount != 0)
			{
				int i = 0;
				foreach (var param in m_ReflectionInfo.ParamTypes)
				{
					// Select
					string select = @"
case %PARAM_INDEX%:
{
	static Prism::ParamInfo info = {
		PRISM_STR(""%PARAM_NAME%""),
		Prism::Assembly::Get().FindTypeOf<%PARAM_TYPE%>(),
		%PARAM_TYPE_PTR%,
		%PARAM_TYPE_CONST%
	};
	return &info;
}
";
					paramSelect += select
						.Replace("%PARAM_INDEX%", "" + i)
						.Replace("%PARAM_NAME%", param.VariableName)
						.Replace("%PARAM_TYPE%", param.TypeInfo.InnerType.TypeName)
						.Replace("%PARAM_TYPE_PTR%", param.TypeInfo.PointerCount != 0 ? "1" : "0")
						.Replace("%PARAM_TYPE_CONST%", param.TypeInfo.IsConst ? "1" : "0");

					// Params
					string call = "";
					if (param.TypeInfo.IsReference)
						call = "*safeParams[%PARAM_INDEX%].GetPtrAs<%PARAM_TYPE%>()";
					else if(param.TypeInfo.PointerCount != 0)
						call = "safeParams[%PARAM_INDEX%].GetPtrAs<%PARAM_TYPE%>()";
					else
						call = "safeParams[%PARAM_INDEX%].GetAs<%PARAM_TYPE%>()";

					callParams += call
						.Replace("%PARAM_INDEX%", "" + i)
						.Replace("%PARAM_TYPE%", param.TypeInfo.InnerType.TypeName);

					if (i != m_ReflectionInfo.ParamCount - 1)
						callParams += ", ";

					++i;
				}
			}



			if (m_ReflectionInfo.ReturnType != null)
			{
				content = content
				   .Replace("%HAS_RETURN%", "1")
				   .Replace("%RETURN_TYPE%", m_ReflectionInfo.ReturnType.InnerType.TypeName)
				   .Replace("%RETURN_TYPE_PTR%", m_ReflectionInfo.ReturnType.PointerCount != 0 ? "1" : "0")
				   .Replace("%RETURN_TYPE_CONST%", m_ReflectionInfo.ReturnType.IsConst ? "1" : "0");
			}
			else
			{
				content = content
				   .Replace("%HAS_RETURN%", "0")
				   .Replace("%RETURN_TYPE%", "void")
				   .Replace("%RETURN_TYPE_PTR%", "0")
				   .Replace("%RETURN_TYPE_CONST%", "0");
			}
			
			return content
				.Replace("%FUNCTION_NAME%", m_ReflectionInfo.SafeFunctionName)
				.Replace("%FUNCTION_INTERNAL_NAME%", m_ReflectionInfo.FunctionName)
				.Replace("%PARENT_STRUCTURE%", m_ParentStructure.DeclerationName)
				.Replace("%ATTRIBUTES_VALUES%", GenerateAttributeInstancesString("Method"))
				.Replace("%ACCESSOR%", char.ToUpper(m_Accessor[0]) + m_Accessor.Substring(1))
				.Replace("%IS_VIRTUAL%", m_ReflectionInfo.IsVirtual ? "1" : "0")
				.Replace("%IS_STATIC%", m_ReflectionInfo.IsStatic ? "1" : "0")
				.Replace("%IS_CONST%", m_ReflectionInfo.IsConst ? "1" : "0")
				.Replace("%IS_CONSTRUCTOR%", m_IsConstructor ? "1" : "0")
				.Replace("%PARAM_COUNT%", "" + m_ReflectionInfo.ParamCount)
				.Replace("%SELECT_PARAM%", paramSelect)
				.Replace("%CALL_PARAMS%", callParams)
				.Replace("%DOC_STRING%", SafeDocString);
		}
	}
}
