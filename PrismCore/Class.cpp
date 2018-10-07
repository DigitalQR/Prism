#include "Include\Prism\Class.h"
#include "Include\Prism\Method.h"
#include "Include\Prism\Property.h"

namespace Prism 
{
	Class::Class(long uniqueId, const String& space, const String& name, size_t size, const std::vector<const Method*>& methods, const std::vector<const Property*>& properties)
		: Type(uniqueId, space, name, size, true)
		, m_Methods(methods)
		, m_Properties(properties)
	{
	}

	const Method* Class::GetMethodByName(const String& name) const 
	{
		for (const Method* method : m_Methods)
		{
			if (method->GetName() == name)
				return method;
		}

		return nullptr;
	}

	const Property* Class::GetPropertyByName(const String& name) const
	{
		for (const Property* property : m_Properties)
		{
			if (property->GetName() == name)
				return property;
		}

		return nullptr;
	}
}