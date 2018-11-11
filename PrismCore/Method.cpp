#include "Include\Prism\Method.h"

namespace Prism
{
	Method::Method(const String& name, const String& documentation, bool isStatic, bool isConst, bool isVirtual)
		: m_Name(name)
		, m_Documentation(documentation)
		, m_IsStatic(isStatic)
		, m_IsConst(isConst)
		, m_IsVirtual(isVirtual)
	{
	}
	
	bool Method::AreValidParams(Prism::Holder target, const std::vector<Prism::Holder>& params) const 
	{
		if ((!m_IsStatic && !target.GetTypeInfo().IsValid()) || target.GetTypeInfo() != GetParentInfo())
			return false;

		if (params.size() != GetParamCount())
			return false;
				
		for (int i = 0; i < (int)GetParamCount(); ++i)
		{
			const Prism::ParamInfo* param = GetParamInfo(i);

			const bool diffType = param->Type != params[i].GetTypeInfo();
			const bool diffPtr = param->IsPointer != params[i].IsPointer();

			if (diffType || diffPtr)
				return false;
		}

		return true;
	}
}