#include "Include\Prism\Property.h"

namespace Prism 
{
	Property::Property(const String& name, bool isPointer, bool isStatic, bool isConst)
		: m_Name(name)
		, m_IsPointer(isPointer)
		, m_IsStatic(isStatic)
		, m_IsConst(isConst)
	{
	}
}