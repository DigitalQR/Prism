#include "Include\Prism\Property.h"

namespace Prism 
{
	Property::Property(const String& name, const String& documentation, const std::vector<const Attribute*>& attributes, bool isPointer, bool isStatic, bool isConst)
		: AttributeStore(attributes)
		, m_Name(name)
		, m_Documentation(documentation)
		, m_IsPointer(isPointer)
		, m_IsStatic(isStatic)
		, m_IsConst(isConst)
	{
	}
}