#include "Include\Prism\Class.h"
#include "Include\Prism\Method.h"

namespace Prism 
{
	Class::Class(long uniqueId, const String& space, const String& name, size_t size, const std::vector<const Method*>& methods)
		: Type(uniqueId, space, name, size)
		, m_Methods(methods)
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
}