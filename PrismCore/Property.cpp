#include "Include\Prism\Property.h"

namespace Prism 
{
	Property::Property(const String& name, const Type* owningType, const TypeInfo* type, bool isPointer, bool isStatic, bool isConst)
		: m_Name(name)
		, m_OwningType(owningType)
		, m_TypeInfo(type)
		, m_IsPointer(isPointer)
		, m_IsStatic(isStatic)
		, m_IsConst(isConst)
	{
	}
}