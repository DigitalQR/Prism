#include "Include\Prism\Method.h"
#include "Include\Prism\Type.h"

namespace Prism
{
	Method::Method(const String& name, const String& documentation, const std::vector<const Attribute*>& attributes, Accessor accessor, bool isStatic, bool isConst, bool isVirtual)
		: AttributeStore(attributes)
		, m_Name(name)
		, m_Documentation(documentation)
		, m_Accessor(accessor)
		, m_IsStatic(isStatic)
		, m_IsConst(isConst)
		, m_IsVirtual(isVirtual)
	{
	}
	
	bool Method::AreValidParams(Prism::Holder target, const std::vector<Prism::Holder>& params) const 
	{
		if (!m_IsStatic && !(target.GetTypeInfo().IsValid() && target.GetTypeInfo()->IsInstanceOf(GetParentInfo())))
			return false;
		
		if (params.size() != GetParamCount())
			return false;
				
		for (int i = 0; i < (int)GetParamCount(); ++i)
		{
			const Prism::ParamInfo* param = GetParamInfo(i);

			const bool diffType = !param->Type->IsInstanceOf(params[i].GetTypeInfo());
			const bool diffPtr = param->IsPointer != params[i].IsPointer();

			if (diffType || diffPtr)
				return false;
		}

		return true;
	}
}