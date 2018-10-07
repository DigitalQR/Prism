#include "Include\Prism\Method.h"

namespace Prism
{
	Method::Method(const String& name, const Type* owningType, const ParamInfo& returnType, const std::vector<ParamInfo>& paramTypes)
		: m_Name(name)
		, m_OwningType(owningType)
		, m_ReturnType(returnType)
		, m_ParamTypes(paramTypes)
		, m_IsStatic(false)
	{
	}
	
	bool Method::AreValidParams(Prism::Holder target, const std::vector<Prism::Holder>& params) const 
	{
		if (!m_IsStatic && !target.GetTypeInfo() || target.GetTypeInfo()->Get() != m_OwningType)
			return false;

		if (params.size() != GetParamCount())
			return false;
				
		for (int i = 0; i < GetParamCount(); ++i)
		{
			const Prism::ParamInfo& param = GetParamInfo(i);

			const bool diffType = param.TypeInfo != params[i].GetTypeInfo();
			const bool diffPtr = param.IsPointer != params[i].IsPointer();

			if (diffType || diffPtr)
				return false;
		}

		return true;
	}
}